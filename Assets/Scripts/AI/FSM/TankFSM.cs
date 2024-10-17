using UnityEngine;
using AI.ObstacleDetection;

namespace AI.FSM
{
    [RequireComponent(typeof(TankController), typeof(ObstacleDetectionManager))]
    public class TankFSM : StateMachine<TankFSM>
    {
        [SerializeField] Transform target;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f)] protected float movementThreshold = 0.6f;
        [SerializeField, Range(0f, 1f)] protected float hardMovementThreshold = 0.85f;

        [Header("Combat")]
        [SerializeField] protected float lineOfSightRadius = 0.15f;
        [SerializeField, Range(0f, 1f)] protected float shootThreshold = 0.9f;
        [SerializeField, Range(0f, 1f)] protected float recoilControl = 0.75f;
        [SerializeField, Range(0f, 1f)] protected float minAimSpeed = 0.6f;

        [Header("Flee")]
        [SerializeField] protected float fleeDistance = 3.5f;
        [SerializeField, Range(0f, 1f)] protected float fleeHealthThreshold = 0.5f;

        [Header("Resets")]
        [SerializeField] protected bool resetOnDeath, resetOnKill = false;

        public Transform _target => target;
        public float shoot_threshold => shootThreshold;
        public float recoil_control => recoilControl;
        public float min_aim_speed => minAimSpeed;
        public float flee_distance => fleeDistance;
        public float flee_health_threshold => fleeHealthThreshold;
        
        public TankController controller { get; protected set; }
        public ObstacleDetectionManager obstacleDetection { get; protected set; }
        public new Collider collider { get; protected set; }

        #region States
        public IdleState Idle { get; protected set; }
        public PatrolState Patrol { get; protected set; }
        public TrackState Track { get; protected set; }
        public ShootState Shoot { get; protected set; }
        public FleeState Flee { get; protected set; }
        #endregion

        protected virtual void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
            collider = GetComponent<Collider>();

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

        protected new virtual void Update()
        {
            base.Update();

            // check if need to flee and if can flee
            if (currentState == Flee || !Flee.CanEnter) 
                return;
            // switch to flee state to flee
            SwitchState(Flee);
        }

        public bool TargetInRange()
        {
            // get direction of target
            Vector3 dir = (target.position - transform.position).normalized;
            // disable collider to ensure raycast does not detect self
            collider.enabled = false;
            // perform raycast
            bool raycast = !Physics.SphereCast(new Ray(transform.position, dir), lineOfSightRadius, 
                Vector3.Distance(transform.position, target.position), obstacleDetection.detectionMask);
            // reenable collider after raycast is compelted
            collider.enabled = true;
            return raycast;
        }

        public virtual void MoveTowards(Vector3 direction, bool reverse = false)
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
            controller.Move(movementInputs);
        }

        public virtual void DirectMove(Vector2 input)
        {
            controller.Move(input);
        }

        public virtual void DirectShoot()
        {
            controller.Shoot();
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
