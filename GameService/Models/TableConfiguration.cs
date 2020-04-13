using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Models
{
    public class TableConfiguration
    {
        public readonly double SmallBlindSize;
        public readonly double BigBlindSize;
        public uint PlayerTimeout;
        public readonly double StartingChipCount;
        public TableConfiguration()
        {
            SmallBlindSize = 1000;
            BigBlindSize = 2000;
            PlayerTimeout = 15;
            StartingChipCount = 500000;
        }
        public TableConfiguration(double smallBlindSize, double bigBlindSize, uint playerTimeout, double startingChipCount)
        {
            SmallBlindSize = smallBlindSize;
            BigBlindSize = bigBlindSize;
            PlayerTimeout = playerTimeout;
            StartingChipCount = startingChipCount;
        }
    }
}
