using System.Collections.Generic;
using UnityEngine;

namespace AI.ObstacleDetection
{
    public class ObstacleDetectionManager : MonoBehaviour
    {
        public float detectionRange = 5f;
        public float dangerRange = 1.5f;
        public float sphereCastRadius = 1f;
        public bool showGizmos, showDirections, showDetectedObstacles = true;
        public LayerMask detectionMask;

        List<RaycastHit> obstaclesDetected = new List<RaycastHit>();
        Direction direction;
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

        public float[] GetWeightsBasedOnObstacles()
        {
            DetectObstacles();
            CalculateWeights();
            return weights;
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
                if (!Physics.SphereCast(transform.position, sphereCastRadius, 
                    (hit.transform.position - transform.position).normalized, out RaycastHit rayHit, 
                    detectionRange, detectionMask))
                        continue;

                // add to raycast hit list for calculations
                obstaclesDetected.Add(rayHit);
            }
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
        }
    }
}
