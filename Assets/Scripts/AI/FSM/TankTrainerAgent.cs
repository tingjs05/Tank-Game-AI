using UnityEngine;
using Unity.MLAgents.Actuators;

namespace AI.FSM
{
    public class TankTrainerAgent : TankAgent
    {
        [HideInInspector] public Vector2 heuristic_move_input;
        [HideInInspector] public int heuristic_shoot_input;

        protected void LateUpdate() 
        {
            heuristic_shoot_input = 0;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (!heuristicInputs) return;

            ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = heuristic_move_input.y;
            continuousActions[1] = heuristic_move_input.x;

            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = heuristic_shoot_input;
        }
    }
}
