using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

namespace AI.ObstacleDetection
{
    public class ObstacleDetectionManager : MonoBehaviour
    {
        public float detectionRange = 5f;
        public bool showGizmos = true;
        Direction direction;

        void Awake()
        {
            // set directions
            direction = new Direction();
            Debug.Log($"Number of directions: {direction.directions.Length}");
        }

        public void DetectObstacles()
        {
            
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
