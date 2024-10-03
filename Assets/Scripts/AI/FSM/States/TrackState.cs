using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.FSM
{
    public class TrackState : State<TankFSM>
    {
        public TrackState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }
    }
}
