using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents.Search
{
    public class Graph
    {
        public float maxVerticalDistance;
        public List<Vertex> vertices = new List<Vertex>(); //all nodes of the graph
        public List<ObstacleRepresentation> platforms;
        public List<DebugInformation> debugInformation = new List<DebugInformation>();

        private readonly int vertexRadius = 10;
        private readonly GeometryFriends.XNAStub.Color vertexColor = GeometryFriends.XNAStub.Color.Red;
        private readonly GeometryFriends.XNAStub.Color edgeColor = GeometryFriends.XNAStub.Color.Coral;

        public Graph(List<ObstacleRepresentation> platforms, List<CollectibleRepresentation> diamonds, float maxVerticalDistance)
        {
            this.maxVerticalDistance = maxVerticalDistance;
            this.platforms = platforms;
            //Generate the vertices of the graph according to the objects of the level
            GenerateVertices(platforms, diamonds);
            //Check the distances between the vertices and connect the ones that are apparently possible to connect
            GenerateEdges();
            //Get debug information to draw the graph
            GetDebugInformation();
        }

        private Graph()
        {
            //for clonning
        }

        public Vertex AddVertexFromPosition(Position position)
        {
            //create and add vertex
            Vertex vertex = Utils.CreateNewVertex(position, false);
            //create edges
            foreach(Vertex otherVertex in vertices)
            {
                //ignore self
                if (otherVertex.position.X == position.X && otherVertex.position.Y == position.Y)
                {
                    continue;
                }
                //check if possible to connect to the other vertex
                if(PossibleToConnect(vertex, otherVertex))
                {
                    //if so, add edge
                    float dist = Utils.EuclideanDistance(position, otherVertex.position);
                    vertex.edges.Add(new Edge
                    {
                        start = vertex,
                        end = otherVertex,
                        distance = dist
                    });
                }
            }
            return vertex;
        }

        //to use when a diamond is caught and the graph has to be updated without it as a goal
        public void RemoveVertexGoalFromPosition(Position position)
        {
            Vertex vertexToRemove = null;
            //get the vertex to remove and remove the connections to it
            foreach(Vertex vertex in vertices)
            {
                //check if this vertex is the one to remove
                if(vertex.goal && vertex.position.X == position.X && vertex.position.Y == position.Y)
                {
                    vertexToRemove = vertex;
                    continue;
                }
                //get edges connected to the vertex to remove
                List<Edge> edgesToRemove = new List<Edge>();
                foreach(Edge edge in vertex.edges)
                {
                    if(edge.end.goal && edge.end.position.X == position.X && edge.end.position.Y == position.Y)
                    {
                        edgesToRemove.Add(edge);
                    }
                }
                //remove edges
                foreach(Edge edge in edgesToRemove)
                {
                    vertex.edges.Remove(edge);
                }
            }
            if(vertexToRemove != null)
            {
                //remove the vertex
                vertices.Remove(vertexToRemove);
            }
            
        }

        public Graph Clone()
        {
            Graph clone = new Graph
            {
                maxVerticalDistance = this.maxVerticalDistance,
                platforms = this.platforms
            };
            foreach (Vertex vertex in vertices)
            {
                clone.vertices.Add(new Vertex
                {
                    position = new Position
                    {
                        X = vertex.position.X,
                        Y = vertex.position.Y
                    },
                    goal = vertex.goal,
                    visited = vertex.visited,
                    edges = new List<Edge>()
                });
            }
            clone.GenerateEdges();
            return clone;
        }

        private void GenerateVertices(List<ObstacleRepresentation> platforms, List<CollectibleRepresentation> diamonds)
        {
            //create a vertex at each side of the ground platform
            vertices.Add(Utils.CreateNewVertex(new Position
            {
                X = 50,
                Y = 740
            }, false));
            vertices.Add(Utils.CreateNewVertex(new Position
            {
                X = 1200,
                Y = 740
            }, false));
            //create a vertex at the sides of each platform 
            foreach (ObstacleRepresentation platform in platforms)
            {
                //right vertex
                vertices.Add(Utils.CreateNewVertex(new Position
                {
                    X = platform.X + platform.Width / 2,
                    Y = platform.Y - platform.Height / 2 - Utils.circleRadius
                }, false));
                //left vertex
                vertices.Add(Utils.CreateNewVertex(new Position
                {
                    X = platform.X - platform.Width / 2,
                    Y = platform.Y - platform.Height / 2 - Utils.circleRadius
                }, false));
            }
            //create a vertex at the position of each diamond
            foreach(CollectibleRepresentation diamond in diamonds)
            {
                vertices.Add(Utils.CreateNewVertex(new Position
                {
                    X = diamond.X,
                    Y = diamond.Y
                }, true));
            }
        }

        private void GenerateEdges()
        {
            //check which vertices can connect to which
            for(int vertex = 0; vertex < vertices.Count; vertex++)
            {
                for(int vertexAux = 0; vertexAux < vertices.Count; vertexAux++)
                {
                    //do not connect a vertex to itself
                    if(vertex == vertexAux)
                    {
                        continue;
                    }
                    //check if is apparently possible to go from the vertex to the vertexAux
                    if(PossibleToConnect(vertices[vertex], vertices[vertexAux])){
                        //if so, get distance and create edge
                        float dist = Utils.EuclideanDistance(vertices[vertex].position, vertices[vertexAux].position);
                        vertices[vertex].edges.Add(new Edge
                        {
                            start = vertices[vertex],
                            end = vertices[vertexAux],
                            distance = dist
                        });
                    }
                }
            }
        }

        private bool PossibleToConnect(Vertex start, Vertex end)
        {
            //if the vertical distance is higher than the maximum height, then it is not possible to connect
            //negative distance means it is from above to bellow, so it should be possible as long as there are no obstacles preventing so
            if(start.position.Y - end.position.Y > maxVerticalDistance)
            {
                return false;
            }
            //check if there are any obstacles preventing the connection
            foreach(ObstacleRepresentation platform in platforms)
            {
                Position platformRight = new Position
                {
                    X = platform.X + platform.Width / 2,
                    Y = platform.Y - platform.Height / 2
                };

                Position platformLeft = new Position
                {
                    X = platform.X - platform.Width / 2,
                    Y = platform.Y - platform.Height / 2
                };

                if (Utils.LineIntersection(start.position, end.position, platformRight, platformLeft))
                {
                    return false;
                }
            }
            return true;
        }

        private void GetDebugInformation()
        {
            //draw vertices 
            foreach(Vertex vertex in vertices)
            {
                debugInformation.Add(DebugInformationFactory.CreateCircleDebugInfo(new PointF(vertex.position.X, vertex.position.Y), vertexRadius, vertexColor));
                //draw edges
                foreach(Edge edge in vertex.edges)
                {
                    debugInformation.Add(DebugInformationFactory.CreateLineDebugInfo(new PointF(edge.start.position.X, edge.start.position.Y), new PointF(edge.end.position.X, edge.end.position.Y), edgeColor));
                }
            }
        }
    }
}
