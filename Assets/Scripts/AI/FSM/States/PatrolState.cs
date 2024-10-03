using UnityEngine;

namespace AI.FSM
{
    public class PatrolState : State<TankFSM>
    {
        public PatrolState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }

        public override void PhysicsUpdate()
        {
            // check if target is within range to shoot
            if (character.TargetInRange())
            {
                fsm.SwitchState(character.Shoot);
                return;
            }

            Vector3 targetDir = (character._target.position - character.transform.position).normalized;

            // check if there is a preferred direction, if so, follow that instead
            if (character.obstacleDetection.GetPreferredDirection(targetDir) != Vector3.zero)
            {
                fsm.SwitchState(character.Track);
                return;
            }
            
            // if there is no direction to move towards from obstacle detection, move towards target
            character.Move(targetDir);
        }
    }
}
