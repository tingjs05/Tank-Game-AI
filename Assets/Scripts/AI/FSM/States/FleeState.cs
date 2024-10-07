using UnityEngine;

namespace AI.FSM
{
    public class FleeState : State<TankFSM>
    {
        private Vector3 fleeDirection => character.obstacleDetection.GetPreferredDirection(
                (character.transform.position - character._target.position).normalized);
        public bool CanEnter => fleeDirection != Vector3.zero || 
            Vector3.Distance(character.transform.position, character._target.position) < character.flee_distance;

        public FleeState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // check if fled far enough or if there is a direction to flee to
            if (!CanEnter)
            {
                fsm.SwitchState(character.Idle);
                return;
            }

            // move towards flee direction
            character.Move(fleeDirection);
        }
    }
}
