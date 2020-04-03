using GameService.Hubs;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GameService.Models
{
    [MessagePackObject]
    public class Game : IGame
    {

        private readonly IHubContext<PokerHub> _hubContext;
        public LinkedList<GamePlayer> Players { get; private set; }
        public LinkedListNode<GamePlayer> CurrentActionOn { get; private set; }
        public GamePlayer LastAgressor { get; private set; }
        public LinkedListNode<GamePlayer> DealerButtonOn { get; private set; }

        [IgnoreMember]
        [JsonIgnore]
        public Dictionary<string, LinkedListNode<GamePlayer>> PlayersMap { get; private set; }

        public GameConfig Configuration { get; private set; }
        public uint CurrentPotSize { get; private set; }
        public List<Card> Board { get; set; }
        private CardDeck CardDeck { get; set; }
        private readonly HandStateMachine HandState;

        public Game(IHubContext<PokerHub> hubContext, GameConfig configuration) //TODO: add Dependency Injection for handLogger here, redis, and other services
        {
            Configuration = configuration;
            _hubContext = hubContext;
            Players = new LinkedList<GamePlayer>();
            PlayersMap = new Dictionary<string, LinkedListNode<GamePlayer>>();
            Board = new List<Card>(5);
            HandState = new HandStateMachine();
        }

        public Game(IHubContext<PokerHub> hubContext) => new Game(hubContext, new GameConfig());

        #region PlayerActions
        private async void Raise(uint betSize, string playerId)
        {
            CurrentPotSize += betSize;
        }

        private async void Check(string playerId) => throw new NotImplementedException();

        private async void Fold()
        {
            CurrentActionOn.Value.IsActiveInHand = false;
        }

        public async Task ProcessClientAction(PlayerEvent playerAction)
        {
            switch (playerAction.EventType)
            {
                case PlayerActionType.PostSmallBlind:
                    break;
                case PlayerActionType.PostBigBlind:
                    break;
                case PlayerActionType.Call:
                    break;
                case PlayerActionType.Raise:
                    break;
                case PlayerActionType.Fold:
                    Fold();
                    break;
                case PlayerActionType.Check:
                    break;
                case PlayerActionType.ShowCards:
                    break;
                case PlayerActionType.MockCards:
                    break;

                default:
                    // Throw an exception here or dispatch an error message to the clients trying to hack the game or something funny
                    break;
            }
        }
        #endregion PlayerActions

        public async Task AddPlayer(GamePlayer player)
        {
            player.SeatNumber = Players.Count + 1;
            player.CurrentStack = Configuration.StartingChipCount; //TODO: Check if the player is reconnecting or something

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
            var gameEvent = new GameEvent() {
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

        public async Task UpdateDealerButton() {
            DealerButtonOn = DealerButtonOn?.Next ?? Players.First;

            var gameEvent = new GameEvent() { 
                EventType = GameActionType.UpdateDealerButton,
                Data = DealerButtonOn.Value.Id
            };

            await DispatchGameEvent(gameEvent);
        }

        public bool UpdateNextActionOn() {
            var currentPlayer = CurrentActionOn;
            while (!CurrentActionOn.Value.IsActiveInHand)
            {
                CurrentActionOn = CurrentActionOn.Next;
                if(CurrentActionOn == currentPlayer || CurrentActionOn.Value == LastAgressor)
                {
                    // No More actions
                    return false;
                }
            }
            return true;
            
        }

        public async Task StartGame() => await StartNewHand();

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

            await UpdateDealerButton();
            await RunPreFlop();

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
        public async Task WaitForAction(CancellationTokenSource source)
        {
            await Task.Delay((int)Configuration.PlayerTimeout * 1000, source.Token);
            
            var playerTimeoutEvent = new GameEvent()
            {
                EventType = GameActionType.PlayerTimedOut,
                Data = new { PlayerId = CurrentActionOn.Value.Id }
            };

            await DispatchGameEvent(playerTimeoutEvent);
        }
        private async Task CollectSmallBlind()
        {
            var smallBlindEvent = new GameEvent()
            {
                EventType = GameActionType.CollectSmallBlind,
                Data = new
                {
                    size = Configuration.SmallBlindSize,
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
                    size = Configuration.BigBlindSize,
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