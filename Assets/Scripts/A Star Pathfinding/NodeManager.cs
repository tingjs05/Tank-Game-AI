using System;
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

        private PathNode[] _usableNodes;
        public PathNode[] UsableNodes
        {
            get
            {
                if (_usableNodes == null) UpdateNodes();
                return _usableNodes;
            }
        }

        [HideInInspector] public float? gridFrequency = null;
        public event Action OnUsableNodeUpdate;

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
            _nodes = FindObjectsOfType<Node>();
            UpdateObstructedNodes();
        }

        public void UpdateObstructedNodes()
        {
            // check each node for obstruction
            foreach(Node node in _nodes)
            {
                node.CheckObstructed();
            }

            // update usable nodes
            _usableNodes = _nodes
                .Where(x => !x.isObstructed)
                .Select(x => new PathNode(x))
                .ToArray();
            
            // check if need to regenerate connections
            if (gridFrequency != null)
            {
                foreach (PathNode node in _usableNodes)
                {
                    node.node.GenerateConnections((float) gridFrequency);
                }
            }

            // invoke event when updating usable node
            OnUsableNodeUpdate?.Invoke();
        }

        public Node GetNearestNode(Vector3 position)
        {
            Collider[] nearbyNodeCols = Physics.OverlapSphere(position, nodeDetectionRange, nodeLayerMask);

            Node[] nearbyNodes = nearbyNodeCols
                .Select(x => x.GetComponent<Node>())
                .Where(x => x != null && !x.isObstructed)
                .OrderBy(x => Vector3.Distance(position, x.transform.position))
                .ToArray();

            if (nearbyNodes == null || nearbyNodes.Length <= 0)
                nearbyNodes = _usableNodes
                    .Select(x => x.node)
                    .OrderBy(x => Vector3.Distance(position, x.transform.position))
                    .ToArray();
            
            return nearbyNodes[0];
        }
    }
}
