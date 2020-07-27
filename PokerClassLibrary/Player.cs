
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PokerClassLibrary
{
    public class Player : ITrackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }

        [StringLength(15)]
        public string UserName { get; set; }
        
        [StringLength(255)]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual PlayerIdentity Identity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
