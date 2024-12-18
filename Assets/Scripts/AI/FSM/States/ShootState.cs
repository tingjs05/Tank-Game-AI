using UnityEngine;

namespace AI.FSM
{
    public class ShootState : State<TankFSM>
    {
        public ShootState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }

        public override void PhysicsUpdate()
        {
            // check if target is still within range to shoot, if not, return to idle
            if (!character.TargetInRange())
            {
                fsm.SwitchState(character.Idle);
                return;
            }

            Vector3 dir = (character._target.position - character.transform.position).normalized;
            float dot = Vector3.Dot(character.transform.forward, dir);

            // check if enemy is within range to shoot
            if (dot >= character.shoot_threshold)
            {
                character.DirectShoot();
                character.DirectMove(new Vector2(character.recoil_control, 0f));
                return;
            }

            // check if the direction of the target is on the right or left
            dot = Vector3.Dot(character.transform.right, dir);
            float yInput = dot == 0f ? 1f : (Mathf.Abs(dot) < 0.5f ? (dot < 0f ? -0.5f : 0.5f) : dot);
            character.DirectMove(new Vector2(0f, yInput));
        }
    }
}
