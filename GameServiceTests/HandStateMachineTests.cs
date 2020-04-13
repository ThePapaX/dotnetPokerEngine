using GameService.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServiceTests
{
    class HandStateMachineTests
    {
        [Test]
        public void CanBeInstantiated()
        {
            var stateMachine = new HandStageHelper();
            Assert.Pass();
        }

        [Test]
        public void TheInitialStateIsPreFlop()
        {
            var stateMachine = new HandStageHelper();
            Assert.AreEqual(HandStage.Preflop, stateMachine.GetCurrentStage());
        }

        [Test]
        public void MoveNextReturnTheNextStateInOrder()
        {
            var stateMachine = new HandStageHelper();
            var nextState = stateMachine.MoveNext();

            Assert.AreEqual(HandStage.Flop, stateMachine.GetCurrentStage());
            Assert.AreEqual(HandStage.Flop, nextState);

            nextState = stateMachine.MoveNext();
            Assert.AreEqual(HandStage.Turn, stateMachine.GetCurrentStage());
            Assert.AreEqual(HandStage.Turn, nextState);

            nextState = stateMachine.MoveNext();
            Assert.AreEqual(HandStage.River, stateMachine.GetCurrentStage());
            Assert.AreEqual(HandStage.River, nextState);

            nextState = stateMachine.MoveNext();
            Assert.AreEqual(HandStage.Finished, stateMachine.GetCurrentStage());
            Assert.AreEqual(HandStage.Finished, nextState);
        }

        [Test]
        public void MoveNextAfterRiverReturnsInvalidOperationExpection()
        {
            var stateMachine = new HandStageHelper();
            var nextState = stateMachine.MoveNext();
            nextState = stateMachine.MoveNext();
            nextState = stateMachine.MoveNext();
            nextState = stateMachine.MoveNext();
            Assert.Throws(Is.TypeOf<InvalidOperationException>(), delegate { stateMachine.MoveNext(); });
        }

        [Test]
        public void HandCanBeReset()
        {
            var stateMachine = new HandStageHelper();
            stateMachine.MoveNext();
            stateMachine.MoveNext();
            stateMachine.Reset();

            Assert.AreEqual(HandStage.Preflop, stateMachine.GetCurrentStage());

        }
    }
}
