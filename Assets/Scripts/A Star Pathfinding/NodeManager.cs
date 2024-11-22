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
            Node bestNode = FindPair(position);
            if (bestNode != null) return bestNode;

            Collider[] nearbyNodeCols = Physics.OverlapSphere(position, nodeDetectionRange, nodeLayerMask);
            
            float? bestDist = null;
            float currDist;
            Node currNode;

            for (int i = 0; i < nearbyNodeCols.Length; i++)
            {
                currNode = nearbyNodeCols[i].GetComponent<Node>();
                // filter out obstructed nodes
                if (currNode == null || currNode.isObstructed) continue;
                // calculate distance from node
                currDist = FindManhattanDistance(position, nearbyNodeCols[i].transform.position);
                // save the closest node
                if (bestDist != null && currDist >= bestDist) continue;
                bestNode = currNode;
                bestDist = currDist;
            }

            if (bestNode == null)
            {
                bestDist = null;

                for (int i = 0; i < _usableNodes.Length; i++)
                {
                    // filter out obstructed nodes
                    if (_usableNodes[i].node == null || _usableNodes[i].node.isObstructed) continue;
                    // calculate distance from node
                    currDist = FindManhattanDistance(position, _usableNodes[i].node.transform.position);
                    // save the closest node
                    if (bestDist != null && currDist >= bestDist) continue;
                    bestNode = _usableNodes[i].node;
                    bestDist = currDist;
                }
            }
            
            // if there are too many node pairs, remove the one with the least number of uses
            if (nodePairs.Count >= maxNodePairs)
            {
                int leastUseIndex = 0;

                for (int i = 1; i < nodePairs.Count - 1; i++)
                {
                    if (nodePairs[i].numberOfUses > nodePairs[leastUseIndex].numberOfUses) continue;
                    leastUseIndex = i;
                }

                nodePairs.RemoveAt(leastUseIndex);
            }

            // cache node pair
            nodePairs.Add(new NodePair(position, bestNode));
            // return best node
            return bestNode;
        }

        Node FindPair(Vector3 newPosition)
        {
            // check if can find pair
            if (gridFrequency == null || nodePairs == null || nodePairs.Count <= 0) return null;
            // search for matching node pair
            int bestNodePairIndex = 0;
            float bestDist = FindManhattanDistance(newPosition, nodePairs[0].node.transform.position);
            float currDist;

            for (int i = 1; i < nodePairs.Count - 1; i++)
            {
                // calculate distance from node
                currDist = FindManhattanDistance(newPosition, nodePairs[i].node.transform.position);
                // save the closest node
                if (currDist >= bestDist) continue;
                bestNodePairIndex = i;
                bestDist = currDist;
            }

            // check if closest node is within range
            if (bestDist > gridFrequency) return null;
            // increment uses by 1
            nodePairs[bestNodePairIndex].numberOfUses++;
            // return node pair
            return nodePairs[bestNodePairIndex].node;
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
