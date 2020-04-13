using GameService.Hubs;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
using PokerEvaluatorClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GameService.Models
{
    [MessagePackObject]
    public class Game : IGame, IPokerHubServer
    {
        private readonly IHubContext<PokerHub> _hubContext;
        private readonly IPokerEvaluator _pokerEvaluator;

        public Table Table { get; private set; }

        public LinkedListNode<GamePlayer> CurrentActionOn { get; private set; }
        public GamePlayer LastAgressor { get; private set; }
       
        public double CurrentPotSize { get; private set; }
        public List<Card> BoardCards { get; set; }
        private CardDeck CardDeck { get; set; }

        private readonly HandStageHelper HandState;

        private CancellationTokenSource PlayerTimerCancellationTokenSource;

        public Game(IHubContext<PokerHub> hubContext, IPokerEvaluator pokerEvaluator, TableConfiguration tableConfig) //TODO: add Dependency Injection for handLogger here, redis, and other services
        {
            _hubContext = hubContext;
            _pokerEvaluator = pokerEvaluator;

            Table = new Table(tableConfig);
            BoardCards = new List<Card>(5);
            HandState = new HandStageHelper();
        }
        private void AddBetToPot(double bet) => CurrentPotSize += bet;
        public Game(IHubContext<PokerHub> hubContext, IPokerEvaluator pokerEvaluator) : this(hubContext, pokerEvaluator, new TableConfiguration()) { }

        #region PlayerActions

        
        private void Raise(double betSize, string playerId)
        {
            var player = Table.PlayersMap[playerId].Value;
            player.Bet(betSize);
            AddBetToPot(betSize);

            LastAgressor = player;
        }

        private void Call(double betSize, string playerId)
        {
            var player = Table.PlayersMap[playerId].Value;
            player.Call(betSize);
            AddBetToPot(betSize);

        }

        private void Check(string playerId) { }

        private void Fold(string playerId)
        {
            var player = Table.PlayersMap[playerId].Value;
            player.ActiveInHand = false;
            
        }

        public async Task ProcessClientAction(PlayerEvent playerAction)
        {
            //TODO: validate that the player can perform the given action. E.G Can't check and Can't call if someone has raised. 
            switch (playerAction.EventType)
            {
                //case PlayerActionType.PostSmallBlind:
                //    Raise(Table.TableConfig.SmallBlindSize, playerAction.PlayerId);
                //    await DispatchPlayerAction(playerAction);
                //    break;

                //case PlayerActionType.PostBigBlind:
                //    Raise(Table.TableConfig.BigBlindSize, playerAction.PlayerId);
                //    await DispatchPlayerAction(playerAction);
                //    break;

                case PlayerActionType.Call:
                    Call(playerAction.BetSize, playerAction.PlayerId);
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.Raise:
                    Raise(playerAction.BetSize, playerAction.PlayerId);
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.Fold:
                    Fold(playerAction.PlayerId);
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.Check:
                    Check(playerAction.PlayerId);
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.ShowCards:
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.MockCards:
                    await DispatchPlayerAction(playerAction);
                    break;

                default:
                    // Throw an exception here or dispatch an error message to the clients trying to hack the game or something funny
                    Debug.WriteLine($"PLAYER INVALID ACTION ${playerAction.PlayerId} ${playerAction.EventType}");
                    break;
            }
        }

        #endregion PlayerActions

        public async Task AddPlayer(GamePlayer player)
        {
            Table.AddPlayer(player);

            var gameEvent = new GameEvent()
            {
                EventType = GameActionType.PlayerJoined,
                Data = player
            };

            await DispatchGameEvent(gameEvent);
        }

        public async Task RemovePlayer(GamePlayer player)
        {
            Table.RemovePlayer(player);
            var gameEvent = new GameEvent()
            {
                EventType = GameActionType.PlayerLeft,
                Data = new { PlayerId = player.Id }
            };

            await DispatchGameEvent(gameEvent);
        }

        public async Task RemovePlayer(string playerId)
        {
            if (Table.PlayersMap.ContainsKey(playerId))
            {
                var playerNode = Table.PlayersMap[playerId];
                await RemovePlayer(playerNode.Value);
            }
            
        }

        public async Task UpdateDealerButton()
        {
            Table.MoveDealerButton();

            var gameEvent = new GameEvent()
            {
                EventType = GameActionType.UpdateDealerButton,
                Data = Table.Dealer.Value.Id
            };

            await DispatchGameEvent(gameEvent);
        }

        public bool UpdateNextActionOn()
        {

            CurrentActionOn = Table.GetNextActivePlayerNode(CurrentActionOn);

            if(CurrentActionOn.Value == LastAgressor || CurrentActionOn == null)
            {
                return false; // No More actions. No more active players = dead hand
            }

            return true;
        }

        public async Task StartGame() { 
            await StartNewHand();
            
            Table.MoveDealerButton();
            Table.MoveBlindsPointers();
            
            await CollectSmallBlind();
            await CollectBigBlind();

            while (Table.ActivePlayersCount() > 0 && HandState.GetCurrentStage() != HandStage.Finished)
            {
                
                await Deal();
                await RunActionLoop();
                HandState.MoveNext();

            }

            // asess game state, means that the hand is over
            // send even to let players show or mock their cards if applicable
            // award pot
            // start a new game

            
        }

        private async Task RunActionLoop()
        {
            Table.SetFirstToAct(HandState.GetCurrentStage());
            LastAgressor = Table.CurrentActionOn.Value;

            CurrentActionOn = Table.CurrentActionOn; 

            while(true) // Breaks on No more actions condition
            {
                // wait for player action, this action should be cancelled by an action executed by the player
                PlayerTimerCancellationTokenSource = new CancellationTokenSource();
                try
                {
                    await WaitForAction(PlayerTimerCancellationTokenSource);

                    var playerTimeoutEvent = new GameEvent()
                    {
                        EventType = GameActionType.PlayerTimedOut,
                        Data = new { PlayerId = CurrentActionOn.Value.Id }
                    };

                    await DispatchGameEvent(playerTimeoutEvent);
                }
                catch (TaskCanceledException)
                {
                    // Player did action, player action is handled separatedly
                    Debug.WriteLine("CONTINUING ACTION LOOP AFTER TIMEOUT TASK WAS CANCELLED");
                }

                UpdateNextActionOn();

                if (CurrentActionOn.Value == LastAgressor) break;
            }
            
        }
        private void InvokePlayersNewHand()
        {
            foreach (var player in Table.Players)
            {
                player.NewHand();
            }
        }

        public async Task StartNewHand()
        {
            //TODO: add check that the current hand is Not Active before resetting it.
            InvokePlayersNewHand();
            HandState.Reset();
            CurrentPotSize = 0;
            CardDeck = new CardDeck();
            LastAgressor = null;

            var newHandEvent = new GameEvent()
            {
                EventType = GameActionType.NewHand,
                Data = new
                {
                    HandId = Guid.NewGuid().ToString()
                }
            };

            await DispatchGameEvent(newHandEvent);
        }

        public async Task<bool> WaitForAction(CancellationTokenSource source)
        {
            await Task.Delay((int)Table.TableConfig.PlayerTimeout * 1000, source.Token);
            
            return false;

        }

        private async Task CollectSmallBlind()
        {
            var smallBlindPlayer = Table.SmallBlind.Value;
            smallBlindPlayer.Call(Table.TableConfig.SmallBlindSize);

            var smallBlindEvent = new GameEvent()
            {
                EventType = GameActionType.CollectSmallBlind,
                Data = new
                {
                    Size = Table.TableConfig.SmallBlindSize,
                    PlayerID = smallBlindPlayer.Id
                }
            };

            await DispatchGameEvent(smallBlindEvent);
        }

        private async Task CollectBigBlind()
        {
            var bigBlindPlayer = Table.BigBlind.Value;
            bigBlindPlayer.Call(Table.TableConfig.BigBlindSize);

            var bigBlindEvent = new GameEvent()
            {
                EventType = GameActionType.CollectBigBlind,
                Data = new
                {
                    Size = Table.TableConfig.BigBlindSize,
                    PlayerID = Table.BigBlind.Value.Id
                }
            };
            await DispatchGameEvent(bigBlindEvent);
        }

        public async Task DispatchGameEvent(GameEvent gameEventData)
        {
            await _hubContext.Clients.All.SendAsync(ClientInvokableMethods.GameEvent, gameEventData);
        }

        public async Task DispatchPlayerCard(GameEvent gameEventData)
        {
            await _hubContext.Clients.User(gameEventData.Data.PlayerId).SendAsync(ClientInvokableMethods.GameEvent, gameEventData);

            gameEventData.Data.Card = new Card();
            await _hubContext.Clients.AllExcept(gameEventData.Data.PlayerId).SendAsync(ClientInvokableMethods.GameEvent, gameEventData);
        }

        public async Task DispatchPlayerAction(PlayerEvent playerAction)
        {
            await _hubContext.Clients.All.SendAsync(ClientInvokableMethods.PlayerAction, playerAction);
        }

        public async Task Deal()
        {
            switch (HandState.GetCurrentStage())
            {
                case Models.HandStage.Preflop:
                    var currentPlayer = Table.SmallBlind;
                    var cardsDealtPerPlayer = 0;

                    while (!(cardsDealtPerPlayer == 2))
                    {
                        if (currentPlayer == Table.Dealer) cardsDealtPerPlayer++;
                        
                        var playerCard = CardDeck.GetNextCard();

                        var dealCardEvent = new GameEvent()
                        {
                            EventType = GameActionType.DealCard,
                            Data = new { 
                                PlayerId = currentPlayer.Value.Id, 
                                Card = playerCard
                            }
                        };

                        await DispatchPlayerCard(dealCardEvent);

                        currentPlayer = Table.GetNextActivePlayerNode(currentPlayer);
                    }

                    break;

                case Models.HandStage.Flop:
                    _ = CardDeck.GetNextCard();
                    List<Card> cardsToDeal = CardDeck.GetNextCards(3);
                    
                    BoardCards.AddRange(cardsToDeal);

                    await DispatchGameEvent(new GameEvent() { EventType = GameActionType.DealFlop, Data = cardsToDeal });

                    break;

                case Models.HandStage.Turn:
                    _ = CardDeck.GetNextCard();
                    var turnCard = CardDeck.GetNextCard();
                    BoardCards.Add(turnCard);

                    await DispatchGameEvent(new GameEvent() { EventType = GameActionType.DealTurn, Data = turnCard });

                    break;

                case Models.HandStage.River:
                    _ = CardDeck.GetNextCard();
                    var riverCard = CardDeck.GetNextCard();
                    BoardCards.Add(riverCard);
                    await DispatchGameEvent(new GameEvent() { EventType = GameActionType.DealTurn, Data = riverCard });

                    break;

                default:
                    break;
            }
        }
    }
}