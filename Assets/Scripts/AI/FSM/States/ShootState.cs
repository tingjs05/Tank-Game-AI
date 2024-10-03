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

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // check if target is still within range to shoot, if not, return to idle
            if (!character.TargetInRange())
            {
                fsm.SwitchState(character.Idle);
                return;
            }
        }
    }
}
