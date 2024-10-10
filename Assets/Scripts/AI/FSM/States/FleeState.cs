using UnityEngine;

namespace AI.FSM
{
    public class FleeState : State<TankFSM>
    {
        private Vector3 fleeDirection => character.obstacleDetection.GetPreferredDirection(
                (character.transform.position - character._target.position).normalized);
        public bool CanEnter => character.controller.Health < (character.controller.maxHealth * character.flee_health_threshold) && 
            Vector3.Distance(character.transform.position, character._target.position) < character.flee_distance &&
            fleeDirection != Vector3.zero;

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
            character.MoveTowards(fleeDirection, true);
            // continue shooting while reversing
            character.DirectShoot();
        }
    }
}
