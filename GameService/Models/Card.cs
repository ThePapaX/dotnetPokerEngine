using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Models
{
    public class Card
    {
        public CardSuit Suit { get; }
        public CardRank Rank { get; }
        public Card()
        {

        }
        public Card(CardRank rank, CardSuit suit)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}
