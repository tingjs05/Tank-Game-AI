using UnityEngine;

namespace Astar
{
    public class NodeManager : MonoBehaviour
    {
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

        public void Awake()
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
        }
    }
}
