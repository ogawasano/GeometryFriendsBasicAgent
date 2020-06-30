using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.ActionSimulation;
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
    /// A circle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class CircleAgent : AbstractCircleAgent
    {
        //agent implementation specificiation
        private bool implementedAgent;
        private string agentName = "BasicCircle";

        //auxiliary variables for agent action
        private Moves currentAction;
        private List<Moves> possibleMoves;
        private Random rnd;

        //debug agent predictions and history keeping
        private List<CollectibleRepresentation> caughtDiamonds;
        private List<CollectibleRepresentation> uncaughtDiamonds;
        private object remainingInfoLock = new Object();
        private List<CollectibleRepresentation> remaining;
        private DebugInformation[] debugInfo;

        //Sensors Information and level state
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
        private float maxJumpDistance = 200; //the maximum vertical distance the circle can jump (aproximately)
        private Graph graph;
        private List<Position> path;

        //Control
        private int remainingDiamonds;

        //Area of the game screen
        private Rectangle area;

        public CircleAgent()
        {
            //Change flag if agent is not to be used
            implementedAgent = true;
            rnd = new Random();

            //setup for action updates
            currentAction = Moves.NO_ACTION;

            //prepare the possible moves  
            possibleMoves = new List<Moves>
            {
                Moves.ROLL_LEFT,
                Moves.ROLL_RIGHT,
                Moves.JUMP
            };

            //history keeping
            uncaughtDiamonds = new List<CollectibleRepresentation>();
            caughtDiamonds = new List<CollectibleRepresentation>();
            remaining = new List<CollectibleRepresentation>();

            //messages exchange
            messages = new List<AgentMessage>();
        }

        //implements abstract circle interface: used to setup the initial information so that the agent has basic knowledge about the level
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

            //join the list of the platforms the circle considers
            List<ObstacleRepresentation> allCharacterPlatforms = new List<ObstacleRepresentation>();
            allCharacterPlatforms.AddRange(platforms);
            allCharacterPlatforms.AddRange(rectanglePlatforms);

            //generate search graph
            graph = new Graph(allCharacterPlatforms, diamonds, maxJumpDistance);            
        }

        //implements abstract circle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        /*WARNING: this method is called independently from the agent update - Update(TimeSpan elapsedGameTime) - so care should be taken when using complex 
         * structures that are modified in both (e.g. see operation on the "remaining" collection)      
         */
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            diamondsLeft = nC;

            rectangle = rI;
            circle = cI;
            diamonds = colI.ToList<CollectibleRepresentation>();
            lock (remaining)
            {
                remaining = new List<CollectibleRepresentation>(diamonds);
            }
        }

        //implements abstract circle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return implementedAgent;
        }

        //implements abstract circle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return agentName;
        }

        //implements abstract circle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            return currentAction;
        }

        //implements abstract circle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {
            //check if a diamond was caught
            CheckNewlyCaughtColectibles();

            //while there is no path, search for one using a DFS search
            if (path == null || path.Count == 0)
            {
                path = DFS.Search(graph.Clone(), new Position { X = circle.X, Y = circle.Y });
                //if there is still no path found, perform a random action to try position the agent so it is possible to find a solution
                if(path == null || path.Count == 0)
                {
                    RandomAction();
                    return;
                }   
            }
            //when a path is found, follow it
            FollowPath();
            
        }

        private void RandomAction()
        {
            currentAction = possibleMoves[rnd.Next(possibleMoves.Count)];
        }

        private void FollowPath()
        {
            Position currentPosition = new Position { X = circle.X, Y = circle.Y };
            //check if the desired position of the path node has been reached
            if (Utils.ReachedPosition(path[path.Count-1], currentPosition, CircleController.XMargin, CircleController.YMargin))
            {
                //if so, change the current subgoal to the next path node
                Position position = path[path.Count - 1];
                path.RemoveAt(path.Count - 1);
                if(path.Count == 0 && remainingDiamonds > uncaughtDiamonds.Count)
                {
                    graph.RemoveVertexGoalFromPosition(position);
                }
            }
            //If not, get the necessary action to reach it
            currentAction = CircleController.GetNextAction(path[path.Count - 1], currentPosition);
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

        //typically used console debugging used in previous implementations of GeometryFriends
        protected void DebugSensorsInfo()
        {
            Log.LogInformation("Circle Agent - " + numbersInfo.ToString());

            Log.LogInformation("Circle Agent - " + rectangle.ToString());

            Log.LogInformation("Circle Agent - " + circle.ToString());

            foreach (ObstacleRepresentation i in platforms)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Obstacle"));
            }

            foreach (ObstacleRepresentation i in rectanglePlatforms)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Rectangle Platform"));
            }

            foreach (ObstacleRepresentation i in circlePlatforms)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Circle Platform"));
            }

            foreach (CollectibleRepresentation i in diamonds)
            {
                Log.LogInformation("Circle Agent - " + i.ToString());
            }
        }

        //implements abstract circle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Log.LogInformation("CIRCLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
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

        //implememts abstract agent interface: send messages to the rectangle agent
        public override List<GeometryFriends.AI.Communication.AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the rectangle agent
        public override void HandleAgentMessages(List<GeometryFriends.AI.Communication.AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                Log.LogInformation("Circle: received message from rectangle: " + item.Message);
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
    }
}

