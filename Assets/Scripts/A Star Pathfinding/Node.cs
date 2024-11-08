using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    public struct LocalNode
    {
        public Node node;
        public Node previousNode;
        public int G, H;

        public LocalNode(Node node)
        {
            this.node = node;
            previousNode = null;
            G = 0;
            H = 0;
        }
    }

    [RequireComponent(typeof(SphereCollider))]
    public class Node : MonoBehaviour
    {
        // inspector fields
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private List<Node> connections;
        [SerializeField] private bool obstructed = false;

        // properties
        public bool isObstructed
        {
            get { return obstructed; }
        }

        public List<Node> nodeConnections 
        {
            get { return connections; }
        }

        private SphereCollider _collider;
        private SphereCollider sphereCollider
        {
            get 
            {
                if (_collider == null) 
                {
                    _collider = GetComponent<SphereCollider>();
                    _collider.isTrigger = true;
                }

                return _collider;
            }
        }

        private NodeManager nodeManager => NodeManager.Instance;

        public void CheckObstructed()
        {
            obstructed = Physics.OverlapSphere(
                transform.position + sphereCollider.center, sphereCollider.radius, obstacleMask).Length > 0;
        }

        public void GenerateConnections(float frequency)
        {
            if (nodeManager == null)
            {
                Debug.LogWarning($"{this}, Node.cs: NodeManager instance could not be found, unable to generate node connections. ");
                return;
            }

            // reset connections list
            if (connections == null)
                connections = new List<Node>();
            else 
                connections.Clear();

            // set the max distance for a connection between nodes
            float maxDistance = frequency * Mathf.Sqrt(2) * 1.15f;
            // loop through each node to find which nodes can form connection
            foreach (Node node in NodeManager.Instance.Nodes)
            {
                // if the node is itself or obstructed, dont add as a connection
                if (node.Equals(this) || node.isObstructed) continue;
                // ensure node is only within certain distance before making a connection
                if (Vector3.Distance(transform.position, node.transform.position) >= maxDistance) continue;
                // add the connection to connections list
                connections.Add(node);
            }
        }

        void OnDrawGizmos() 
        {
            if (nodeManager == null) return;

            // show node
            if (nodeManager.show_node)
            {
                Gizmos.color = obstructed ? Color.red : Color.green;
                Gizmos.DrawSphere(transform.position, 0.1f);
            }

            // show node connections
            if (!nodeManager.show_connections) return;

            foreach (Node connection in connections)
            {
                Debug.DrawRay(transform.position, connection.transform.position - transform.position, Color.black);
            }
        }
    }
}
