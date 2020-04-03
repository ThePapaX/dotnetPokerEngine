using System.Threading.Tasks;

namespace GameService.Models
{
    public interface IPlayerClient
    {
        Task ExecutePlayerAction(GameEvent action);
    }
}