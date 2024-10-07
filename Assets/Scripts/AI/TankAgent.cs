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

        [Header("Behaviour")]
        [SerializeField] float fleeDistance = 3.5f;
        [SerializeField, Range(0f, 1f)] float fleeHealthThreshold = 0.5f;

        TankController controller;
        ObstacleDetectionManager obstacleDetection;
        
        public Vector3 interest_direction { get; private set; }
        public Vector3 preferred_direction { get; private set; }
        public Transform _target => target;
        public bool flee => controller != null && 
            Vector3.Distance(transform.position, target.position) < fleeDistance && 
            controller.Health < (controller.maxHealth * fleeHealthThreshold);

        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
        }

        public bool TargetInRange()
        {
            float x = obstacleDetection.agentRadius * obstacleDetection.agentRadius;
            float offset = Mathf.Sqrt(x + x);

            return !Physics.Raycast(transform.position + (interest_direction * offset), interest_direction, 
                Vector3.Distance(transform.position, target.position), obstacleDetection.detectionMask);
        }

        public override void OnEpisodeBegin()
        {
            // reset agent
            controller.Reset();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // calculate directions
            preferred_direction = obstacleDetection.GetPreferredDirection(interest_direction);
            interest_direction = (target.position - transform.position).normalized;
            // check flee interest
            if (flee) interest_direction *= -1f;

            // add observations
            sensor.AddObservation(preferred_direction);
            sensor.AddObservation(interest_direction);
            sensor.AddObservation(transform.position);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(target.position);
            sensor.AddObservation(TargetInRange());
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
            discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
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
