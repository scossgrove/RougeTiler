using System;
using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Common

{
    /// Shared base class of [Vec] and [Direction]. We do this instead of having
    /// [Direction] inherit directly from [Vec] so that we can avoid it inheriting
    /// an `==` operator, which would prevent it from being used in `switch`
    /// statements. Instead, [Direction] uses identity equality.
    [Serializable]
    public class VectorBase : ICloneable
    {
        public VectorBase(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x { get; set; }
        public int y { get; set; }

        /// Gets the area of a [Rect] whose corners are (0, 0) and this Vec.
        /// 
        /// Returns a negative area if one of the Vec's coordinates are negative.
        public int area()
        {
            return x*y;
        }

        /// Gets the rook length of the Vec, which is the number of squares a rook on
        /// a chessboard would need to move from (0, 0) to reach the endpoint of the
        /// Vec. Also known as Manhattan or taxicab distance.
        public int rookLength()
        {
            return Math.Abs(x) + Math.Abs(y);
        }

        /// Gets the king length of the Vec, which is the number of squares a king on
        /// a chessboard would need to move from (0, 0) to reach the endpoint of the
        /// Vec. Also known as Chebyshev distance.
        public int kingLength()
        {
            return Math.Max(Math.Abs(x), Math.Abs(y));
        }

        public int lengthSquared()
        {
            return x*x + y*y;
        }

        /// The Cartesian length of the vector.
        /// 
        /// If you just need to compare the magnitude of two vectors, prefer using
        /// the comparison operators or [lengthSquared], both of which are faster
        /// than this.
        public double length()
        {
            return Math.Sqrt(lengthSquared());
        }

        /// Scales this VectorBase by [other].
        public static VectorBase operator *(VectorBase obj, int other)
        {
            return new VectorBase(obj.x*other, obj.y*other);
        }

        /// Scales this VectorBase by [other].
        public VectorBase Scale(VectorBase obj, int other)
        {
            return new VectorBase(obj.x*other, obj.y*other);
        }

        /// Adds [other] to this Vec.
        /// 
        /// *  If [other] is a [Vec] or [Direction], adds each pair of coordinates.
        /// *  If [other] is an [int], adds that value to both coordinates.
        /// 
        /// Any other type is an error.
        public static VectorBase operator +(VectorBase obj, object other)
        {
            if (other is VectorBase)
            {
                var otherVectorBase = (VectorBase) other;
                return new VectorBase(obj.x + otherVectorBase.x, obj.y + otherVectorBase.y);
            }
            if (other is int)
            {
                var otherScaler = (int) other;
                return new VectorBase(obj.x + otherScaler, obj.y + otherScaler);
            }

            throw new ApplicationException("Operand must be an int or VecBase.");
        }

        /// Substracts [other] from this Vec.
        /// 
        /// *  If [other] is a [Vec] or [Direction], subtracts each pair of
        /// coordinates.
        /// *  If [other] is an [int], subtracts that value from both coordinates.
        /// 
        /// Any other type is an error.
        public static VectorBase operator -(VectorBase obj, object other)
        {
            if (other is VectorBase)
            {
                var otherVectorBase = (VectorBase) other;
                return new VectorBase(obj.x - otherVectorBase.x, obj.y - otherVectorBase.y);
            }
            if (other is int)
            {
                var otherScaler = (int) other;
                return new VectorBase(obj.x - otherScaler, obj.y - otherScaler);
            }

            throw new ApplicationException("Operand must be an int or VecBase.");
        }

        public static bool operator >(VectorBase obj, object other)
        {
            if (other is VectorBase)
            {
                var otherVectorBase = (VectorBase) other;
                return obj.lengthSquared() > otherVectorBase.lengthSquared();
            }
            if (other is int)
            {
                var otherScaler = (int) other;
                return obj.lengthSquared() > otherScaler*otherScaler;
            }

            if (other is double)
            {
                var otherScaler = (double)other;
                return obj.lengthSquared() > otherScaler * otherScaler;
            }

            throw new ApplicationException("Operand must be an int or VecBase.");
        }

        /// Returns `true` if the magnitude of this vector is greater than or equal
        /// to [other].
        public static bool operator >=(VectorBase obj, object other)
        {
            if (other is VectorBase)
            {
                var otherVectorBase = (VectorBase) other;
                return obj.lengthSquared() >= otherVectorBase.lengthSquared();
            }
            if (other is int)
            {
                var otherScaler = (int) other;
                return obj.lengthSquared() >= otherScaler*otherScaler;
            }

            throw new ApplicationException("Operand must be an int or VecBase.");
        }

        /// Returns `true` if the magnitude of this vector is less than [other].
        public static bool operator <(VectorBase obj, object other)
        {
            if (other is VectorBase)
            {
                var otherVectorBase = (VectorBase) other;
                return obj.lengthSquared() < otherVectorBase.lengthSquared();
            }
            if (other is int)
            {
                var otherScaler = (int) other;
                return obj.lengthSquared() < otherScaler*otherScaler;
            }
            if (other is double)
            {
                var otherScaler = (double)other;
                return obj.lengthSquared() < otherScaler * otherScaler;
            }

            throw new ApplicationException("Operand must be an int or VecBase.");
        }

        /// Returns `true` if the magnitude of this vector is less than or equal to
        /// [other].
        public static bool operator <=(VectorBase obj, object other)
        {
            if (other is VectorBase)
            {
                var otherVectorBase = (VectorBase) other;
                return obj.lengthSquared() <= otherVectorBase.lengthSquared();
            }
            if (other is int)
            {
                var otherScaler = (int) other;
                return obj.lengthSquared() <= otherScaler*otherScaler;
            }

            throw new ApplicationException("Operand must be an int or VecBase.");
        }

        /// Gets whether the given vector is within a rectangle from (0,0) to this
        /// vector (half-inclusive).
        public bool contains(VectorBase pos)
        {
            var left = Math.Min(0, x);
            if (pos.x < left)
            {
                return false;
            }

            var right = Math.Max(0, x);
            if (pos.x >= right)
            {
                return false;
            }

            var top = Math.Min(0, y);
            if (pos.y < top)
            {
                return false;
            }

            var bottom = Math.Max(0, y);
            if (pos.y >= bottom)
            {
                return false;
            }

            return true;
        }

        /// Returns a new [Vec] with the absolute value of the coordinates of this
        /// one.
        public VectorBase abs()
        {
            return new VectorBase(Math.Abs(x), Math.Abs(y));
        }

        /// Returns a new [Vec] whose coordinates are this one's translated by [x] and
        /// [y].
        public VectorBase offset(int x, int y)
        {
            return new VectorBase(this.x + x, this.y + y);
        }

        /// Returns a new [Vec] whose coordinates are this one's but with the X
        /// coordinate translated by [x].
        public VectorBase offsetX(int x)
        {
            return new VectorBase(this.x + x, y);
        }

        /// Returns a new [Vec] whose coordinates are this one's but with the Y
        /// coordinate translated by [y].
        public VectorBase offsetY(int y)
        {
            return new VectorBase(x, this.y + y);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", x, y);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        protected bool Equals(VectorBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VectorBase) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x*397) ^ y;
            }
        }

        public static bool operator ==(VectorBase lhs, VectorBase rhs)
        {
            if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs))
            {
                return true;
            }

            if (ReferenceEquals(null, lhs))
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(VectorBase lhs, VectorBase rhs)
        {
            return !(lhs == rhs);
        }
    }
}