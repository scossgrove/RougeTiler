using System;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public class Direction : VectorBase
    {
        public static Direction None = new Direction(0, 0);
        public static Direction North = new Direction(0, -1);
        public static Direction NorthEast = new Direction(1, -1);
        public static Direction East = new Direction(1, 0);
        public static Direction SouthEast = new Direction(1, 1);
        public static Direction South = new Direction(0, 1);
        public static Direction SouthWest = new Direction(-1, 1);
        public static Direction West = new Direction(-1, 0);
        public static Direction NorthWest = new Direction(-1, -1);

        /// The eight cardinal and intercardinal directions.
        public static Direction[] All = {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest};

        /// The four cardinal directions: north, south, east, and west.
        public static Direction[] Cardinal = {North, East, South, West};

        /// The four directions between the cardinal ones: northwest, northeast,
        /// southwest and southeast.
        public static Direction[] InterCardinal = {NorthEast, SouthEast, SouthWest, NorthWest};

        public Direction(int x, int y) : base(x, y)
        {
        }

        public Direction rotateLeft45()
        {
            if (this == None)
            {
                return None;
            }
            if (this == North)
            {
                return NorthWest;
            }
            if (this == NorthEast)
            {
                return North;
            }
            if (this == East)
            {
                return NorthEast;
            }
            if (this == SouthEast)
            {
                return East;
            }
            if (this == South)
            {
                return SouthEast;
            }
            if (this == SouthWest)
            {
                return South;
            }
            if (this == West)
            {
                return SouthWest;
            }
            if (this == NorthWest)
            {
                return West;
            }
            throw new ApplicationException("unreachable");
        }

        public Direction rotateRight45()
        {
            if (this == None)
            {
                return None;
            }
            if (this == North)
            {
                return NorthEast;
            }
            if (this == NorthEast)
            {
                return East;
            }
            if (this == East)
            {
                return SouthEast;
            }
            if (this == SouthEast)
            {
                return South;
            }
            if (this == South)
            {
                return SouthWest;
            }
            if (this == SouthWest)
            {
                return West;
            }
            if (this == West)
            {
                return NorthWest;
            }
            if (this == NorthWest)
            {
                return North;
            }
            throw new ApplicationException("unreachable");
        }

        public Direction rotateLeft90()
        {
            if (this == None)
            {
                return None;
            }
            if (this == North)
            {
                return West;
            }
            if (this == NorthEast)
            {
                return NorthWest;
            }
            if (this == East)
            {
                return North;
            }
            if (this == SouthEast)
            {
                return NorthEast;
            }
            if (this == South)
            {
                return East;
            }
            if (this == SouthWest)
            {
                return SouthEast;
            }
            if (this == West)
            {
                return South;
            }
            if (this == NorthWest)
            {
                return SouthWest;
            }
            throw new ApplicationException("unreachable");
        }

        public Direction rotateRight90()
        {
            if (this == None)
            {
                return None;
            }
            if (this == North)
            {
                return East;
            }
            if (this == NorthEast)
            {
                return SouthEast;
            }
            if (this == East)
            {
                return South;
            }
            if (this == SouthEast)
            {
                return SouthWest;
            }
            if (this == South)
            {
                return West;
            }
            if (this == SouthWest)
            {
                return NorthWest;
            }
            if (this == West)
            {
                return North;
            }
            if (this == NorthWest)
            {
                return NorthEast;
            }
            throw new ApplicationException("unreachable");
        }

        public Direction rotate180()
        {
            if (this == None)
            {
                return None;
            }
            if (this == North)
            {
                return South;
            }
            if (this == NorthEast)
            {
                return SouthWest;
            }
            if (this == East)
            {
                return West;
            }
            if (this == SouthEast)
            {
                return NorthWest;
            }
            if (this == South)
            {
                return North;
            }
            if (this == SouthWest)
            {
                return NorthEast;
            }
            if (this == West)
            {
                return East;
            }
            if (this == NorthWest)
            {
                return SouthEast;
            }
            throw new ApplicationException("unreachable");
        }

        public override string ToString()
        {
            if (this == None)
            {
                return "None";
            }
            if (this == North)
            {
                return "North";
            }
            if (this == NorthEast)
            {
                return "North East";
            }
            if (this == East)
            {
                return "East";
            }
            if (this == SouthEast)
            {
                return "South East";
            }
            if (this == South)
            {
                return "South";
            }
            if (this == SouthWest)
            {
                return "South West";
            }
            if (this == West)
            {
                return "West";
            }
            if (this == NorthWest)
            {
                return "North West";
            }
            throw new ApplicationException("unreachable");
        }
    }
}