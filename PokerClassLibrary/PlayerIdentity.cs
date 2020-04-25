
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PokerClassLibrary
{
    public class PlayerIdentity : ITrackable
    {
        [ForeignKey("Player")]
        [Key]
        public Guid PlayerId { get; set; }

        public string Password { get; set; }

        // Base64 encoded
        public string Hash { get; set; }

        public string SessionToken { get; set; }

        public virtual Player Player { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

    }
}
