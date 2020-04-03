using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameService.Models
{
    public interface IGame
    {
        public LinkedList<GamePlayer> Players { get; }
        public LinkedListNode<GamePlayer> CurrentActionOn { get; }
        public GamePlayer LastAgressor { get; }
        public LinkedListNode<GamePlayer> DealerButtonOn { get; }
        public Dictionary<string, LinkedListNode<GamePlayer>> PlayersMap { get; }
        public List<Card> Board { get; set; }
        public uint CurrentPotSize { get; }

        Task AddPlayer(GamePlayer player);

        Task RemovePlayer(GamePlayer player);

        Task RemovePlayer(string playerID);

        Task StartGame();

        Task StartNewHand();

        Task Deal();

        Task UpdateDealerButton();

        bool UpdateNextActionOn();

        Task ProcessClientAction(PlayerEvent playerAction);
        Task DispatchGameEvent(GameEvent gameEvent);
    }
}