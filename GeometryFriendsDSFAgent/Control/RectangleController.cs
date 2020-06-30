using GeometryFriends.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents.Control
{
    public class RectangleController
    {
        public static readonly int XMargin = 70;
        public static readonly int YMargin = 50;
        private static readonly int AboveMargin = 70;
        private static readonly int MorphXMargin = 200;

        public static Moves GetNextAction(Position nextPosition, Position currentPosition)
        {
            //if the next position is above the current one roll to the next position direction and jump when close to it 
            if (currentPosition.Y - nextPosition.Y >= AboveMargin &&
                Utils.ValueInBetween(currentPosition.X, nextPosition.X - MorphXMargin, nextPosition.X + MorphXMargin))
            {
                return Moves.MORPH_UP;
            }
            //roll to the next position direction
            if (currentPosition.X < nextPosition.X)
            {
                return Moves.MOVE_RIGHT;
            }
            return Moves.MOVE_LEFT;
        }
    }
}
