using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameService.Models
{
    public interface IGame
    {
        public GamePlayer LastAgressor { get; }
        public Table Table { get; }
        public List<Card> BoardCards { get; }
        public double CurrentPotSize { get; }

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