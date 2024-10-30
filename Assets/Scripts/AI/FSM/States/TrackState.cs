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

            Vector3 prefDir = character.obstacleDetection.GetPreferredDirection(
                (character._target.position - character.transform.position).normalized);

            // check if there is a preferred direction
            if (prefDir == Vector3.zero)
            {
                fsm.SwitchState(character.Patrol);
                return;
            }

            // scale interest direction strength depending on if there is an obstacle in the way
            ScaleInterestStrength(prefDir);
            // move towards preferred direction
            character.MoveTowards(prefDir);
            // log move direction to patrol state
            character.Patrol.moveDirection = prefDir;
        }

        void ScaleInterestStrength(Vector3 prefDir)
        {
            // try projecting hitbox in move direction to double check for obstacles
            if (Physics.OverlapBox(character.transform.position + 
                (prefDir * character.obstacleDetection.agentRadius), 
                new Vector3(character.obstacleDetection.agentRadius, character.obstacleDetection.agentRadius, 
                character.obstacleDetection.agentRadius) * character.movement_obstacle_detection_scale, character.transform.rotation, 
                character.obstacleDetection.detectionMask).Length > 0)
            {
                character.obstacleDetection.interestDirectionStrength = 
                    Mathf.Clamp01(character.obstacleDetection.interestDirectionStrength - character.obstacle_avoidance_correction);
            }
            else
            {
                character.obstacleDetection.interestDirectionStrength = 1f;
            }
        }
    }
}
