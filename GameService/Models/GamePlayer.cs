using System;

namespace GameService.Models
{
    public class GamePlayer
    {
        public string Id { get; set; }
        public int SeatNumber { get; set; }

        public bool ActiveInHand { get; set; }
        public bool Absent { get; set; }
        public double CurrentBetSize { get; private set; }
        public double CurrentStack { get; private set; }

        internal Card[] PocketCards { get; set; }

        public GamePlayer()
        {
            SeatNumber = -1;
            ActiveInHand = false;
            Absent = false;
            CurrentBetSize = 0;
            CurrentStack = 0;
            PocketCards = new Card[2];
        }

        internal void SetCurrentStack(double amount)
        {
            CurrentStack = amount;
        }

        internal void Bet(double amount)
        {
            CurrentBetSize += amount;
            CurrentStack -= amount;
        }
        internal void AwardPot(double amount)
        { 
            CurrentStack += amount;
        }
        internal void NewHand()
        {
            CurrentBetSize = 0;
            ActiveInHand = !Absent;
        }

        internal void Call(double amount)
        {
            CurrentBetSize += amount;
            CurrentStack -= amount;
        }
    }
}