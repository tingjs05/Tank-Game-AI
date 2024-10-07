using UnityEngine;
using AI.ObstacleDetection;

namespace AI.FSM
{
    [RequireComponent(typeof(TankController), typeof(ObstacleDetectionManager))]
    public class TankFSM : StateMachine<TankFSM>
    {
        [SerializeField] Transform target;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f)] float movementThreshold = 0.6f;
        [SerializeField, Range(0f, 1f)] float hardMovementThreshold = 0.85f;

        [Header("Combat")]
        [SerializeField, Range(0f, 1f)] float shootThreshold = 0.9f;
        [SerializeField, Range(0f, 1f)] float recoilControl = 0.75f;
        [SerializeField, Range(0f, 1f)] float minAimSpeed = 0.6f;

        [Header("Flee")]
        [SerializeField] float fleeDistance = 3.5f;
        [SerializeField, Range(0f, 1f)] float fleeHealthThreshold = 0.5f;

        [Header("Testing")]
        [SerializeField] string boundaryTag = "Boundary";
        [SerializeField] bool resetOnDeath, resetOnKill = false;

        public Transform _target => target;
        public float shoot_threshold => shootThreshold;
        public float recoil_control => recoilControl;
        public float min_aim_speed => minAimSpeed;
        public float flee_distance => fleeDistance;
        
        public TankController controller { get; private set; }
        public ObstacleDetectionManager obstacleDetection { get; private set; }

        #region States
        public IdleState Idle { get; private set; }
        public PatrolState Patrol { get; private set; }
        public TrackState Track { get; private set; }
        public ShootState Shoot { get; private set; }
        public FleeState Flee { get; private set; }
        #endregion

        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();

            // initialize fsm
            Idle = new IdleState(this, this);
            Patrol = new PatrolState(this, this);
            Track = new TrackState(this, this);
            Shoot = new ShootState(this, this);
            Flee = new FleeState(this, this);
            Initialize(Idle);

            // handle death reset
            if (resetOnDeath) controller.Died += controller.Reset;

            // handle resetting on kill
            if (!resetOnKill) return;
            TankController enemyController = target.GetComponent<TankController>();
            if (enemyController == null) return;
            enemyController.Died += controller.Reset;
        }

        new void Update()
        {
            base.Update();

            // check if need to flee and if can flee
            if (currentState == Flee || !Flee.CanEnter||
                controller.Health >= (controller.maxHealth * fleeHealthThreshold)) 
                    return;
            // switch to flee state to flee
            SwitchState(Flee);
        }

        public bool TargetInRange()
        {
            Vector3 dir = (target.position - transform.position).normalized;
            float x = obstacleDetection.agentRadius * obstacleDetection.agentRadius;
            float offset = Mathf.Sqrt(x + x);

            return !Physics.Raycast(transform.position + (dir * offset), dir, 
                Vector3.Distance(transform.position, target.position), obstacleDetection.detectionMask);
        }

        public void Move(Vector3 direction)
        {
            // clean up direction input
            direction.y = 0f;
            direction.Normalize();
            // store movement vector
            Vector2 movementInputs = Vector2.zero;
            // calculate movement
            float dot = Vector3.Dot(transform.forward, direction);
            if (dot >= movementThreshold) movementInputs.x = 1f;
            float right_dot = Vector3.Dot(transform.right, direction);
            movementInputs.y = right_dot >= 0f ? 1f : -1f;
            if (dot >= hardMovementThreshold) movementInputs.y = 0f;
            // input movement
            controller.Move(movementInputs);
        }

        void OnTriggerEnter(Collider other) 
        {
            // check for falling out of boundary
            if (!resetOnDeath || !other.CompareTag(boundaryTag)) return;
            controller.Reset();
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

            // show flee range
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, fleeDistance);

            // show target detection ray
            Vector3 dir = (target.position - transform.position).normalized;
            float x = obstacleDetection.agentRadius * obstacleDetection.agentRadius;
            float offset = Mathf.Sqrt(x + x);

            Debug.DrawRay(transform.position + (dir * offset), dir * 
                Vector3.Distance(transform.position, target.position), TargetInRange() ? Color.green :  Color.red);
        }
    }
}
