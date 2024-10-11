using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(TankAgent))]
    public class AgentRewardManager : MonoBehaviour
    {
        [SerializeField] bool logRewards = true;

        [Header("Combat")]
        [SerializeField] float damagedPenalty = 0.5f;
        [SerializeField] float missedShotPenalty = 0.15f;
        [SerializeField] float hitShotReward = 0.5f;
        [SerializeField] float aimedShotReward = 0.5f;
        [SerializeField] float killReward = 2f;
        [SerializeField] float findTargetReward = 0.5f;

        [Header("Movement")]
        [SerializeField] float faceForwardWhenMovingReward = 0.5f;
        [SerializeField] float faceInteresetDirReward = 0.5f;
        [SerializeField] float moveTowardsInterestDirReward = 1f;
        [SerializeField] float moveTowardsPreferredDirReward = 5f;
        [SerializeField] float rotateTowardsDirectionReward = 0.5f;
        [SerializeField] float wrongRotationDirectionPenalty = 0.5f;
        [SerializeField, Range(0f, 1f)] float correctDirThreshold = 0.85f;
        [SerializeField, Range(0f, 1f)] float aimDirThreshold = 0.99f;

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
        bool targetSeen, prevTargetSeen;

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
            prevTargetSeen = targetSeen;
            targetSeen = agent.TargetInRange();

            // reward AI for finding the target
            if (!prevTargetSeen && targetSeen)
                agent.AddReward(findTargetReward);

            // check if target can be seen, if so, only reward for aiming and shooting
            if (targetSeen)
            {
                // reward for aiming at target when seen
                dot = Vector3.Dot(transform.forward, agent.interest_direction);

                if (dot >= aimDirThreshold) 
                {
                    LogReward("Aim Reward");
                    agent.AddReward(faceInteresetDirReward * dot);
                }

                return;
            }

            // calculate horizontal velocity
            horizontalVel = controller.rb.velocity;
            horizontalVel.y = 0f;
            horizontalVel.Normalize();

            dot = Vector3.Dot(transform.forward, horizontalVel);

            // reward agent for facing forward while moving
            if (dot < correctDirThreshold) return;
            LogReward("Move Forward Reward");
            agent.AddReward(faceForwardWhenMovingReward);

            dot = Vector3.Dot(horizontalVel, agent.preferred_direction);

            // reward for moving in preferred direction
            if (agent.preferred_direction != Vector3.zero && dot >= correctDirThreshold)
            {
                agent.AddReward(moveTowardsPreferredDirReward * dot);
                LogReward("Pref Dir Reward");
                return;
            }

            dot = Vector3.Dot(horizontalVel, agent.interest_direction);

            // reward for moving in interest direction when no preferred direction
            if (dot < correctDirThreshold) return;
            LogReward("Int Dir Reward");
            agent.AddReward(moveTowardsInterestDirReward * dot);
        }

        void LogReward(string log)
        {
            if (!logRewards) return;
            Debug.Log(log);
        }

        void OnCollisionEnter(Collision other)
        {
            // punish agent for colliding with obstacle
            if (!other.collider.CompareTag(obstacleTag)) return;
            LogReward("Obstacle Penalty");
            agent.AddReward(-obstacleCollisionPenalty);
        }

        void OnTriggerEnter(Collider other) 
        {
            // punish agent for falling off the map
            if (!other.CompareTag(boundaryTag)) return;
            LogReward("Death Penalty");
            agent.AddReward(-deathPenalty);
            agent.EndEpisode();
        }

        #region Event Listeners
        
        void OnDamaged()
        {
            LogReward("Damaged Penalty");
            agent.AddReward(-damagedPenalty);
        }

        void OnDeath()
        {
            LogReward("Death Penalty");
            agent.AddReward(-deathPenalty);
            agent.EndEpisode();
        }

        void OnKill()
        {
            LogReward("Kill Reward");
            agent.AddReward(killReward);
            agent.EndEpisode();
        }

        void OnShoot(bool hit)
        {
            if (hit)
            {
                LogReward("Hit Reward");
                agent.AddReward(hitShotReward);
            }

            LogReward("Miss Penalty");
            agent.AddReward(-missedShotPenalty);
        }

        void HandleActionRewards(Vector2 moveInput, bool shoot)
        {
            // reward for rotating towards correct direction
            Vector3 direction = agent.preferred_direction == Vector3.zero || targetSeen ? 
                agent.interest_direction : agent.preferred_direction;
            // calculate dot based on direction to face
            dot = Vector3.Dot(transform.right, direction);

            if (dot != 0f)
            {
                if ((dot > 0f && moveInput.y > 0) || (dot < 0f && moveInput.y < 0))
                {
                    LogReward("Rotation Reward");
                    agent.AddReward(rotateTowardsDirectionReward);
                }
                else
                {
                    LogReward("Wrong Rotation Penalty");
                    agent.AddReward(-wrongRotationDirectionPenalty);
                }
            }

            // do not check for reward if target is not seen
            if (!targetSeen) return;
            // reward for aiming in correct direction and shooting
            dot = Vector3.Dot(transform.forward, agent.interest_direction);
            if (dot < aimDirThreshold || !shoot) return;
            LogReward("Aim + Shoot Reward");
            agent.AddReward(aimedShotReward * dot);
        }

        #endregion
    }
}
