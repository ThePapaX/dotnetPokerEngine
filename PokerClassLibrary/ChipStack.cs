namespace PokerClassLibrary
{
    internal class ChipStack
    {
        private long Id { get; set; }
        private uint Size { get; set; }
        private long PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}