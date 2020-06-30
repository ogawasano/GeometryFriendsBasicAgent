using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents.Search
{
    //Depth First Search
    public class DFS
    {
        public static List<Position> Search(Graph graph, Position initialPosition)
        {
            //Reset visited status
            ResetVertexes(graph);
            //Add initial position to the graph and get the initial node
            Node initialNode = AddInitialPosition(graph, initialPosition);

            //perform search
            List<Node> openNodes = new List<Node>
            {
                initialNode
            };

            while (openNodes.Count > 0)
            {
                //LIFO
                Node currentNode = openNodes[openNodes.Count - 1];
                openNodes.RemoveAt(openNodes.Count - 1);
                //get the nodes the current node is connected to 
                List<Node> children = currentNode.GetChildren();
                foreach(Node child in children)
                {
                    //check if it reached the goal
                    if (child.vertex.goal)
                    {
                        return GetSolutionFromNode(child);
                    }
                    //if not check if this vertex has already been visited
                    if (!child.vertex.visited)
                    {
                        //if not, add node to the open nodes
                        openNodes.Add(child);
                        child.vertex.visited = true;
                    }
                }
            }
            //if the search reaches here, then no solution was found
            return null;
        }

        private static void ResetVertexes(Graph graph)
        {
            foreach(Vertex vertex in graph.vertices)
            {
                vertex.visited = false;
            }
        }

        private static Node AddInitialPosition(Graph graph, Position initialPosition)
        {
            return new Node(graph.AddVertexFromPosition(initialPosition));
        }

        private static List<Position> GetSolutionFromNode(Node node)
        {
            List<Position> solution = new List<Position>();
            Node currentNode = node;
            while(currentNode.parent != null)
            {
                solution.Add(currentNode.vertex.position);
                currentNode = currentNode.parent;
            }
            //add last position
            solution.Add(currentNode.vertex.position);

            return solution;
        }
    }
}
