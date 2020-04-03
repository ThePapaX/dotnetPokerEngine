using MessagePack;
using System;

namespace GameService.Models
{
    [MessagePackObject]
    public class GameEvent
    {
        public GameActionType EventType { get; set; }
        public object Data { get; set; } //TODO: think about a typed Data structure for this... needs to hold maybe cards data when dealing, pot award size, etc.
        public DateTime CreatedAt { get; set; }

        public GameEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}