using System.Collections;
using UnityEngine;

namespace AI.FSM
{
    public class FleeState : State<TankFSM>
    {
        private Coroutine duration, cooldown;
        private Vector3 fleeDirection, dodgeDirection, aggregatedDodgeDirection;
        private float dodgeDirDot;
        private bool reverseDodge = false;

        // try to flee if health is low, after cooldown, and the target can be seen
        public bool CanEnter => character.controller.Health < (character.controller.maxHealth * character.flee_health_threshold) && 
            cooldown == null && character.TargetInRange();

        public FleeState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }

        public override void Enter()
        {
            // start counting state duration
            duration = character.StartCoroutine(CountDuration());
            // ensure cooldown is reset
            if (cooldown == null) return;
            character.StopCoroutine(cooldown);
            cooldown = null;
        }

        public override void PhysicsUpdate()
        {
            // calculate flee direction
            fleeDirection = character.obstacleDetection.GetPreferredDirection(
                (character.transform.position - character._target.position).normalized);
            
            // move away if too close, and a direction to flee exists
            if (Vector3.Distance(character.transform.position, character._target.position) < character.flee_distance
                && fleeDirection != Vector3.zero)
            {
                // scale interest dir strength before moving
                character.ScaleInterestStrength(fleeDirection);
                // move towards flee direction
                character.MoveTowards(fleeDirection, true);
                // continue shooting while reversing
                character.DirectShoot();
                return;
            }

            // calculate dodge direction (perpendicular direction from target)
            dodgeDirection = (character._target.position - character.transform.position).normalized;
            dodgeDirection = new Vector3(dodgeDirection.z, dodgeDirection.y, -dodgeDirection.x);
            dodgeDirection *= reverseDodge ? -1f : 1f;
            aggregatedDodgeDirection = character.obstacleDetection.GetPreferredDirection(dodgeDirection);

            // scale interest dir strength before moving
            character.ScaleInterestStrength(aggregatedDodgeDirection);
            // move towards dodge direction
            character.MoveTowards(aggregatedDodgeDirection);

            // check if still have room to dodge
            dodgeDirDot = Vector3.Dot(aggregatedDodgeDirection, dodgeDirection);

            if (dodgeDirDot >= 0f && aggregatedDodgeDirection != Vector3.zero) return;
            // return to idle state if cannot dodge anymore
            fsm.SwitchState(character.Idle);
        }

        public override void Exit()
        {
            // reverse dodge the next time
            reverseDodge = !reverseDodge;
            // start cooldown
            cooldown = character.StartCoroutine(CountCooldown());
            // ensure duration is reset when exiting state
            if (duration == null) return;
            character.StopCoroutine(duration);
            duration = null;
        }

        IEnumerator CountCooldown()
        {
            yield return new WaitForSeconds(Random.Range(character.flee_cooldown.x, character.flee_cooldown.y));
            cooldown = null;
        }

        IEnumerator CountDuration()
        {
            yield return new WaitForSeconds(Random.Range(character.flee_duration.x, character.flee_duration.y));
            duration = null;
            // return to idle state after state duration
            fsm.SwitchState(character.Idle);
        }
    }
}
