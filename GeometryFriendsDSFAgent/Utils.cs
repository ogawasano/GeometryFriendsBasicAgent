using GeometryFriendsAgents.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents
{
    public struct Position
    {
        public float X;
        public float Y;
    }

    public class Utils
    {
        public static readonly int circleRadius = 40;

        public static float EuclideanDistance(Position start, Position end)
        {
            //the square root of (X2 - X1)^2 + (Y2 - Y1)^2
            return (float)Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
        }

        public static Vertex CreateNewVertex(Position position, bool goal)
        {
            return new Vertex
            {
                position = new Position
                {
                    X = position.X,
                    Y = position.Y
                },
                goal = goal,
                visited = false,
                edges = new List<Edge>()
            };
        }

        // code adapted from https://rosettacode.org/wiki/Find_the_intersection_of_two_lines#Java
        public static Boolean LineIntersection(Position line1Start, Position line1End, Position line2Start, Position line2End)
        {
            double a1 = line1End.Y - line1Start.Y;
            double b1 = line1Start.X - line1End.X;
            double c1 = a1 * line1Start.X + b1 * line1Start.Y;

            double a2 = line2End.Y - line2Start.Y;
            double b2 = line2Start.X - line2End.X;
            double c2 = a2 * line2Start.X + b2 * line2Start.Y;

            double delta = a1 * b2 - a2 * b1;

            double x = (b2 * c1 - b1 * c2) / delta;
            double y = (a1 * c2 - a2 * c1) / delta;

            if (Double.IsInfinity(x) || Double.IsInfinity(y))
            {
                return false;
            }

            double minX1 = Math.Min(line1Start.X, line1End.X);
            double maxX1 = Math.Max(line1Start.X, line1End.X);
            double minX2 = Math.Min(line2Start.X, line2End.X);
            double maxX2 = Math.Max(line2Start.X, line2End.X);
            double minY1 = Math.Min(line1Start.Y, line1End.Y);
            double maxY1 = Math.Max(line1Start.Y, line1End.Y);
            double minY2 = Math.Min(line2Start.Y, line2End.Y);
            double maxY2 = Math.Max(line2Start.Y, line2End.Y);

            if (!ValueInBetween(x, minX1, maxX1) || !ValueInBetween(x, minX2, maxX2) ||
                !ValueInBetween(y, minY1, maxY1) || !ValueInBetween(y, minY2, maxY2))
            {
                return false;
            }
            return true;
        }

        public static bool ValueInBetween(double value, double min, double max)
        {
            if (value <= max && value >= min)
            {
                return true;
            }
            return false;
        }

        public static bool ReachedPosition(Position nextPosition, Position currentPosition, int xMargin, int yMargin)
        {
            if (Utils.ValueInBetween(currentPosition.X, nextPosition.X - xMargin, nextPosition.X + xMargin) &&
                Utils.ValueInBetween(currentPosition.Y, nextPosition.Y - yMargin, nextPosition.Y + yMargin))
            {
                return true;
            }
            return false;
        }
    }
    
}
