using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Models
{
    public interface IPokerHubServer
    {
        Task ProcessClientAction(PlayerEvent playerAction);
        Task DispatchGameEvent(GameEvent gameEvent);
    }
}
