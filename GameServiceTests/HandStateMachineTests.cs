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
            var stateMachine = new HandStateMachine();
            Assert.Pass();
        }

        [Test]
        public void TheInitialStateIsPreFlop()
        {
            var stateMachine = new HandStateMachine();
            Assert.AreEqual(HandState.Preflop, stateMachine.GetCurrentStateValue());
        }

        [Test]
        public void MoveNextReturnTheNextStateInOrder()
        {
            var stateMachine = new HandStateMachine();
            var nextState = stateMachine.MoveNext();

            Assert.AreEqual(HandState.Flop, stateMachine.GetCurrentStateValue());
            Assert.AreEqual(HandState.Flop, nextState);

            nextState = stateMachine.MoveNext();
            Assert.AreEqual(HandState.Turn, stateMachine.GetCurrentStateValue());
            Assert.AreEqual(HandState.Turn, nextState);

            nextState = stateMachine.MoveNext();
            Assert.AreEqual(HandState.River, stateMachine.GetCurrentStateValue());
            Assert.AreEqual(HandState.River, nextState);
        }

        [Test]
        public void MoveNextAfterRiverReturnsInvalidOperationExpection()
        {
            var stateMachine = new HandStateMachine();
            var nextState = stateMachine.MoveNext();
            nextState = stateMachine.MoveNext();
            nextState = stateMachine.MoveNext();
            Assert.Throws(Is.TypeOf<InvalidOperationException>(), delegate { stateMachine.MoveNext(); });
        }

        [Test]
        public void HandCanBeReset()
        {
            var stateMachine = new HandStateMachine();
            stateMachine.MoveNext();
            stateMachine.MoveNext();
            stateMachine.Reset();

            Assert.AreEqual(HandState.Preflop, stateMachine.GetCurrentStateValue());

        }
    }
}
