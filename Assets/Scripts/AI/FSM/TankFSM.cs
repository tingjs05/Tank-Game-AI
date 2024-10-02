using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.ObstacleDetection;

namespace AI.FSM
{
    [RequireComponent(typeof(TankController), typeof(ObstacleDetectionManager))]
    public class TankFSM : StateMachine<TankFSM>
    {
        [SerializeField] Transform target;
        TankController controller;
        ObstacleDetectionManager obstacleDetection;

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
        }
    }
}
