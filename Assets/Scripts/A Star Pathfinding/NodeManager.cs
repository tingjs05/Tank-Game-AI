using System;
using System.Collections.Generic;
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

        [SerializeField] private int maxNodePairs = 1000;
        private List<NodePair> nodePairs = new List<NodePair>();
        private class NodePair
        {
            public Vector3 position;
            public Node node;
            public int numberOfUses;

            public NodePair(Vector3 position, Node node)
            {
                this.position = position;
                this.node = node;
                numberOfUses = 0;
            }
        }

        [HideInInspector] public float? gridFrequency = null;
        public event Action OnUsableNodeUpdate;

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
            // reset node pairs list
            nodePairs.Clear();
            // iterators
            int i;

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
            // try to find node from cache first
            Node foundNode = FindPair(position);
            if (foundNode != null) return foundNode;

            Collider[] nearbyNodeCols = Physics.OverlapSphere(position, nodeDetectionRange, nodeLayerMask);

            Node[] nearbyNodes = nearbyNodeCols
                .Select(x => x.GetComponent<Node>())
                .Where(x => x != null && !x.isObstructed)
                .OrderBy(x => FindManhattanDistance(position, x.transform.position))
                .ToArray();

            if (nearbyNodes == null || nearbyNodes.Length <= 0)
                nearbyNodes = _usableNodes
                    .Select(x => x.node)
                    .OrderBy(x => FindManhattanDistance(position, x.transform.position))
                    .ToArray();
            
            // if there are too many node pairs, remove the one with the least number of uses
            if (nodePairs.Count >= maxNodePairs)
            {
                nodePairs = nodePairs.OrderBy(x => x.numberOfUses).ToList();
                nodePairs.RemoveAt(0);
            }

            // cache node pair
            nodePairs.Add(new NodePair(position, nearbyNodes[0]));
            // return found node
            return nearbyNodes[0];
        }

        Node FindPair(Vector3 newPosition)
        {
            // check if can find pair
            if (gridFrequency == null || nodePairs == null || nodePairs.Count <= 0) return null;
            // search for matching node pair
            nodePairs = nodePairs
                .OrderBy(x => FindManhattanDistance(x.node.transform.position, newPosition))
                .ToList();
            // check if closest node is within range
            if (FindManhattanDistance(nodePairs[0].node.transform.position, newPosition) > gridFrequency) return null;
            // increment uses by 1
            nodePairs[0].numberOfUses++;
            // return node pair
            return nodePairs[0].node;
        }

        int FindManhattanDistance(Vector3 start, Vector3 end)
        {
            // return Vector3.Distance(start, end);
            return Mathf.Abs(ConvertToInt(end.x) - ConvertToInt(start.x)) + 
                Mathf.Abs(ConvertToInt(end.z) - ConvertToInt(start.z));
        }

        int ConvertToInt(float num)
        {
            return (int) Mathf.Round(num * 10);
        }
    }
}
