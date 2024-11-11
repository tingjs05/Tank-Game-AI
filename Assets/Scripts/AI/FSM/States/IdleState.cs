using UnityEngine;

namespace AI.FSM
{
    public class IdleState : State<TankFSM>
    {
        public IdleState(StateMachine<TankFSM> fsm, TankFSM character) : base (fsm, character)
        {
        }

        public override void LogicUpdate()
        {
            // check if target is within range to shoot
            if (character.TargetInRange())
            {
                fsm.SwitchState(character.Shoot);
                return;
            }

            // if not within range to shoot, attempt to move to target
            if (character.obstacleDetection.GetPathFindingDirection(
                (character._target.position - character.transform.position).normalized) == Vector3.zero)
                    fsm.SwitchState(character.Patrol);
            else
                fsm.SwitchState(character.Track);
        }
    }
}
