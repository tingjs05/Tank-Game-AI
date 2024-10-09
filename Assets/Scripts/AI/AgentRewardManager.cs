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
        [SerializeField] float killReward = 2f;

        [Header("Movement")]
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

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<TankAgent>();
            controller = GetComponent<TankController>();

            // subscribe to events
            controller.Damaged += OnDamaged;
            controller.Died += OnDeath;
            controller.OnShoot += OnShoot;
            
            // suscribe to kill event
            TankController enemyController = agent._target.GetComponent<TankController>();
            if (enemyController == null) return;
            enemyController.Died += OnKill;
        }

        void FixedUpdate() 
        {
            // store target seen result
            bool targetSeen = agent.TargetInRange();
            // store dot calculation (to check direction)
            float dot;

            // calculate horizontal velocity
            Vector3 horizontalVel = controller.rb.velocity;
            horizontalVel.y = 0f;
            horizontalVel.Normalize();

            if (targetSeen)
            {
                // reward for aiming at target when seen
                dot = Vector3.Dot(transform.forward, agent.interest_direction);
                if (dot >= correctDirThreshold) agent.AddReward(faceInteresetDirReward * dot);
                return;
            }

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

        #endregion
    }
}
