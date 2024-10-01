using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using AI.ObstacleDetection;

namespace AI
{
    [RequireComponent(typeof(TankController), typeof(ObstacleDetectionManager))]
    public class TankAgent : Agent
    {
        [SerializeField] Transform target;

        TankController controller;
        ObstacleDetectionManager obstacleDetection;

        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
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
    }
}
