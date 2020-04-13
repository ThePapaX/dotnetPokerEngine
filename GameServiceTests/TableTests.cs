using GameService.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServiceTests
{
    [TestFixture]
    class TableTests
    {
        private static Table GetTableWithPlayers(int playerCount = 4)
        {
            var table = new Table();
            while (playerCount-- != 0)
            {
                table.AddPlayer(new GamePlayer() { Id = Guid.NewGuid().ToString(), ActiveInHand = true });
            }
            return table;
        }
        [Test]
        public void CanBeInstanstiatedWithNoParameters()
        {
            var table = new Table();
            Assert.IsTrue(table.TableConfig.StartingChipCount > 0);
        }
        [Test]
        public void CanBeInstanstiatedWithConfiguration()
        {
            var tableConfig = new TableConfiguration(1000, 2000, 30, 100000);
            var table = new Table(tableConfig);
            Assert.AreEqual(table.TableConfig.BigBlindSize, tableConfig.BigBlindSize);
            Assert.AreEqual(table.TableConfig.SmallBlindSize, tableConfig.SmallBlindSize);
            Assert.AreEqual(table.TableConfig.PlayerTimeout, tableConfig.PlayerTimeout);
            Assert.AreEqual(table.TableConfig.StartingChipCount, tableConfig.StartingChipCount);
        }

        [Test]
        public void CanAddPlayers()
        {
            // Arrange 
            var table = new Table();
            var player = new GamePlayer() {  Id = "123" };

            // Act
            table.AddPlayer(player);

            // Assert
            Assert.AreEqual(table.Players.Count, 1);
            Assert.AreEqual(table.PlayersMap.Count, 1);
            
        }
        [Test]
        public void CanRemovePlayer()
        {
            // Arrange 
            var table = new Table();
            var player = new GamePlayer() { Id = "123" };
            table.AddPlayer(player);

            // Act
            table.RemovePlayer(player);
            // Assert
            Assert.AreEqual(table.Players.Count, 0);
            Assert.AreEqual(table.PlayersMap.Count, 0);

        }
        [Test]
        public void CanRemovePlayerById()
        {
            // Arrange 
            var table = new Table();
            var player = new GamePlayer() { Id = "123" };
            table.AddPlayer(player);

            // Act
            table.RemovePlayer(player.Id);
            // Assert
            Assert.AreEqual(table.Players.Count, 0);
            Assert.AreEqual(table.PlayersMap.Count, 0);

        }
        [Test]
        public void CantRemoveAPlayerThatHasntBeenPrevioslyAdded()
        {
            // Arrange 
            var table = new Table();
            var player = new GamePlayer() { Id = "123" };
            var anotherPlayer = new GamePlayer { Id = "456" };
            table.AddPlayer(player);

            // Act
            table.RemovePlayer("non_existing_id");
            table.RemovePlayer(anotherPlayer);

            // Assert
            Assert.AreEqual(table.Players.Count, 1);
            Assert.AreEqual(table.PlayersMap.Count, 1);

        }

        [Test]
        public void DealerButtonIsSetToTheFirstPlayerOnTheTableByDefault()
        {
            // Arrange 
            var table = GetTableWithPlayers();

            // Act
            table.MoveDealerButton();

            // Assert
            Assert.AreEqual(table.Players.First, table.Dealer);

        }

        [Test]
        public void DealerButtonIsSetCorrectly_WhenThereAreAbsentPlayersOnTheTable()
        {
            // Arrange 
            var table = GetTableWithPlayers(3);
            table.Players.First.Next.Value.ActiveInHand = false;
            table.Dealer = table.Players.First;

            // Act
            table.MoveDealerButton();

            // Assert
            Assert.AreEqual(table.Players.Last, table.Dealer);

        }

        [Test]
        public void DealerButtonIsSetCorrectly_EdgeCase_LastPlayerOnTheLinkedList()
        {
            // Arrange 
            var table = GetTableWithPlayers();
            table.Dealer = table.Players.Last;

            // Act
            table.MoveDealerButton();

            // Assert
            Assert.AreEqual(table.Players.First, table.Dealer);

        }

        [Test]
        public void BlindPointersAreSetCorrectly()
        {
            // Arrange 
            var table = GetTableWithPlayers();
            table.MoveDealerButton();

            // Act
            table.MoveBlindsPointers();

            // Assert
            Assert.AreEqual(table.SmallBlind, table.Dealer.Next);
            Assert.AreEqual(table.BigBlind, table.Dealer.Next.Next);

        }

        [Test]
        public void BlindPointersAreSetCorrectly_EdgeCase_TwoPlayersOnly_DealerIsBigBlindToo()
        {
            // Arrange 
            var table = GetTableWithPlayers(2);
            table.MoveDealerButton();

            // Act
            table.MoveBlindsPointers();

            // Assert
            Assert.AreEqual(table.SmallBlind, table.Dealer.Next);
            Assert.AreEqual(table.BigBlind, table.Dealer);

        }

        [Test]
        public void BlindPointersAreSetCorrectly_EdgeCase_TwoPlayers_DealerIsOnTheLastPlayer()
        {
            // Arrange 
            var table = GetTableWithPlayers(2);
            table.Dealer = table.Players.First;
            table.MoveDealerButton(); // Dealer is the seat 2.

            // Act
            table.MoveBlindsPointers();

            // Assert
            Assert.AreEqual(table.SmallBlind, table.Players.First);
            Assert.AreEqual(table.BigBlind, table.Dealer);

        }
        [Test]
        public void FirstToActOnPreFlopIsThePlayerNextToBigBlind()
        {
            // Arrange 
            var table = GetTableWithPlayers();
            table.MoveDealerButton();
            table.MoveBlindsPointers();

            // Act
            table.SetFirstToAct(HandStage.Preflop);

            // Assert
            Assert.AreEqual(table.CurrentActionOn, table.BigBlind.Next); 

        }

        [Test]
        public void FirstToActOnPreFlopIsThePlayerNextToBigBlind_EdgeCase()
        {
            // Arrange 
            var playerCount = 3;
            var table = GetTableWithPlayers(playerCount);
            table.MoveDealerButton();
            table.MoveBlindsPointers();

            // Act
            table.SetFirstToAct(HandStage.Preflop);

            // Assert
            Assert.AreEqual(table.CurrentActionOn, table.Players.First); // Because the player count is 3.

        }

        [Test]
        public void FirstToActOnStagesOtherThanPreflopIsFirstActivePlayerNextToTheDealer()
        {
            // Arrange 
            var table = GetTableWithPlayers();
            table.MoveDealerButton();
            table.MoveBlindsPointers();

            // Act
            table.SetFirstToAct(HandStage.Flop);

            // Assert
            Assert.AreEqual(table.CurrentActionOn, table.Dealer.Next);

            // Act
            var nextToDealerNode = table.Dealer.Next;
            nextToDealerNode.Value.ActiveInHand = false; // Emulate a fold 
            var expectedActivePlayer = nextToDealerNode.Next;

            table.SetFirstToAct(HandStage.Turn);
            // Assert
            Assert.AreEqual(table.CurrentActionOn, expectedActivePlayer);

            // Act
            table.SetFirstToAct(HandStage.River);
            // Assert
            Assert.AreEqual(table.CurrentActionOn, expectedActivePlayer);

        }
    }
}
