using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Models
{
    public enum HandState
    {
        Preflop,
        Flop,
        Turn,
        River
    }
    public class HandStateMachine
    {
        private readonly LinkedList<HandState> State;
        private LinkedListNode<HandState> CurrentState;
        public HandState GetCurrentStateValue() => CurrentState.Value;

        public void Reset() => CurrentState = State.First;
        public HandState MoveNext()
        {
            if(CurrentState.Value == HandState.River)
            {
                throw new InvalidOperationException("CANNOT_MOVE_NEXT_BECAUSE_HAND_STATE_IS_FINISHED");
            }

            CurrentState = CurrentState.Next;
            return CurrentState.Value;
        }
    
        public HandStateMachine()
        {
            State = new LinkedList<HandState>();

            State.AddFirst(HandState.Preflop);
            State.AddLast(HandState.Flop);
            State.AddLast(HandState.Turn);
            State.AddLast(HandState.River);
            
            CurrentState = State.First;
        }
        
    }
}
