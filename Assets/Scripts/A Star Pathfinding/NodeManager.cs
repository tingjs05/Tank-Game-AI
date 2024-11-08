using System.Linq;
using UnityEngine;

namespace Astar
{
    public class NodeManager : MonoBehaviour
    {
        [Header("Node Detection")]
        [SerializeField] private float nodeDetectionRange = 1.5f;
        [SerializeField] private LayerMask nodeLayerMask;

        [Header("Gizmos")]
        [SerializeField] private bool showNode = true;
        [SerializeField] private bool showConnections = false;

        #region Gizmos Setting Properties
        public bool show_node => showNode;
        public bool show_connections => showConnections;
        #endregion

        public static NodeManager Instance { get; private set; }

        private Node[] _nodes;
        public Node[] Nodes
        {
            get 
            {
                if (_nodes == null) UpdateNodes();
                return _nodes;
            }
        }

        private Node[] _usableNodes;
        public Node[] UsableNodes
        {
            get
            {
                if (_usableNodes == null) UpdateNodes();
                return _usableNodes;
            }
        }

        void Awake()
        {
            Instantiate();
            UpdateNodes();
        }

        public void Instantiate()
        {
            // create a singleton, only allow one node manager to exist at once
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"{this}, NodeManager.cs: More than one NodeManager instance was found! ");
                gameObject.SetActive(false);
            }
        }

        public void UpdateNodes()
        {
            // get all objects with the node component
            _nodes = FindObjectsOfType<Node>();

            // check each node for obstruction
            foreach(Node node in _nodes)
            {
                node.CheckObstructed();
            }

            // update usable nodes
            _usableNodes = _nodes.Where(x => !x.isObstructed).ToArray();
        }

        public Node GetNearestNode(Vector3 position)
        {
            Collider[] nearbyNodes = Physics.OverlapSphere(position, nodeDetectionRange, nodeLayerMask);
            nearbyNodes = nearbyNodes.OrderBy(x => Vector3.Distance(position, x.transform.position)).ToArray();

            foreach (Collider nodeCol in nearbyNodes)
            {
                if (nodeCol.TryGetComponent<Node>(out Node node)) return node;
            }

            return null;
        }
    }
}
