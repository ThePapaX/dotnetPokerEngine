using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerClassLibrary
{
    public interface ITrackable
    {
        DateTime CreatedAt { get; set; }
        DateTime LastUpdatedAt { get; set; }
    }
}
