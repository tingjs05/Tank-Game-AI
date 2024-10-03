using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.FSM
{
    public class PatrolState : State<TankFSM>
    {
        public PatrolState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }
    }
}
