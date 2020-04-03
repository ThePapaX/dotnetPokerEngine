using MessagePack;
using System;

namespace GameService.Models
{
    [MessagePackObject]
    public class PlayerEvent
    {
        public string PlayerId { get; set; }
        public PlayerActionType EventType { get; set; }
        public uint BetSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public PlayerEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}