using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents.Search
{
    public class Node
    {
        public bool visited;
        public Vertex vertex;
        public Node parent;

        public Node(Vertex vertex)
        {
            this.vertex = vertex;
            this.visited = false;
        }

        public List<Node> GetChildren()
        {
            List<Node> children = new List<Node>();
            foreach(Edge edge in vertex.edges)
            {
                Node child = new Node(edge.end)
                {
                    parent = this
                };
                children.Add(child);
            }
            return children;
        }

    }
}
