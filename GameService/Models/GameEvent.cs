using MessagePack;
using MessagePack.Formatters;
using System;

namespace GameService.Models
{
    [MessagePackObject]
    public class GameEvent
    {
        [Key(0)]
        public GameActionType EventType { get; set; }

        [Key(1)]
        public dynamic Data { get; set; } //TODO: think about a typed Data structure for this... needs to hold maybe cards data when dealing, pot award size, etc.
        
        [Key(2)]
        public DateTime CreatedAt { get; set; }

        public GameEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}