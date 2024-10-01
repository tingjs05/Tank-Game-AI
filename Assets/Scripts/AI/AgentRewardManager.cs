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
        }

        void OnCollisionEnter(Collision other)
        {
            // punish agent for colliding with obstacle
            if (other.collider.CompareTag(obstacleTag))
                agent.AddReward(-obstacleCollisionPenalty);
            
            // punish agent for falling off the map
            if (other.collider.CompareTag(boundaryTag))
            {
                agent.AddReward(-deathPenalty);
                agent.EndEpisode();
            }
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
