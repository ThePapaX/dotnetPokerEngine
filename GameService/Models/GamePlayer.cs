namespace GameService.Models
{
    public class GamePlayer
    {
        public string Id { get; set; }
        public int SeatNumber { get; set; }
        public string Username { get; set; }

        //TODO: make this more solid, all this properties should be protected only modifiable by the game.
        public bool IsActiveInHand { get; set; }
        public bool IsAbsent { get; set; }
        public long BetSize { get; set; }
        public long CurrentStack { get; set; }
        public GamePlayer()
        {
            SeatNumber = -1;
            IsActiveInHand = false;
            IsAbsent = false;
            BetSize = 0;
            CurrentStack = 0;
        }
    }
}