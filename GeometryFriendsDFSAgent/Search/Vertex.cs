using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents.Search
{
    public class Vertex
    {
        public bool goal;
        public bool visited;
        public Position position;
        public List<Edge> edges;
    }
}
