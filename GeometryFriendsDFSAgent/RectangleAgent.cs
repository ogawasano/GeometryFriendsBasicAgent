using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriendsAgents.Control;
using GeometryFriendsAgents.Search;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A rectangle agent implementation for the GeometryFriends game that demonstrates simple random action selection.
    /// </summary>
    public class RectangleAgent : AbstractRectangleAgent
    {
        //agent implementation specificiation
        private bool implementedAgent;
        private string agentName = "BasicRectangle";

        //auxiliary variables for agent action
        private Moves currentAction;
        private List<Moves> possibleMoves;
        private Random rnd;

        //debug agent predictions and history keeping
        private List<CollectibleRepresentation> caughtDiamonds;
        private List<CollectibleRepresentation> uncaughtDiamonds;
        private List<CollectibleRepresentation> remaining;
        private DebugInformation[] debugInfo;

        //Sensors Information
        private CountInformation numbersInfo;
        private RectangleRepresentation rectangle;
        private CircleRepresentation circle;
        private List<ObstacleRepresentation> platforms;
        private List<ObstacleRepresentation> rectanglePlatforms;
        private List<ObstacleRepresentation> circlePlatforms;
        private List<CollectibleRepresentation> diamonds;


        private int diamondsLeft;

        private List<AgentMessage> messages;

        //Search
        private float maxMorphDistance = 70; //the maximum vertical distance the rectangle can morph (aproximately)
        private Graph graph;
        private List<Position> path;

        //Control
        private int remainingDiamonds;

        //Area of the game screen
        protected Rectangle area;

        public RectangleAgent()
        {
            //Change flag if agent is not to be used
            implementedAgent = true;

            currentAction = Moves.NO_ACTION;
            rnd = new Random();

            //prepare the possible moves  
            possibleMoves = new List<Moves>
            {
                Moves.MOVE_LEFT,
                Moves.MOVE_RIGHT,
                Moves.MORPH_UP,
                Moves.MORPH_DOWN
            };

            //history keeping
            uncaughtDiamonds = new List<CollectibleRepresentation>();
            caughtDiamonds = new List<CollectibleRepresentation>();
            remaining = new List<CollectibleRepresentation>();

            //messages exchange
            messages = new List<AgentMessage>();
        }

        //implements abstract rectangle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            numbersInfo = nI;
            diamondsLeft = nI.CollectiblesCount;
            rectangle = rI;
            circle = cI;
            platforms = oI.ToList<ObstacleRepresentation>();
            rectanglePlatforms = rPI.ToList<ObstacleRepresentation>(); ;
            circlePlatforms = cPI.ToList<ObstacleRepresentation>(); ;
            diamonds = colI.ToList<CollectibleRepresentation>(); ;
            uncaughtDiamonds = new List<CollectibleRepresentation>(diamonds);
            remainingDiamonds = uncaughtDiamonds.Count();
            this.area = area;

            //join the list of the platforms the rectangle considers
            List<ObstacleRepresentation> allCharacterPlatforms = new List<ObstacleRepresentation>();
            allCharacterPlatforms.AddRange(platforms);
            allCharacterPlatforms.AddRange(circlePlatforms);

            //generate search graph
            graph = new Graph(allCharacterPlatforms, diamonds, maxMorphDistance);
        }

        //implements abstract rectangle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            diamondsLeft = nC;

            rectangle = rI;
            circle = cI;
            diamonds = colI.ToList<CollectibleRepresentation>();
        }

        //implements abstract rectangle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return implementedAgent;
        }

        //implements abstract rectangle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return agentName;
        }

        //simple algorithm for choosing a random action for the rectangle agent
        private void RandomAction()
        {
            currentAction = possibleMoves[rnd.Next(possibleMoves.Count)];

            //send a message to the circle agent telling what action it chose
            messages.Add(new AgentMessage("Going to :" + currentAction));
        }

        //implements abstract rectangle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            return currentAction;
        }

        //implements abstract rectangle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {
            CheckNewlyCaughtColectibles();

            //while there is no path, search for one
            if (path == null || path.Count == 0)
            {
                path = DFS.Search(graph.Clone(), new Position { X = rectangle.X, Y = rectangle.Y });
                //if there is still no path found, perform a random action
                if (path == null || path.Count == 0)
                {
                    RandomAction();
                    return;
                }
            }
            //when a path is found, follow it
            FollowPath();
        }

        private void FollowPath()
        {
            Position currentPosition = new Position { X = rectangle.X, Y = rectangle.Y };
            //check if the desired position of the path node has been reached
            if (Utils.ReachedPosition(path[path.Count - 1], currentPosition, RectangleController.XMargin, RectangleController.YMargin))
            {
                //if so, change the current subgoal to the next path node
                Position position = path[path.Count - 1];
                path.RemoveAt(path.Count - 1);
                if (path.Count == 0 && remainingDiamonds > uncaughtDiamonds.Count)
                {
                    graph.RemoveVertexGoalFromPosition(position);
                }
            }
            //If not, get the necessary action to reach it
            currentAction = RectangleController.GetNextAction(path[path.Count - 1], currentPosition);
        }

        private void CheckNewlyCaughtColectibles()
        {
            //check if any collectible was caught
            lock (remaining)
            {
                if (remaining.Count > 0)
                {
                    List<CollectibleRepresentation> toRemove = new List<CollectibleRepresentation>();
                    foreach (CollectibleRepresentation item in uncaughtDiamonds)
                    {
                        if (!remaining.Contains(item))
                        {
                            caughtDiamonds.Add(item);
                            toRemove.Add(item);
                        }
                    }
                    foreach (CollectibleRepresentation item in toRemove)
                    {
                        uncaughtDiamonds.Remove(item);
                    }
                }
            }
        }

        //implements abstract rectangle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Log.LogInformation("RECTANGLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
        }

        //implememts abstract agent interface: send messages to the circle agent
        public override List<GeometryFriends.AI.Communication.AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the circle agent
        public override void HandleAgentMessages(List<GeometryFriends.AI.Communication.AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                Log.LogInformation("Rectangle: received message from circle: " + item.Message);
                if (item.Attachment != null)
                {
                    Log.LogInformation("Received message has attachment: " + item.Attachment.ToString());
                    if (item.Attachment.GetType() == typeof(Pen))
                    {
                        Log.LogInformation("The attachment is a pen, let's see its color: " + ((Pen)item.Attachment).Color.ToString());
                    }
                }
            }
        }

        //implements abstract circle interface: gets the debug information that is to be visually represented by the agents manager
        public override DebugInformation[] GetDebugInformation()
        {
            //get graph debug information
            List<DebugInformation> debugInformation = new List<DebugInformation>();
            debugInformation.Add(DebugInformationFactory.CreateClearDebugInfo());
            debugInformation.AddRange(graph.debugInformation);
            debugInfo = debugInformation.ToArray();
            return debugInfo;
        }
    }
}