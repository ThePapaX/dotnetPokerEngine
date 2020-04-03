using GameService.Hubs;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
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
        public LinkedList<GamePlayer> Players { get; private set; }

        [IgnoreMember]
        [JsonIgnore]
        public Dictionary<string, LinkedListNode<GamePlayer>> PlayersMap { get; private set; }

        public LinkedListNode<GamePlayer> CurrentActionOn { get; private set; }
        public GamePlayer LastAgressor { get; private set; }
        public LinkedListNode<GamePlayer> DealerButtonOn { get; private set; }
        public TableConfiguration TableConfig { get; private set; }
        public uint CurrentPotSize { get; private set; }
        private void AddBetToPot(uint bet) => CurrentPotSize += bet;
        public List<Card> Board { get; set; }
        private CardDeck CardDeck { get; set; }

        private readonly HandStateMachine HandState;

        private CancellationTokenSource PlayerTimerCancellationTokenSource;

        public Game(IHubContext<PokerHub> hubContext, TableConfiguration tableConfig) //TODO: add Dependency Injection for handLogger here, redis, and other services
        {
            TableConfig = tableConfig;
            _hubContext = hubContext;
            Players = new LinkedList<GamePlayer>();
            PlayersMap = new Dictionary<string, LinkedListNode<GamePlayer>>();
            Board = new List<Card>(5);
            HandState = new HandStateMachine();
        }

        public Game(IHubContext<PokerHub> hubContext) : this(hubContext, new TableConfiguration()) { }

        #region PlayerActions

        private void Raise(uint betSize, string playerId)
        {
            AddBetToPot(betSize);
            LastAgressor = PlayersMap[playerId].Value;
        }

        private void Check(string playerId) => throw new NotImplementedException();

        private void Fold(string playerId)
        {
            var player = PlayersMap[playerId].Value;
            player.IsActiveInHand = false;
            
        }

        public async Task ProcessClientAction(PlayerEvent playerAction)
        {
            switch (playerAction.EventType)
            {
                case PlayerActionType.PostSmallBlind:
                    Raise(TableConfig.SmallBlindSize, playerAction.PlayerId);
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.PostBigBlind:
                    Raise(TableConfig.BigBlindSize, playerAction.PlayerId);
                    await DispatchPlayerAction(playerAction);
                    break;

                case PlayerActionType.Call:
                    AddBetToPot(playerAction.BetSize);
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
            player.SeatNumber = Players.Count + 1;
            player.CurrentStack = TableConfig.StartingChipCount; //TODO: Check if the player is reconnecting or something

            var playersNode = Players.AddLast(player);

            PlayersMap[player.Id] = playersNode;

            var gameEvent = new GameEvent()
            {
                EventType = GameActionType.PlayerJoined,
                Data = player
            };

            await DispatchGameEvent(gameEvent);
        }

        public async Task RemovePlayer(GamePlayer player)
        {
            Players.Remove(player);
            PlayersMap.Remove(player.Id);
            var gameEvent = new GameEvent()
            {
                EventType = GameActionType.PlayerLeft,
                Data = new { PlayerId = player.Id }
            };

            await DispatchGameEvent(gameEvent);
        }

        public async Task RemovePlayer(string playerId)
        {
            var playerNode = PlayersMap[playerId];
            if (playerNode != null)
            {
                await RemovePlayer(playerNode.Value);
            }
        }

        public async Task UpdateDealerButton()
        {
            DealerButtonOn = DealerButtonOn?.Next ?? Players.First;

            var gameEvent = new GameEvent()
            {
                EventType = GameActionType.UpdateDealerButton,
                Data = DealerButtonOn.Value.Id
            };

            await DispatchGameEvent(gameEvent);
        }

        public bool UpdateNextActionOn()
        {
            if(CurrentActionOn.Next == null)
            {
                CurrentActionOn = Players.First;
            }
            else
            {
                CurrentActionOn = CurrentActionOn.Next;
            }

            if (CurrentActionOn.Value == LastAgressor || (CurrentActionOn == DealerButtonOn && LastAgressor == null))
            {
                // No More actions
                return false;
            }

            if (!CurrentActionOn.Value.IsActiveInHand)
            {
                return UpdateNextActionOn();
            }

            return true;
        }

        public async Task StartGame() { 
            await StartNewHand();

            //Pre flop
            //await RunPreFlop();
            CurrentActionOn = DealerButtonOn;
            
            _ = UpdateNextActionOn();
            await CollectSmallBlind();

            _ = UpdateNextActionOn();
            await CollectBigBlind();

            var canDeal = UpdateNextActionOn();

            while (canDeal)
            {
                await Deal();
                await RunActionLoop();
                HandState.MoveNext();

                CurrentActionOn = DealerButtonOn;
                canDeal = UpdateNextActionOn();
            }

            // asess game state, means that the hand is over
            // send even to let players show or mock their cards if applicable
            // award pot
            // start a new game

            
        }
        private bool NoMoreActions => LastAgressor != CurrentActionOn.Value;
        private async Task RunActionLoop()
        {
            while(!NoMoreActions) //No more action condition
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
            }
        }
        private void SetPlayersToActive()
        {
            foreach (var player in Players)
            {
                player.IsActiveInHand = !player.IsAbsent;
            }
        }

        public async Task StartNewHand()
        {
            //TODO: add check that the current hand is Not Active before resetting it.
            SetPlayersToActive();
            HandState.Reset();
            CurrentPotSize = 0;
            CardDeck = new CardDeck();
            LastAgressor = null;

            await UpdateDealerButton();
        }

        public async Task RunPreFlop()
        {
            CurrentActionOn = DealerButtonOn.Next;

            await CollectSmallBlind();
            UpdateNextActionOn();

            await CollectBigBlind();
            UpdateNextActionOn();

            //Deal Cards
            await Deal();
        }

        public async Task<bool> WaitForAction(CancellationTokenSource source)
        {
            await Task.Delay((int)TableConfig.PlayerTimeout * 1000, source.Token);
            
            return false;

        }

        private async Task CollectSmallBlind()
        {
            var smallBlindEvent = new GameEvent()
            {
                EventType = GameActionType.CollectSmallBlind,
                Data = new
                {
                    size = TableConfig.SmallBlindSize,
                    playerID = CurrentActionOn.Value.Id
                }
            };

            await DispatchGameEvent(smallBlindEvent);
        }

        private async Task CollectBigBlind()
        {
            var bigBlindEvent = new GameEvent()
            {
                EventType = GameActionType.CollectBigBlind,
                Data = new
                {
                    size = TableConfig.BigBlindSize,
                    playerID = CurrentActionOn.Value.Id
                }
            };
            await DispatchGameEvent(bigBlindEvent);
        }

        public async Task DispatchGameEvent(GameEvent gameEventData)
        {
            await _hubContext.Clients.All.SendAsync(ClientInvokableMethods.GameEvent, gameEventData);
        }

        public async Task DispatchPlayerAction(PlayerEvent playerAction)
        {
            await _hubContext.Clients.All.SendAsync(ClientInvokableMethods.PlayerAction, playerAction);
        }

        public async Task Deal()
        {
            switch (HandState.GetCurrentStateValue())
            {
                case Models.HandState.Preflop:
                    var nextPlayer = DealerButtonOn.Next;
                    var cardDealtPerPlayerCount = 1;

                    while (!(nextPlayer == DealerButtonOn && cardDealtPerPlayerCount == 2))
                    {
                        if (nextPlayer == DealerButtonOn) cardDealtPerPlayerCount++;

                        var standardMaskedCardEvent = new GameEvent()
                        {
                            EventType = GameActionType.DealCard,
                            Data = new { PlayerId = nextPlayer.Value.Id }
                        };

                        var playerCard = CardDeck.GetNextCard();
                        var encryptedPlayerCard = new GameEvent()
                        {
                            EventType = GameActionType.DealEncryptedCard,
                            Data = playerCard
                        };

                        await DispatchGameEvent(encryptedPlayerCard);
                        await DispatchGameEvent(standardMaskedCardEvent);

                        nextPlayer = nextPlayer.Next;
                    }

                    break;

                case Models.HandState.Flop:
                    _ = CardDeck.GetNextCard();
                    List<Card> cardsToDeal = CardDeck.GetNextCards(3);

                    await DispatchGameEvent(new GameEvent() { EventType = GameActionType.DealFlop, Data = cardsToDeal });

                    break;

                case Models.HandState.Turn:
                    _ = CardDeck.GetNextCard();
                    var turnCard = CardDeck.GetNextCard();

                    await DispatchGameEvent(new GameEvent() { EventType = GameActionType.DealTurn, Data = turnCard });

                    break;

                case Models.HandState.River:
                    _ = CardDeck.GetNextCard();
                    var riverCard = CardDeck.GetNextCard();
                    await DispatchGameEvent(new GameEvent() { EventType = GameActionType.DealTurn, Data = riverCard });

                    break;

                default:
                    break;
            }
        }
    }
}