using System;
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
        [SerializeField] protected TankController target;
        [SerializeField] protected float lineOfSightRadius = 0.15f;
        [SerializeField] protected bool heuristicInputs = false;

        protected TankController controller;
        protected ObstacleDetectionManager obstacleDetection;
        protected new Collider collider;

        protected Vector2 moveInput;
        protected float moveX, moveY;
        protected int shootInput;
        
        public ObstacleDetectionManager obstacle_detection => obstacleDetection;
        public Transform _target => target.transform;
        public Vector3 interest_direction { get; protected set; }
        public float[] weights { get; protected set; }

        public event Action<Vector2, bool> OnActionCalled;
        public event Action OnNewEpisode;

        void Start()
        {
            controller = GetComponent<TankController>();
            obstacleDetection = GetComponent<ObstacleDetectionManager>();
            collider = GetComponent<Collider>();
        }

        public bool TargetInRange()
        {
            // disable collider to ensure raycast does not detect self
            collider.enabled = false;
            // perform raycast
            bool raycast = !Physics.SphereCast(new Ray(transform.position, interest_direction), lineOfSightRadius, 
                Vector3.Distance(transform.position, target.transform.position), obstacleDetection.detectionMask);
            // reenable collider after raycast is compelted
            collider.enabled = true;
            return raycast;
        }

        public override void OnEpisodeBegin()
        {
            // reset agent
            controller.Reset();
            // invoke event for new episode
            OnNewEpisode?.Invoke();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // add weight observations
            weights = obstacleDetection.GetWeightsBasedOnObstacles();

            foreach (float weight in weights)
            {
                sensor.AddObservation(weight);
            }

            // calculate directions
            interest_direction = (target.transform.position - transform.position).normalized;

            // add observations
            sensor.AddObservation(interest_direction);
            sensor.AddObservation(transform.position);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(target.transform.position);
            sensor.AddObservation(controller.Health);
            sensor.AddObservation(target.Health);
            sensor.AddObservation(TargetInRange());
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // get movmeent decision
            moveX = actions.ContinuousActions[0];
            moveY = actions.ContinuousActions[1];
            moveInput = new Vector2(moveX, moveY);
            controller.Move(moveInput);

            // get shooting decision
            shootInput = actions.DiscreteActions[0];

            // invoke action called event
            OnActionCalled?.Invoke(moveInput, shootInput > 0);

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
            if (Input.GetKey(KeyCode.LeftShift)) continuousActions[1] *= 0.5f;

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

            // show target detection ray
            Vector3 dir = (target.transform.position - transform.position).normalized;
            float x = obstacleDetection.agentRadius * obstacleDetection.agentRadius;
            float offset = Mathf.Sqrt(x + x);

            Debug.DrawRay(transform.position + (dir * offset), dir * 
                Vector3.Distance(transform.position, target.transform.position), TargetInRange() ? Color.green :  Color.red);
        }
    }
}
