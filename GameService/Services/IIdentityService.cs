using PokerClassLibrary;
using System;
using System.Threading.Tasks;

namespace GameService.Services
{
    public interface IIdentityService
    {
        public Task<Player> Authenticate(string username, string password);
        public Task<bool> IsAuthenticated(Guid playerId);
        public Task Logout(Guid playerId);
    }
}
