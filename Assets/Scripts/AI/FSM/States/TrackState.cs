using UnityEngine;

namespace AI.FSM
{
    public class TrackState : State<TankFSM>
    {
        public TrackState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
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

            Vector3 prefDir = character.obstacleDetection.GetContextSteeringDirection(
                (character._target.position - character.transform.position).normalized);

            // check if there is a preferred direction
            if (prefDir == Vector3.zero)
            {
                fsm.SwitchState(character.Patrol);
                return;
            }

            // scale interest direction strength depending on if there is an obstacle in the way
            character.ScaleInterestStrength(prefDir);
            // move towards preferred direction
            character.MoveTowards(prefDir);
            // log move direction to patrol state
            character.Patrol.moveDirection = prefDir;
        }
    }
}
