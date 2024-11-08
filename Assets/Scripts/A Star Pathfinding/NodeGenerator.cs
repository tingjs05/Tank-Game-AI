using UnityEngine;
using EasyButtons;

namespace Astar
{
    public class NodeGenerator : MonoBehaviour
    {
        // editor fields
        [Header("Grid Setup Fields")]
        public Vector2 gridSize;
        public float gridFrequency = 1f;

        [Header("Node Setup Fields")]
        public NodeManager nodeManager;
        public GameObject nodePrefab;
        public LayerMask groundMask;

        [Header("Gizmos")]
        public bool showGizmos = true;

        public Vector3 pointOfOrigin => new Vector3(transform.position.x - (gridSize.x * 0.5f), 
            transform.position.y, transform.position.z - (gridSize.y * 0.5f));

        [Button]
        public void GenerateNode()
        {
            // error prevention
            if (nodePrefab == null)
            {
                Debug.LogWarning($"{this}, NodeGenerator.cs: Node prefab is null, nodes could not be generated! ");
                return;
            }

            // instantiate node manager instance if not instantiated
            if (NodeManager.Instance == null && nodeManager != null)
                nodeManager.Awake();

            // generate grid
            float x = 0f;
            float z = 0f;
            Vector3 currentPosition;
            RaycastHit hit;
            
            // loop through each position to gernate grid
            for (int i = 0; i < ((1 / gridFrequency) * gridSize.x) + 1; i++)
            {
                for (int j = 0; j < ((1 / gridFrequency) * gridSize.y) + 1; j++)
                {
                    // set node position
                    currentPosition = new Vector3(pointOfOrigin.x + x, pointOfOrigin.y, pointOfOrigin.z + z);
                    
                    // detect ground above and below node to follow terrain
                    // only generate node if detected ground
                    if (Physics.Raycast(currentPosition, -Vector3.up, out hit, Mathf.Infinity, groundMask) ||
                        Physics.Raycast(currentPosition, Vector3.up, out hit, Mathf.Infinity, groundMask))
                    {
                        Instantiate(nodePrefab, hit.point, Quaternion.identity, transform);
                    }

                    // iterate position
                    z += gridFrequency;
                    // if it is the last column, reset z to 0 to prepare to iterate through next column
                    z = (j >= (1 / gridFrequency) * gridSize.y)? 0f : z;
                }
                // iterate position
                x += gridFrequency;
            }

            // generate connection
            foreach (Node node in nodeManager.Nodes)
            {
                node.GenerateConnections(gridFrequency);
            }
        }

        [Button]
        public void ClearNodes()
        {
            foreach (Transform node in transform)
            {
                if (Application.isPlaying)
                    Destroy(node.gameObject);
                else
                    DestroyImmediate(node.gameObject);
            }
        }

        void OnDrawGizmos() 
        {
            // check if want to show gizmos
            if (!showGizmos) return;

            // show position
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(pointOfOrigin, 0.5f);

            // check to show setup gizmos
            Vector3 tempPoint;
            // show point of origin
            Gizmos.color = Color.magenta;
            tempPoint = pointOfOrigin;
            Gizmos.DrawSphere(tempPoint, 0.5f);
            // show other points
            tempPoint.x += gridSize.x;
            tempPoint.z += gridSize.y;
            Gizmos.DrawSphere(tempPoint, 0.5f);
            // change color to white
            Gizmos.color = Color.white;
            tempPoint.x -= gridSize.x;
            Gizmos.DrawSphere(tempPoint, 0.5f);
            tempPoint.x += gridSize.x;
            tempPoint.z -= gridSize.y;
            Gizmos.DrawSphere(tempPoint, 0.5f);
        }
    }
}
