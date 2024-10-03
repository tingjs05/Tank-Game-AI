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

        public bool TargetInRange()
        {
            Vector3 dir = (target.position - transform.position).normalized;
            float x = obstacleDetection.agentRadius * obstacleDetection.agentRadius;
            float offset = Mathf.Sqrt(x + x);

            return !Physics.Raycast(transform.position + (dir * offset), dir, 
                Vector3.Distance(transform.position, target.position), obstacleDetection.detectionMask);
        }

        void OnDrawGizmosSelected() 
        {
            // ensure target is not null
            if (target == null) return;
            // ensure obstacle detection is not null
            if (obstacleDetection == null) Start();
            if (obstacleDetection == null) return;
            // check if showing obstacles
            if (!obstacleDetection.showGizmos) return;

            // show target detection ray
            Vector3 dir = (target.position - transform.position).normalized;
            float x = obstacleDetection.agentRadius * obstacleDetection.agentRadius;
            float offset = Mathf.Sqrt(x + x);

            Debug.DrawRay(transform.position + (dir * offset), dir * 
                Vector3.Distance(transform.position, target.position), TargetInRange() ? Color.green :  Color.red);
        }
    }
}
