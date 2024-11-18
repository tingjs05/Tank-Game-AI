using UnityEngine;

namespace AI.FSM
{
    [RequireComponent(typeof(TankTrainerAgent))]
    public class TankTrainerFSM : TankFSM
    {
        TankTrainerAgent agent;

        protected override void Start()
        {
            base.Start();
            agent = GetComponent<TankTrainerAgent>();
        }

        public override void MoveTowards(Vector3 direction, bool reverse = false)
        {
            // clean up direction input
            direction.y = 0f;
            direction.Normalize();
            // store movement vector
            Vector2 movementInputs = Vector2.zero;
            // calculate movement
            float dot = Vector3.Dot(transform.forward, direction);
            if (dot >= movementThreshold) movementInputs.x = 1f;
            if (reverse) movementInputs *= -1f;
            float right_dot = Vector3.Dot(transform.right, (reverse ? -direction : direction));
            movementInputs.y = right_dot >= 0f ? 1f : -1f;
            if (dot >= hardMovementThreshold) movementInputs.y = 0f;
            // input movement
            agent.heuristic_move_input = movementInputs;
        }

        public override void DirectMove(Vector2 input)
        {
            agent.heuristic_move_input = input;
        }

        public override void DirectShoot()
        {
            agent.heuristic_shoot_input = 1;
        }
    }
}
