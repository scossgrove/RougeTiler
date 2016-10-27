using System;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    [Serializable]
    public class Vector : VectorBase
    {
        public Vector(int x, int y) : base(x, y)
        {
        }

        #region constants

        public static Vector zero
        {
            get { return new Vector(0, 0); }
        }

        public static Vector up
        {
            get { return new Vector(0, 1); }
        }

        public static Vector down
        {
            get { return new Vector(0, -1); }
        }

        public static Vector left
        {
            get { return new Vector(-1, 0); }
        }

        public static Vector right
        {
            get { return new Vector(1, 0); }
        }

        #endregion

        #region operator overloads

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Vector return false.
            var p = obj as Vector;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public bool Equals(Vector p)
        {
            // If parameter is null return false:
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        #endregion
    }
}