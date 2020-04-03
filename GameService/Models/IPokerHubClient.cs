using System.Threading.Tasks;

namespace GameService.Models
{
    public interface IPokerHubClient
    {
        Task ExecutePlayerAction(PlayerEvent action);
        Task SendMessage(string senderId, string message);
    }
}