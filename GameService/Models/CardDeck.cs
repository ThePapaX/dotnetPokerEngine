using GameService.Models;
using GameService.Utilities;
using System;
using System.Collections.Generic;

namespace GameService
{
    public class CardDeck
    {
        //TODO: perhaps we can just an array of CARDS to do an in place shuffle ?
        private Stack<Card> Cards;

        private Stack<Card> DealtCards;

        public CardDeck()
        {
            var cards = GenerateCardDeck();

            cards.Shuffle();

            Cards = new Stack<Card>(cards);
            DealtCards = new Stack<Card>();
        }

        public int Count => Cards.Count;
        public int DealtCardsCount => DealtCards.Count;

        public Card GetNextCard()
        {
            var card = Cards.Pop();
            DealtCards.Push(card);
            return card;
        }

        public List<Card> GetNextCards(int count = 2)
        {
            var nextCards = new List<Card>();

            for (int i = 0; i < count; i++)
            {
                nextCards.Add(GetNextCard());
            }

            return nextCards;
        }
        

        private static Card[] GenerateCardDeck()
        {
            var cardStack = new Card[52];

            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
            {
                if (rank == CardRank.None) continue;

                var index = (int)rank - 1;
                cardStack[index] = new Card(rank, CardSuit.Heart);
                cardStack[index + 13] = new Card(rank, CardSuit.Spade);
                cardStack[index + 26] = new Card(rank, CardSuit.Diamond);
                cardStack[index + 39] = new Card(rank, CardSuit.Club);
            }
            
            return cardStack;
        }
        
    }
}