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
        [SerializeField] float faceMoveDirectionReward = 0.05f;
        [SerializeField] float moveTowardsPreferredDirReward = 5f;
        [SerializeField] float correctRotationReward = 0.5f;
        [SerializeField] float movementPenalty = 0.05f;
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

        TankAgent agent;
        TankController controller;
        Vector3 horizontalVel;
        float currDistance, targetNotFoundCounter;
        bool targetSeen, foundTarget, facingMoveDirection;

        #region MonoBehaviour Callbacks

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
            // calculate horizontal velocity
            horizontalVel = controller.rb.velocity;
            horizontalVel.y = 0f;
            horizontalVel.Normalize();

            // check if facing and moving forward
            float dot = Vector3.Dot(transform.forward, horizontalVel);
            // set facing move direction flag
            facingMoveDirection = dot >= correctDirThreshold;

            if (facingMoveDirection) 
            {
                LogReward("Face Move Direction Reward");
                agent.AddReward(ScaleReward(faceMoveDirectionReward, dot, correctDirThreshold));
            }

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

            // penalize the AI every interval for not finding target
            TargetFoundCheck();

            // ensure obstacle detection is not null
            if (agent.obstacle_detection == null) return;

            // compare horizontal velocity with preferred direction
            dot = Vector3.Dot(horizontalVel, agent.preferred_direction);

            // reward AI for travelling in preferred direction and facing forward
            if (dot >= correctDirThreshold && facingMoveDirection)
            {
                LogReward("Move Towards Preferred Direction Reward");
                agent.AddReward(ScaleReward(moveTowardsPreferredDirReward, dot, correctDirThreshold));
                return;
            }
            
            // do not give penalty if dot is still positive
            if (dot >= 0) return;

            // give penalty for moving in the wrong direction
            LogReward("Not Moving Towards Preferred Direction Penalty");
            agent.AddReward(ScaleReward(-moveTowardsPreferredDirReward, Mathf.Abs(dot), 0f));
        }

        void OnCollisionEnter(Collision other)
        {
            // punish agent for colliding with obstacle
            if (!other.collider.CompareTag(obstacleTag)) return;
            LogReward("Obstacle Penalty");
            agent.AddReward(-obstacleCollisionPenalty);
        }

        #endregion

        void TargetFoundCheck()
        {
            if (foundTarget) return;
            targetNotFoundCounter += Time.fixedDeltaTime;
            if (targetNotFoundCounter <= targetNotFoundPenaltyInterval) return;
            targetNotFoundCounter = 0f;
            LogReward("Target Not Found Penalty");
            agent.AddReward(-targetNotFoundPenalty);
        }

        #region Reward Calculation

        void CalculateRotationReward(float horizontalInput, Vector3 targetDir)
        {
            float dot = Vector3.Dot(transform.forward, targetDir);

            if (dot >= aimDirThreshold)
            {
                ApplyRotationReward(horizontalInput <= 0.5f && horizontalInput >= -0.5f, horizontalInput == 0f, correctRotationReward, 
                    "Perfect Rotation Reward", "Wrong Perfect Rotation Penalty");
                return;
            }

            float right_dot = Vector3.Dot(transform.right, targetDir);

            // check rotating right
            if (right_dot > 0)
                ApplyRotationReward(horizontalInput > 0f, (dot > correctDirThreshold ? horizontalInput <= 0.5f : horizontalInput > 0.5f), 
                    correctRotationReward, "Rotate Right Reward", "Wrong Rotate Left Penalty");
            // check rotating left
            else if (right_dot < 0)
                ApplyRotationReward(horizontalInput < 0f, (dot < -correctDirThreshold ? horizontalInput >= -0.5f : horizontalInput < -0.5f), 
                    correctRotationReward, "Rotate Left Reward", "Wrong Rotate Right Penalty");
            // give penalty for facing complete wrong direction and not fixing it
            else
                ApplyRotationReward(horizontalInput != 0f, horizontalInput > 0.5f || horizontalInput < -0.5f, correctRotationReward, 
                    "Fix Bad Rotation Reward", "Wrong Fix Bad Rotation Penalty");
        }

        void ApplyRotationReward(bool correctRotation, bool refinedInput, float rewardAmt, string correctLog, string wrongLog)
        {
            LogReward(correctRotation ? correctLog + (refinedInput ? " (Perfect)" : "") : wrongLog);
            agent.AddReward(correctRotation ? rewardAmt * (refinedInput ? 2f : 1f) : -rewardAmt);
        }

        float ScaleReward(float rewardAmt, float dot, float threshold)
        {
            return rewardAmt * Mathf.Clamp01((dot - threshold) / (1f - threshold));
        }

        void LogReward(string log)
        {
            if (!logRewards) return;
            Debug.Log(log);
        }

        #endregion

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
            targetNotFoundCounter = 0f;
        }

        void HandleActionRewards(Vector2 moveInput, bool shoot)
        {
            // reward for controlling recoil
            if (targetSeen && moveInput.x > 0f)
            {
                bool perfectRecoilControl = moveInput.x > 0.75f && moveInput.x < 0.85f;
                LogReward("Recoil Control Reward" + (perfectRecoilControl ? " (Perfect)" : ""));
                agent.AddReward(recoilControlReward * (perfectRecoilControl ? 2f : 1f));
            }
            // check for movement, give penalty (cost while moving)
            else if (moveInput != Vector2.zero)
            {
                LogReward("Movement Penalty");
                agent.AddReward(-movementPenalty);
            }

            // check rotation reward, reward agent for rotating correctly
            CalculateRotationReward(moveInput.y, targetSeen ? agent.interest_direction : agent.preferred_direction);
            // do not check for reward if target is not seen or shooting
            if (!targetSeen || !shoot) return;

            // reward for aiming in correct direction and shooting
            float dot = Vector3.Dot(transform.forward, agent.interest_direction);
            if (dot < aimDirThreshold) return;
            LogReward("Aim + Shoot Reward");
            agent.AddReward(ScaleReward(aimedShotReward, dot, aimDirThreshold));
        }

        #endregion
    }
}
