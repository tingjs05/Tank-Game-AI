using UnityEngine;

namespace AI.FSM
{
    public class FleeState : State<TankFSM>
    {
        public Vector3 fleeDirection => character.obstacleDetection.GetPreferredDirection(
                (character.transform.position - character._target.position).normalized);

        public FleeState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // check if fled far enough or if there is a direction to flee to
            if (Vector3.Distance(character.transform.position, character._target.position) >= character.flee_distance ||
                fleeDirection == Vector3.zero)
            {
                fsm.SwitchState(character.Flee);
                return;
            }

            // move towards flee direction
            character.Move(fleeDirection);
        }
    }
}
