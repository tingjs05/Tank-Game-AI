using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astar
{
    public class Pathfinding
    {
        // list of nodes stored locally
        public PathNode[] nodes => NodeManager.Instance.UsableNodes == null ? null : NodeManager.Instance.UsableNodes;

        // lists to store nodes that have been visited
        public List<PathNode> open { get; private set; } = new List<PathNode>();
        public List<PathNode> closed { get; private set; } = new List<PathNode>();

        // list to store path
        List<PathNode> path = new List<PathNode>();

        // store start and end node after converting position => node
        PathNode startNode, endNode, connectionNode;
        // boolean to control whether or not a path is found
        bool pathFound;

        public Pathfinding()
        {
            // ensure node manager is not null
            if (NodeManager.Instance == null)
            {
                Debug.LogError($"{this}, Pathfinding.cs: NodeManager instance is null! Unable to initialize pathfinding. ");
                return;
            }

            // ensure nodes are generated
            if (NodeManager.Instance.UsableNodes == null || NodeManager.Instance.UsableNodes.Length <= 0)
            {
                Debug.LogWarning($"{this}, Pathfinding.cs: Nodes could not be found, unable to initialize pathfinding. ");
                return;
            }
        }

        // public method to find path from start to end vector
        public List<PathNode> FindPath(Vector3 startPosition, Vector3 endPosition)
        {
            // ensure node manager is not null
            if (NodeManager.Instance == null)
            {
                Debug.LogError($"{this}, Pathfinding.cs: NodeManager instance is null! Unable to find path. ");
                return null;
            }

            // ensure nodes are generated
            if (nodes == null || nodes.Length <= 0)
            {
                Debug.LogWarning($"{this}, Pathfinding.cs: FindPath() was called before nodes are generated! Process has been terminated. ");
                return null;
            }

            // get start and end nodes
            startNode = new PathNode(NodeManager.Instance.GetNearestNode(startPosition));
            endNode = new PathNode(NodeManager.Instance.GetNearestNode(endPosition));

            // ensure start and end node can be found
            if (startNode.node == null || endNode.node == null)
            {
                Debug.LogWarning($"{this}, Pathfinding.cs: Nodes near the start or end position could not be found, process has been terminated. ");
                return null;
            }

            // reset all other variables to setup for new path find
            Reset();
            // add start node to open list
            open.Add(startNode);

            // find path
            // limit loop to number of nodes
            for (int i = 0; i < NodeManager.Instance.UsableNodes.Length; i++)
            {
                // stop when path is found
                if (pathFound) break;
                // check if opened nodes list is empty, if so abort operation
                if (open.Count <= 0) return null;
                // sort open list based on distance to end point
                open = open.OrderBy(x => GetCost(x)).ToList();
                // open the closest node to the end point
                OpenNode(open[0]);
            }
            // check if path has been found sucessfully
            if (!pathFound) return null;

            // calculate path
            path.Add(endNode);
            // reset path found boolean
            pathFound = false;
            // calculate path
            // limit loop to number of nodes
            for (int i = 0; i < NodeManager.Instance.UsableNodes.Length; i++)
            {
                // stop when path is found
                if (pathFound) break;
                // calculate path back to starting node
                CalculatePath(path[0]);
            }
            // check if path has been found sucessfully
            if (!pathFound) return null;
            // return path
            return path;
        }

        // public method to reset opend and closed lists
        public void ResetLists()
        {
            open.Clear();
            closed.Clear();
        }

        // method to reset variables before a new path find
        void Reset()
        {
            // reset boolean
            pathFound = false;
            // reset open and closed lists
            ResetLists();
            // reset path list
            path.Clear();
            // reset the previous node of the start node
            startNode.previousNode = null;
            // set G and H value of starting node
            startNode.G = 0;
            startNode.H = FindManhattanDistance(startNode.node.transform.position, endNode.node.transform.position);
        }

        // code to "open" a node, and check it out
        void OpenNode(PathNode node)
        {
            // remove from open list since its already opened
            open.Remove(node);

            // add all connected nodes to open list
            foreach (Node connection in node.node.nodeConnections)
            {
                // convert node to path node
                connectionNode = nodes.Where(x => x.node == connection).ToArray()[0];

                // if connection is the end point, set the previous node as current node
                // and mark path found as true
                if (connectionNode.node == endNode.node)
                {
                    // set previous node as current node
                    endNode.previousNode = node;
                    // complete path find
                    pathFound = true;
                    // break out of loop when found end node, no need to continue searching
                    return;
                }

                // do not check connection if connection node is already opened before
                if (closed.Contains(connectionNode)) continue;

                // find if the node from the connection is already known
                if (open.Contains(connectionNode))
                {
                    // if the current node is cheaper than the connection's previous node
                    // change the connection node's previous node connection to current node
                    if (node.G < connectionNode.previousNode.G) MakeConnection(node, connectionNode);
                    // do not add connection to open if it is already known
                    continue;
                }
                // set connection to current node
                MakeConnection(node, connectionNode);
                // if node is not seen before, add to open list
                open.Add(connectionNode);
            }
            // move node to closed list after visiting it
            closed.Add(node);
        }

        // code to trace previous nodes from end node to generate path leading to start node
        void CalculatePath(PathNode node)
        {
            // // ensure a previous node is set, assuming it is not the starting node
            if (node.previousNode == null && node.node != startNode.node)
            {
                Debug.LogError("Pathfinding.cs: path calculation failed due to null node. ");
                return;
            }
            // end path calculation if current node is start node
            if (node.node == startNode.node)
                pathFound = true;
            // otherwise add the previous node into the list
            else
                path.Insert(0, node.previousNode);
        }

        // method to make a connection between two nodes
        void MakeConnection(PathNode node, PathNode connection)
        {
            // set G and H values of connection nodes
            // get H value which is manhattan distance to end node
            connection.H = FindManhattanDistance(connection.node.transform.position, endNode.node.transform.position);
            // get G value, previous node (current node making the connection) + distance travelled from previous node
            connection.G = node.G + FindManhattanDistance(node.node.transform.position, connection.node.transform.position);
            // make the connection to the current node
            connection.previousNode = node;
        }

        // methods to find cost of node
        int GetCost(PathNode node)
        {
            return node.G + node.H;
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
