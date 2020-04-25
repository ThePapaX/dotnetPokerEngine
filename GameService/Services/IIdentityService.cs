using PokerClassLibrary;
using System;
using System.Threading.Tasks;

namespace GameService.Services
{
    public interface IIdentityService
    {
        Task<Player> Authenticate(string username, string password);
        Task<bool> IsAuthenticated(Guid playerId);
        Task Logout(Guid playerId);
    }
}
