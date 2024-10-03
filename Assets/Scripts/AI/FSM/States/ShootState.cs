using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.FSM
{
    public class ShootState : State<TankFSM>
    {
        public ShootState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }
    }
}
