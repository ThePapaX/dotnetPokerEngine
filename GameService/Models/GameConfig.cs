using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Models
{
    public class GameConfig
    {
        public readonly uint SmallBlindSize;
        public readonly uint BigBlindSize;
        public uint PlayerTimeout;
        public uint StartingChipCount;
        public GameConfig()
        {
            SmallBlindSize = 1000;
            BigBlindSize = 2000;
            PlayerTimeout = 15;
            StartingChipCount = 500000;
        }
        public GameConfig(uint smallBlindSize, uint bigBlindSize, uint playerTimeout, uint startingChipCount)
        {
            SmallBlindSize = smallBlindSize;
            BigBlindSize = bigBlindSize;
            PlayerTimeout = playerTimeout;
            StartingChipCount = startingChipCount;
        }
    }
}
