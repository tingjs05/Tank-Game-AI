using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.FSM
{
    public class IdleState : State<TankFSM>
    {
        public IdleState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }
    }
}
