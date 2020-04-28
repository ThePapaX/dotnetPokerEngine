using GameService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameService.Hubs
{
    [Authorize]
    public class PokerHub : Hub, IPokerHubClient
    {
        private IGame _game;

        public PokerHub(IGame gameState)
        {
            _game = gameState;
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Context.User.FindFirst(ClaimTypes.Email).Value, message);
        }

        public async Task ExecutePlayerAction(PlayerEvent playerAction)
        {
            await _game.ProcessClientAction(playerAction);
        }

        public override async Task OnConnectedAsync()
        {
            var playerId = Context.User.FindFirst("Id").Value;
            var player = new GamePlayer()
            {
                Id = playerId
            };

            await _game.AddPlayer(player);

            await base.OnConnectedAsync();
        }

        /**
         * If the client disconnects intentionally (by calling connection.stop(), for example),
         * the exception parameter will be null. However, if the client is disconnected due to an
         * error (such as a network failure), the exception parameter will contain an exception describing the failure.
         */

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception == null)
            {
                await _game.RemovePlayer(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}