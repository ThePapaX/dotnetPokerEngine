using GameService;
using GameService.Models;
using NUnit.Framework;

namespace GameServiceTests
{
    public class CardTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DefaultCardValuesAreZeroValue()
        {
            var card = new Card();
            Assert.AreEqual((CardRank)0, card.Rank);
            Assert.AreEqual(CardSuit.None, card.Suit);
        }

        [Test]
        public void CanBeInstantiatedWithValues()
        {
            var card = new Card(CardRank.Ace, CardSuit.Club);
            Assert.AreEqual(CardSuit.Club, card.Suit);
            Assert.AreEqual(CardRank.Ace, card.Rank);
        }
    }
}