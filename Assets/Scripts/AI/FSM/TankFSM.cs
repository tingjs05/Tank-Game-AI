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
        [SerializeField] LayerMask targetDetectionMask;

        public TankController controller { get; private set; }
        public ObstacleDetectionManager obstacleDetection { get; private set; }

        #region States
        public IdleState Idle { get; private set; }
        public PatrolState Patrol { get; private set; }
        public TrackState Track { get; private set; }
        public ShootState Shoot { get; private set; }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
        }
    }
}
