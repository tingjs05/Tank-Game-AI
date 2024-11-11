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
            
            // check if there is a preferred direction, if so, follow that instead
            if (character.obstacleDetection.GetPathFindingDirection(character._target.position) != Vector3.zero)
            {
                fsm.SwitchState(character.Track);
                return;
            }
            
            // get target direction
            Vector3 targetDir = (character._target.position - character.transform.position).normalized;
            // try to get context steering direction
            Vector3 moveDirection = character.obstacleDetection.GetContextSteeringDirection(targetDir);
            // scale interest direction strength depending on if there is an obstacle in the way
            character.ScaleInterestStrength(targetDir);

            // if there is no direction to move towards from obstacle detection, move towards target
            // only move towards target when last move direction is unknown (0, 0, 0)
            character.MoveTowards((moveDirection == Vector3.zero ? targetDir : moveDirection));
        }
    }
}
