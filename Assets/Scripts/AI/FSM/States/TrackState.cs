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

            Vector3 prefDir = character.obstacleDetection.GetPathFindingDirection(character._target.position);

            // check if there is a preferred direction
            if (prefDir == Vector3.zero)
            {
                fsm.SwitchState(character.Patrol);
                return;
            }
            
            // move towards preferred direction
            character.MoveTowards(prefDir);
        }
    }
}
