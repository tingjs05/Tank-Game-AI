using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Astar;

namespace AI.ObstacleDetection
{
    public class ObstacleDetectionManager : MonoBehaviour
    {
        public float detectionRange = 5f;
        public float dangerRange = 1.5f;
        public float agentRadius = 0.5f;
        public string boundaryTag = "Boundary";

        [Range(0f, 1f)] 
        public float interestDirectionStrength = 1f;

        public bool showGizmos, showDirections, showDetectedObstacles, showPathfinding = true;
        public LayerMask detectionMask, groundMask;
        
        // context steering fields
        List<RaycastHit> obstaclesDetected = new List<RaycastHit>();
        Direction direction;
        Vector3 preferredDirection;
        float[] weights;

        // pathfinding fields
        Pathfinding pathfinder;
        List<PathNode> path;

        public Vector3[] directions => direction.directions;
        public int numberOfDirections => directions.Length;

        void Start()
        {
            // set directions
            direction = new Direction();
            // Debug.Log($"Number of directions: {numberOfDirections}");
            // create weights array
            weights = new float[numberOfDirections];
            // initialize pathfinding component
            pathfinder = new Pathfinding();
        }

        public Vector3 GetPathFindingDirection(Vector3 targetPos)
        {
            // get path using Astar
            path = pathfinder.FindPath(transform.position, targetPos);
            // check if path can be found
            if (path == null) return Vector3.zero;
            // search through path for next closest node
            foreach (PathNode node in path)
            {
                if (Vector3.Distance(node.node.transform.position, transform.position) <= agentRadius) continue;
                return (node.node.transform.position - transform.position).normalized;
            }
            // default direction is 0 (meaning destination reached)
            return Vector3.zero;
        }

        public Vector3 GetContextSteeringDirection(Vector3 interestDir)
        {
            CalculateContextSteering(interestDir);
            
            // if there is no preferred direction, try to calculate again with no interest direction
            if (preferredDirection == Vector3.zero)
                return CalculateContextSteering(Vector3.zero);
            else
                return preferredDirection;
        }

        Vector3 CalculateContextSteering(Vector3 interestDir)
        {
            GetWeightsBasedOnObstacles();
            preferredDirection = Vector3.zero;

            // calculate add interest direction
            float[] interests = AddWeight(new float[numberOfDirections], transform.position + interestDir);

            // aggregate directions based on weights
            for (int i = 0; i < numberOfDirections; i++)
            {
                preferredDirection += directions[i] * ((interests[i] * interestDirectionStrength) - weights[i]);
            }

            // keep magnitude and take out y-axis direction
            float mag = preferredDirection.magnitude;
            preferredDirection.y = 0f;
            preferredDirection *= mag;
            // normalize direction before returning
            preferredDirection.Normalize();
            return preferredDirection;
        }

        public float[] GetWeightsBasedOnObstacles()
        {
            DetectObstacles();
            DetectGround();
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
            foreach (Vector3 dir in directions)
            {
                if (dir.y != 0f) continue;
                // check if ground can be detected at agent radius
                // if it can be detected, check if it can still be detected at danger and detection range
                if (!DetectGroundAtEdge(transform.position + (dir * agentRadius))) continue;
                if (!DetectGroundAtEdge(transform.position + (dir * dangerRange))) continue;
                DetectGroundAtEdge(transform.position + (dir * detectionRange));
            }
        }

        bool DetectGroundAtEdge(Vector3 edge)
        {
            // check if ground or boundary can be detected at corner
            if (!Physics.Raycast(edge, -Vector3.up, out RaycastHit rayHit, 
                Mathf.Infinity, groundMask))
                    return false;
            
            // check if it is the boundary
            if (!rayHit.collider.CompareTag(boundaryTag)) return true;
            // handle detecting boundary
            obstaclesDetected.Add(rayHit);
            // return not being able to detect ground
            return false;
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
                weights = AddWeight(weights, hit.point);
            }
        }

        float[] AddWeight(float[] weights_array, Vector3 obj_pos)
        {
            for (int i = 0; i < numberOfDirections; i++)
            {
                Vector3 dirVector = directions[i];
                Vector3 objectDir = (obj_pos - transform.position).normalized;
                float dot = Mathf.Clamp01(Vector3.Dot(objectDir, dirVector));
                float dist = Vector3.Distance(obj_pos, transform.position);
                float distWeight = dist <= dangerRange ? 1f : ((detectionRange - dist) / detectionRange);
                float finalWeight = dot * distWeight;
                weights_array[i] += finalWeight;
            }

            return weights_array;
        }

        void OnDrawGizmos()
        {
            // do not draw gizmos of show gizmos is false
            if (!showGizmos || !showPathfinding) return;
            // if pathfinder cannot be found, dont draw gizmos for path finding
            if (pathfinder == null) return;
            
            // draw horizon nodes
            foreach (PathNode node in pathfinder.open)
            {
                // if node is part of path, draw it as yellow
                Gizmos.color = path != null && path.Contains(node) ? Color.yellow : Color.blue;

                // show connection to previous node
                if (node.previousNode != null) 
                    Debug.DrawRay(node.node.transform.position, 
                        node.previousNode.node.transform.position - node.node.transform.position, Gizmos.color);
                
                Gizmos.DrawSphere(node.node.transform.position, 0.15f);
            }

            // draw visited nodes
            foreach (PathNode node in pathfinder.closed)
            {
                // if node is part of path, draw it as yellow
                Gizmos.color = path != null && path.Contains(node) ? Color.yellow : Color.cyan;

                // show connection to previous node
                if (node.previousNode != null) 
                    Debug.DrawRay(node.node.transform.position, 
                        node.previousNode.node.transform.position - node.node.transform.position, Gizmos.color);

                Gizmos.DrawSphere(node.node.transform.position, 0.15f);
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
            // if directions is null, run start to setup directions
            if (direction == null) Start();

            for (int i = 0; i < numberOfDirections; i++)
            {
                Debug.DrawRay(transform.position, directions[i] * weights[i] + directions[i], Color.yellow);
            }

            // show preferred direction
            Debug.DrawRay(transform.position, preferredDirection * 5f, Color.cyan);
        }
    }
}
