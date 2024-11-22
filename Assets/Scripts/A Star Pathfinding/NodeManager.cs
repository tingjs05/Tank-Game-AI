using System;
using System.Linq;
using UnityEngine;
using EasyButtons;

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

        [Header("Nodes")]
        [SerializeField] private Node[] _nodes;
        public Node[] Nodes
        {
            get 
            {
                if (_nodes == null) UpdateNodes();
                return _nodes;
            }
        }

        [SerializeField] private PathNode[] _usableNodes;
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

        // iterators
        int i, j;

        void Awake()
        {
            Instantiate();

            if ((_nodes != null && _nodes.Length > 0) && 
                (_usableNodes != null && _usableNodes.Length > 0))
                    return;

            UpdateNodes();
        }

        [Button]
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

        [Button]
        public void UpdateNodes()
        {
            _nodes = FindObjectsOfType<Node>();
            UpdateObstructedNodes();
        }

        [Button]
        public void UpdateObstructedNodes()
        {
            // check each node for obstruction
            for (i = 0; i < _nodes.Length; i++)
                _nodes[i].CheckObstructed();
            
            // check if need to regenerate connections
            if (gridFrequency != null)
            {
                for (i = 0; i < _nodes.Length; i++)
                {
                    // skip obstructed nodes
                    if (_nodes[i].isObstructed) continue;
                    _nodes[i].GenerateConnections((float) gridFrequency);
                }
            }

            // update usable nodes after generating connections
            _usableNodes = _nodes
                .Where(x => !x.isObstructed)
                .Select(x => new PathNode(x))
                .ToArray();

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
