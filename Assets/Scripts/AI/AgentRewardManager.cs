using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(TankAgent))]
    public class AgentRewardManager : MonoBehaviour
    {
        [Header("Combat")]
        [SerializeField] float damagedPenalty = 0.5f;
        [SerializeField] float missedShotPenalty = 0.15f;
        [SerializeField] float hitShotReward = 0.5f;
        [SerializeField] float aimedShotReward = 0.5f;
        [SerializeField] float killReward = 2f;

        [Header("Movement")]
        [SerializeField] float faceForwardWhenMovingReward = 0.5f;
        [SerializeField] float faceInteresetDirReward = 0.5f;
        [SerializeField] float moveTowardsInterestDirReward = 1f;
        [SerializeField] float moveTowardsPreferredDirReward = 5f;
        [SerializeField, Range(0f, 1f)] float correctDirThreshold = 0.85f;

        [Header("Obstacle Collision")]
        [SerializeField] float obstacleCollisionPenalty = 1f;
        [SerializeField] string obstacleTag = "Obstacle";

        [Header("Death")]
        [SerializeField] float deathPenalty = 10f;
        [SerializeField] string boundaryTag = "Boundary";

        TankAgent agent;
        TankController controller;

        Vector3 horizontalVel;
        float dot;
        bool targetSeen;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<TankAgent>();
            controller = GetComponent<TankController>();

            // subscribe to events
            controller.Damaged += OnDamaged;
            controller.Died += OnDeath;
            controller.OnShoot += OnShoot;
            agent.OnActionCalled += HandleActionRewards;
            
            // suscribe to kill event
            TankController enemyController = agent._target.GetComponent<TankController>();
            if (enemyController == null) return;
            enemyController.Died += OnKill;
        }

        void FixedUpdate() 
        {
            targetSeen = agent.TargetInRange();

            // check if target can be seen, if so, only reward for aiming and shooting
            if (targetSeen)
            {
                // reward for aiming at target when seen
                dot = Vector3.Dot(transform.forward, agent.interest_direction);
                if (dot >= correctDirThreshold) agent.AddReward(faceInteresetDirReward * dot);
                return;
            }

            // calculate horizontal velocity
            horizontalVel = controller.rb.velocity;
            horizontalVel.y = 0f;
            horizontalVel.Normalize();

            dot = Vector3.Dot(transform.forward, horizontalVel);

            // reward agent for facing forward while moving
            if (dot >= correctDirThreshold)
                agent.AddReward(faceForwardWhenMovingReward);

            dot = Vector3.Dot(horizontalVel, agent.preferred_direction);

            // reward for moving in preferred direction
            if (agent.preferred_direction != Vector3.zero && dot >= correctDirThreshold)
            {
                agent.AddReward(moveTowardsPreferredDirReward * dot);
                return;
            }

            dot = Vector3.Dot(horizontalVel, agent.interest_direction);

            // reward for moving in interest direction when no preferred direction
            if (dot >= correctDirThreshold)
                agent.AddReward(moveTowardsInterestDirReward * dot);
        }

        void OnCollisionEnter(Collision other)
        {
            // punish agent for colliding with obstacle
            if (!other.collider.CompareTag(obstacleTag)) return;
            agent.AddReward(-obstacleCollisionPenalty);
        }

        void OnTriggerEnter(Collider other) 
        {
            // punish agent for falling off the map
            if (!other.CompareTag(boundaryTag)) return;
            agent.AddReward(-deathPenalty);
            agent.EndEpisode();
        }

        #region Event Listeners
        
        void OnDamaged()
        {
            agent.AddReward(-damagedPenalty);
        }

        void OnDeath()
        {
            agent.AddReward(-deathPenalty);
            agent.EndEpisode();
        }

        void OnKill()
        {
            agent.AddReward(killReward);
            agent.EndEpisode();
        }

        void OnShoot(bool hit)
        {
            if (hit)
                agent.AddReward(hitShotReward);
            else
                agent.AddReward(-missedShotPenalty);
        }

        void HandleActionRewards(Vector2 moveInput, bool shoot)
        {
            // reward for aiming in correct direction and shooting
            dot = Vector3.Dot(transform.forward, agent.interest_direction);
            if (dot >= correctDirThreshold && shoot) agent.AddReward(aimedShotReward * dot);
        }

        #endregion
    }
}
