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
        [SerializeField] float aimReward = 0.5f;
        [SerializeField] float aimedShotReward = 0.5f;
        [SerializeField] float killReward = 2f;
        [SerializeField] float recoilControlReward = 0.5f;

        [Header("Movement")]
        [SerializeField] float moveTowardsPreferredDirReward = 5f;
        [SerializeField] float closeDistanceReward = 0.5f;
        [SerializeField, Range(0f, 1f)] float dangerWeight = 0.5f;
        [SerializeField, Range(0f, 1f)] float correctDirThreshold = 0.85f;
        [SerializeField, Range(0f, 1f)] float aimDirThreshold = 0.99f;

        [Header("Search")]
        [SerializeField] float findTargetReward = 1f;
        [SerializeField] float targetNotFoundPenalty = 1f;
        [SerializeField] float targetNotFoundPenaltyInterval = 2.5f;

        [Header("Obstacle Collision")]
        [SerializeField] float obstacleCollisionPenalty = 1f;
        [SerializeField] string obstacleTag = "Obstacle";

        [Header("Death")]
        [SerializeField] float deathPenalty = 10f;
        [SerializeField] string boundaryTag = "Boundary";

        TankAgent agent;
        TankController controller;

        Vector3 horizontalVel;
        float dot, dirRewardScale, currDistance, prevDistance, targetNotFoundCounter;
        bool targetSeen, foundTarget;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<TankAgent>();
            controller = GetComponent<TankController>();

            // subscribe to events
            controller.Damaged += OnDamaged;
            controller.Died += OnDeath;
            controller.OnShoot += OnShoot;
            agent.OnNewEpisode += HandleNewEpisode;
            agent.OnActionCalled += HandleActionRewards;
            
            // suscribe to kill event
            TankController enemyController = agent._target.GetComponent<TankController>();
            if (enemyController == null) return;
            enemyController.Died += OnKill;
        }

        void FixedUpdate() 
        {
            targetSeen = agent.TargetInRange();

            // reward AI for finding the target
            if (!foundTarget && targetSeen)
            {
                foundTarget = true;
                LogReward("Found Target Reward");
                agent.AddReward(findTargetReward);
            }

            // check if target can be seen, if so, only reward for aiming and shooting
            if (targetSeen)
            {
                // reward for aiming at target when seen
                dot = Vector3.Dot(transform.forward, agent.interest_direction);
                if (dot < aimDirThreshold) return;
                LogReward("Aim Reward");
                agent.AddReward(ScaleReward(aimReward, dot, aimDirThreshold));
                return;
            }

            // calculate horizontal velocity
            horizontalVel = controller.rb.velocity;
            horizontalVel.y = 0f;
            horizontalVel.Normalize();

            // penalize the AI every interval for not finding target
            TargetFoundCheck();
            // check if closing distance between self and target
            CheckDistanceReward();

            // ensure weights array and obstacle detection is not null
            if (agent.weights == null || agent.obstacle_detection == null) return;
            
            for (int i = 0; i < agent.weights.Length; i++)
            {
                // check if facing a good direction
                dot = Vector3.Dot(transform.forward, agent.obstacle_detection.directions[i]);
                if (dot < correctDirThreshold) continue;
                // scale reward depending on if it is a good or bad direction (if there are obstacles)
                dirRewardScale = (1f - agent.weights[i]) * (agent.weights[i] <= dangerWeight ? 1f : -1f);
                // check if moving in good direction
                dot = Vector3.Dot(horizontalVel, agent.obstacle_detection.directions[i]);
                if (dot < correctDirThreshold) continue;
                LogReward(dirRewardScale < 0f ? "Move Bad Direction" : "Move Good Direction");
                agent.AddReward(ScaleReward(moveTowardsPreferredDirReward * dirRewardScale, dot, correctDirThreshold));
            }
        }

        void TargetFoundCheck()
        {
            if (foundTarget) return;
            targetNotFoundCounter += Time.fixedDeltaTime;
            if (targetNotFoundCounter <= targetNotFoundPenaltyInterval) return;
            targetNotFoundCounter = 0f;
            LogReward("Target Not Found Penalty");
            agent.AddReward(-targetNotFoundPenalty);
        }

        void CheckDistanceReward()
        {
            if (foundTarget || targetSeen) return;
            currDistance = Vector3.Distance(transform.position, agent._target.position);
            if (currDistance >= prevDistance && prevDistance != -1f) return;
            LogReward("Close Distance Reward");
            agent.AddReward(prevDistance == -1f ? (closeDistanceReward) : 
                (closeDistanceReward * (prevDistance - currDistance)));
            prevDistance = currDistance;
        }

        float ScaleReward(float rewardAmt, float dot, float threshold)
        {
            return rewardAmt * ((dot - threshold) / (1f - threshold));
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

        void HandleNewEpisode()
        {
            // reset some variables
            foundTarget = false;
            prevDistance = -1f;
            targetNotFoundCounter = 0f;
        }

        void HandleActionRewards(Vector2 moveInput, bool shoot)
        {
            // do not check for reward if target is not seen or shooting
            if (!targetSeen || !shoot) return;

            // reward for controlling recoil
            if (moveInput.x > 0f)
            {
                LogReward("Recoil Control Reward");
                agent.AddReward(recoilControlReward);
            }

            // reward for aiming in correct direction and shooting
            dot = Vector3.Dot(transform.forward, agent.interest_direction);
            if (dot < aimDirThreshold) return;
            LogReward("Aim + Shoot Reward");
            agent.AddReward(ScaleReward(aimedShotReward, dot, aimDirThreshold));
        }

        #endregion
    }
}
