using System.Collections.Generic;
using UnityEngine;

namespace AI.ObstacleDetection
{
    public class ObstacleDetectionManager : MonoBehaviour
    {
        public float detectionRange = 5f;
        public float dangerRange = 1.5f;
        public float agentRadius = 0.5f;
        public string boundaryTag = "Boundary";
        public bool showGizmos, showDirections, showDetectedObstacles = true;
        public LayerMask detectionMask, groundMask;

        List<RaycastHit> obstaclesDetected = new List<RaycastHit>();
        Direction direction;
        Vector3 preferredDirection;
        float[] weights;

        public int numberOfDirections => direction.directions.Length;

        void Awake()
        {
            // set directions
            direction = new Direction();
            Debug.Log($"Number of directions: {numberOfDirections}");
            // create weights array
            weights = new float[numberOfDirections];
        }
        
        // TODO: comment this out in production, this is for debugging & testing purposes
        // void Update()
        // {
        //     GetWeightsBasedOnObstacles();
        // }

        public Vector3 GetPreferredDirection(Vector3 interestDir)
        {
            GetWeightsBasedOnObstacles();
            preferredDirection = Vector3.zero;

            // aggregate directions based on weights
            for (int i = 0; i < numberOfDirections; i++)
            {
                preferredDirection += direction.directions[i] * weights[i];
            }

            // reverse direction to get direction of "safe" area
            preferredDirection = -preferredDirection;
            // after reversing danger, add interest direction
            preferredDirection += interestDir.normalized;
            // normalize direction before returning
            return preferredDirection.normalized;
        }

        void GetWeightsBasedOnObstacles()
        {
            DetectObstacles();
            DetectGround();
            CalculateWeights();
        }

        void DetectObstacles()
        {
            // reset ray hits list
            obstaclesDetected.Clear();
            // detect obstacles within range
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, detectionMask);
            // check if any obstacles are hit
            if (hits == null || hits.Length <= 0) return;

            // try to fire a sphere cast at each obstacle to find the edge of the collider
            foreach (Collider hit in hits)
            {
                if (!Physics.SphereCast(transform.position, agentRadius * 0.95f, 
                    (hit.transform.position - transform.position).normalized, out RaycastHit rayHit, 
                    detectionRange, detectionMask))
                        continue;

                // add to raycast hit list for calculations
                obstaclesDetected.Add(rayHit);
            }
        }

        void DetectGround()
        {
            // cast a raycast vertically down to detect the ground
            // this is to be done at each edge of the agent
            foreach (Vector3 dir in direction.directions)
            {
                if (dir.y != 0f) continue;
                DetectGroundAtEdge(transform.position + (dir * agentRadius));
            }
        }

        void DetectGroundAtEdge(Vector3 edge)
        {
            // check if ground or boundary can be detected at corner
            if (!Physics.Raycast(edge, -Vector3.up, out RaycastHit rayHit, 
                Mathf.Infinity, groundMask))
                    return;
            
            // check if it is the boundary
            if (!rayHit.collider.CompareTag(boundaryTag)) return;
            // handle detecting boundary
            obstaclesDetected.Add(rayHit);
        }

        void CalculateWeights()
        {
            // reset weights
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 0f;
            }

            // calculate weights for each obstacle detected
            foreach (RaycastHit hit in obstaclesDetected)
            {
                for (int i = 0; i < numberOfDirections; i++)
                {
                    Vector3 dirVector = direction.directions[i];
                    Vector3 obstacleDir = (hit.point - transform.position).normalized;
                    float dot = Mathf.Clamp01(Vector3.Dot(obstacleDir, dirVector));
                    float dist = Vector3.Distance(hit.point, transform.position);
                    float distWeight = dist <= dangerRange ? 1f : ((detectionRange - dist) / detectionRange);
                    float finalWeight = dot * distWeight;
                    weights[i] += finalWeight;
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // show detection range
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            // show danger range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dangerRange);
            // show agent radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, agentRadius);

            // handle showing direction using debug ray
            ShowDirection();

            // show detected obstacles, ensure there are obstacles detected
            if (!showDetectedObstacles || obstaclesDetected.Count <= 0) return;

            Gizmos.color = Color.magenta;

            foreach (RaycastHit hit in obstaclesDetected)
            {
                Gizmos.DrawSphere(hit.point, 0.15f);
            }
        }

        void ShowDirection()
        {
            // show directions and respective weights
            if (!showDirections) return;
            // if directions is null, run awake to setup directions
            if (direction == null) Awake();

            for (int i = 0; i < numberOfDirections; i++)
            {
                Debug.DrawRay(transform.position, direction.directions[i] * weights[i] + direction.directions[i], Color.yellow);
            }

            // show preferred direction
            Debug.DrawRay(transform.position, preferredDirection * 5f, Color.cyan);
        }
    }
}
