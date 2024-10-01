using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using AI.ObstacleDetection;

namespace AI
{
    [RequireComponent(typeof(TankController), typeof(ObstacleDetectionManager), typeof(AgentRewardManager))]
    public class TankAgent : Agent
    {
        [SerializeField] Transform target;
        [SerializeField] bool heuristicInputs = false;

        TankController controller;
        ObstacleDetectionManager obstacleDetection;

        // TODO: this is temporary for testing purposes
        Vector3 originalPosition;
        void Awake()
        {
            originalPosition = transform.position;
        }

        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
        }

        public override void OnEpisodeBegin()
        {
            transform.position = originalPosition;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float[] weights = obstacleDetection.GetWeightsBasedOnObstacles();

            foreach (float value in weights)
            {
                sensor.AddObservation(value);
            }

            sensor.AddObservation(transform.position);
            sensor.AddObservation(target.position);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // get movmeent decision
            float moveX = actions.ContinuousActions[0];
            float moveZ = actions.ContinuousActions[1];
            controller.Move(new Vector2(moveX, moveZ));

            // get shooting decision
            int shootInput = actions.DiscreteActions[0];
            // check for input to not shoot
            if (shootInput <= 0) return;
            controller.Shoot();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (!heuristicInputs) return;

            ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = Input.GetAxisRaw("Vertical");
            continuousActions[1] = Input.GetAxisRaw("Horizontal");

            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = Input.GetButton("Fire1") ? 1 : 0;
        }
    }
}
