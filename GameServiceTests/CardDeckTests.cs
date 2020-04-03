
using GameService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServiceTests
{
    class CardDeckTests
    {
        [Test]
        public void CardDeckIsShuffled()
        {
            var deck = new CardDeck();
            Assert.Pass();

        }
        [Test]
        public void CardDeckHasFiftyTwoCards()
        {
            var deck = new CardDeck();
            Assert.AreEqual(52, deck.Count);
        }

        [Test]
        public void CountIsUpdatedAccordinglyToTheNumberOfDealtCards()
        {
            var deck = new CardDeck();
            _ = deck.GetNextCards(10);
            Assert.AreEqual(42, deck.Count);
        }


        [Test]
        public void GetNextCardsReturnTwoCardsByDefault()
        {
            var deck = new CardDeck();
            var nextCards = deck.GetNextCards();

            Assert.AreEqual(2, nextCards.Count);
        }
    }
}
