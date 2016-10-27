using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.AI;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public class Rect
    {
        public Vector pos;
        public Vector size;

        public Rect(int left, int top, int width, int height)
        {
            pos = new Vector(left, top);
            size = new Vector(width, height);
        }

        public Rect(VectorBase position, VectorBase size)
        {
            pos = (Vector) position;
            this.size = (Vector) size;
        }

        public int x
        {
            get { return pos.x; }
        }

        public int y
        {
            get { return pos.y; }
        }

        public int width
        {
            get { return size.x; }
            set { size.x = value; }
        }

        public int height
        {
            get { return size.y; }
            set { size.y = value; }
        }

        // Use min and max to handle negative sizes.
        public int left
        {
            get { return Math.Min(x, x + width); }
        }

        public int top
        {
            get { return Math.Min(y, y + height); }
        }

        public int right
        {
            get { return Math.Max(x, x + width); }
        }

        public int bottom
        {
            get { return Math.Max(y, y + height); }
        }

        public Vector topLeft
        {
            get { return new Vector(left, top); }
        }

        public Vector topRight
        {
            get { return new Vector(right, top); }
        }

        public Vector bottomLeft
        {
            get { return new Vector(left, bottom); }
        }

        public Vector bottomRight
        {
            get { return new Vector(right, bottom); }
        }

        public static Rect LeftTopRightBottom(int left, int top, int right, int bottom)
        {
            return new Rect(left, top, right - left, bottom - top);
        }


        public void Inflate(int size)
        {
            height += size;
            width += size;
        }

        public void Inflate(int height, int width)
        {
            this.height += height;
            this.width += width;
        }

        /// Returns the distance between this Rect and [other]. This is minimum
        /// length that a corridor would have to be to go from one Rect to the other.
        /// If the two Rects are adjacent, returns zero. If they overlap, returns -1.
        public int DistanceTo(Rect other)
        {
            int vertical;
            if (top >= other.bottom)
            {
                vertical = top - other.bottom;
            }
            else if (bottom <= other.top)
            {
                vertical = other.top - bottom;
            }
            else
            {
                vertical = -1;
            }

            int horizontal;
            if (left >= other.right)
            {
                horizontal = left - other.right;
            }
            else if (right <= other.left)
            {
                horizontal = other.left - right;
            }
            else
            {
                horizontal = -1;
            }

            if ((vertical == -1) && (horizontal == -1))
            {
                return -1;
            }
            if (vertical == -1)
            {
                return horizontal;
            }
            if (horizontal == -1)
            {
                return vertical;
            }
            return horizontal + vertical;
        }

        public bool Contains(object obj)
        {
            if (!(obj is VectorBase))
            {
                return false;
            }

            var point = obj as VectorBase;
            if (point.x < pos.x)
            {
                return false;
            }
            if (point.x >= pos.x + size.x)
            {
                return false;
            }
            if (point.y < pos.y)
            {
                return false;
            }
            if (point.y >= pos.y + size.y)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0},{1} : {2},{3}", topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }
        
        public IList<VectorBase> PointsInRect()
        {
            var enumerableList = new List<VectorBase>();
            for (var xIndex = topLeft.x; xIndex <= bottomRight.x; xIndex++)
            {
                for (var yIndex = topLeft.y; yIndex <= bottomRight.y; yIndex++)
                {
                    enumerableList.Add(new VectorBase(xIndex, yIndex));
                }
            }
            return enumerableList;
        }

        public IList<VectorBase> PointsInsideRect()
        {
            var enumerableList = new List<VectorBase>();
            for (var xIndex = topLeft.x; xIndex <= bottomRight.x; xIndex++)
            {
                for (var yIndex = topLeft.y; yIndex <= bottomRight.y; yIndex++)
                {
                    if (xIndex == topLeft.x || xIndex == bottomRight.x)
                    {
                        continue;
                    }

                    if (yIndex == topLeft.y || yIndex == bottomRight.y)
                    {
                        continue;
                    }

                    enumerableList.Add(new VectorBase(xIndex, yIndex));
                }
            }
            return enumerableList;
        }

        public IList<VectorBase> PointsTracingRect()
        {
            var enumerableList = new List<VectorBase>();
            for (var xIndex = topLeft.x; xIndex <= bottomRight.x; xIndex++)
            {
                for (var yIndex = topLeft.y; yIndex <= bottomRight.y; yIndex++)
                {
                    if (xIndex == topLeft.x || xIndex == bottomRight.x)
                    {
                        enumerableList.Add(new VectorBase(xIndex, yIndex));
                    }

                    if (yIndex == topLeft.y || yIndex == bottomRight.y)
                    {
                        enumerableList.Add(new VectorBase(xIndex, yIndex));
                    }
                }
            }
            return enumerableList;
        }

        /// Creates a new rectangle that is the intersection of [a] and [b].
        /// 
        /// .----------.
        /// | a        |
        /// | .--------+----.
        /// | | result |  b |
        /// | |        |    |
        /// '-+--------'    |
        /// |             |
        /// '-------------'
        public static Rect intersect(Rect a, Rect b)
        {
            var left = Math.Max(a.left, b.left);
            var right = Math.Min(a.right, b.right);
            var top = Math.Max(a.top, b.top);
            var bottom = Math.Min(a.bottom, b.bottom);

            var width = Math.Max(0, right - left);
            var height = Math.Max(0, bottom - top);

            return new Rect(left, top, width, height);
        }

        public VectorBase GetCenter()
        {
            var centerX = (bottomRight.x - bottomLeft.x) / 2 + bottomLeft.x;

            var centerY = (bottomRight.y - topRight.y) / 2 + topRight.y;

            return new VectorBase(centerX, centerY);
        }

        public VectorBase GetIntersect(VectorBase origin)
        {
            var center = GetCenter();

            VectorBase pointOfIntersection = null;
            pointOfIntersection = LineIntersectsLine(origin, center, new VectorBase(topLeft.x, topLeft.y), new VectorBase(topLeft.x + width, topLeft.y));
            if (pointOfIntersection != null)
            {
                return pointOfIntersection;
            }
            pointOfIntersection = LineIntersectsLine(origin, center, new VectorBase(topLeft.x + width, topLeft.y), new VectorBase(topLeft.x + width, topLeft.y + height));
            if (pointOfIntersection != null)
            {
                return pointOfIntersection;
            }
            pointOfIntersection = LineIntersectsLine(origin, center, new VectorBase(topLeft.x + width, topLeft.y + height), new VectorBase(topLeft.x, topLeft.y + height));
            if (pointOfIntersection != null)
            {
                return pointOfIntersection;
            }
            pointOfIntersection = LineIntersectsLine(origin, center, new VectorBase(topLeft.x, topLeft.y + height), new VectorBase(topLeft.x, topLeft.y));
            return pointOfIntersection;
        }

        public bool Intersect(VectorBase origin)
        {
            return GetIntersect(origin) != null;
        }

        private VectorBase LineIntersectsLine(VectorBase l1p1, VectorBase l1p2, VectorBase l2p1, VectorBase l2p2)
        {
            var intersectingLine = new Los(l1p1, l1p2);

            var queryingLine = new Los(l2p1, l2p2);

            foreach (var point in intersectingLine.Points)
            {
                if (queryingLine.Points.Contains(point))
                {
                    return point;
                }
            }

            return null;
        }
    }
}