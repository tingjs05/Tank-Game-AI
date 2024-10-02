using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.ObstacleDetection;

namespace AI.FSM
{
    [RequireComponent(typeof(TankController), typeof(ObstacleDetectionManager))]
    public class TankFSM : StateMachine<TankFSM>
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }
    }
}
