using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Models
{
    public enum HandStage
    {
        Preflop,
        Flop,
        Turn,
        River,
        Finished
    }
    public class HandStageHelper
    {
        private readonly LinkedList<HandStage> Stage;
        private LinkedListNode<HandStage> CurrentStage;
        public HandStage GetCurrentStage() => CurrentStage.Value;

        public void Reset() => CurrentStage = Stage.First;
        public HandStage MoveNext()
        {
            if(CurrentStage.Value == HandStage.Finished)
            {
                throw new InvalidOperationException("CANNOT_MOVE_NEXT_BECAUSE_HAND_STATE_IS_FINISHED");
            }

            CurrentStage = CurrentStage.Next;
            return CurrentStage.Value;
        }
    
        public HandStageHelper()
        {
            Stage = new LinkedList<HandStage>();

            Stage.AddFirst(HandStage.Preflop);
            Stage.AddLast(HandStage.Flop);
            Stage.AddLast(HandStage.Turn);
            Stage.AddLast(HandStage.River);
            Stage.AddLast(HandStage.Finished);

            CurrentStage = Stage.First;
        }
        
    }
}
