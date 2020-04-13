using GameService.Hubs;
using GameService.Models;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using PokerEvaluator;
using PokerEvaluatorClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameServiceTests
{
    [TestFixture]
    internal class GameTest
    {
        private static (Mock<IHubContext<PokerHub>>, Mock<IClientProxy>) GetMockPokerHubContext()
        {
            var mockHubContext = new Mock<IHubContext<PokerHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
            mockHubContext.Setup(context => context.Clients).Returns(mockClients.Object);

            return (mockHubContext, mockClientProxy);
        }
        private static Mock<IPokerEvaluator> GetMockPokerEvaluator()
        {
            var mock = new Mock<IPokerEvaluator>();
            mock.Setup(mc => mc.EvaluateBoard(It.IsAny<string>())).Returns(new EvaluationResult());

            return mock;
        }
        private static (Game, Mock<IHubContext<PokerHub>>, Mock<IClientProxy>) GetMocks() {
            (var mockHubContext, var mockClientProxy) = GetMockPokerHubContext();
            var mockPokerEvaluator = GetMockPokerEvaluator();
            var mockGame = new Game(mockHubContext.Object, mockPokerEvaluator.Object);

            return (mockGame, mockHubContext, mockClientProxy);
        }

        [SetUp]
        public void Setup()
        {
            //hubContext.Clients.All.SendAsync(ClientInvokableMethods.GameEvent, gameEventData);
        }

        [Test]
        public void GameCanBeInstantiated()
        {
            // Arrange
            (var game ,var mockHubContext, var mockClientProxy) = GetMocks();

            Assert.Pass();
        }

        [Test]
        public async Task NewPlayersCanJoinTheGame()
        {
            // Arrange
            (var game, var mockHubContext, var mockClientProxy) = GetMocks();
            var newPlayer = new GamePlayer()
            {
                Id = "test_player"
            };

            // Act

            await game.AddPlayer(newPlayer);

            // Assert

            Assert.AreEqual(game.Table.Players.Count, 1);
            Assert.AreEqual(game.Table.Players.First.Value.Id, newPlayer.Id);
            Assert.AreEqual(game.Table.PlayersMap[newPlayer.Id].Value, newPlayer);
        }

        [Test]
        public async Task WhenAPlayerJoinsAllPlayersAreNotified()
        {
            // Arrange
            (var game, var mockHubContext, var mockClientProxy) = GetMocks();

            var players = new List<GamePlayer>() {
                new GamePlayer()
                {
                    Id = "test_player"
                },
                new GamePlayer()
                {
                    Id = "test_player2"
                }
            };
            
            // Act

            await game.AddPlayer(players[0]);
            await game.AddPlayer(players[1]);

            // Assert

            mockClientProxy.Verify(clientProxy => clientProxy.SendCoreAsync(ClientInvokableMethods.GameEvent, It.Is<object[]>(obj => obj != null && obj.Length == 1 && ((GameEvent)obj[0]).EventType == GameActionType.PlayerJoined), default), Times.Exactly(2));
        }

        [Test]
        public async Task UpdatesTheDealerButtonCorrectly()
        {
            // Arrange
            (var game, var mockHubContext, var mockClientProxy) = GetMocks();
            var player = new GamePlayer() { Id = "test_player" };
            await game.AddPlayer(player);
            await game.AddPlayer(new GamePlayer() { Id = "test_player2" });

            // Act
            await game.UpdateDealerButton();

            // Assert
            Assert.AreEqual(game.Table.Dealer.Value, player);
            mockClientProxy.Verify(clientProxy => clientProxy.SendCoreAsync(ClientInvokableMethods.GameEvent, It.Is<object[]>(obj => obj != null && obj.Length == 1 && ((GameEvent)obj[0]).EventType == GameActionType.UpdateDealerButton), default), Times.Once);

        }
    }
}