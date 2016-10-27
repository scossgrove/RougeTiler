using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public class Circle
    {
        /// The position of the center of the circle.
        public VectorBase Center { get; set; }

        /// The radius of this Circle.
        public int Radius { get; set; }

        public List<VectorBase> Points { get; set; } = new List<VectorBase>();

        public Circle(VectorBase center, int radius)
        {
            Center = center;
            Radius = radius;
            CalculatePoints();
        }

        private void CalculatePoints()
        {
            int d = (5 - Radius * 4) / 4;
            int x = 0;
            int y = Radius;

            do
            {
                Points.Add(new VectorBase(Center.x + x, Center.y + y));
                Points.Add(new VectorBase(Center.x + x, Center.y - y));
                Points.Add(new VectorBase(Center.x - x, Center.y + y));
                Points.Add(new VectorBase(Center.x - x, Center.y - y));
                Points.Add(new VectorBase(Center.x + y, Center.y + x));
                Points.Add(new VectorBase(Center.x + y, Center.y - x));
                Points.Add(new VectorBase(Center.x - y, Center.y + x));
                Points.Add(new VectorBase(Center.x - y, Center.y - x));
                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            } while (x <= y);
        }
    }


    /// <summary>
    ///     Utility class for handling simple rasterized circles of a relatively small radius.
    ///     Used for lighting, ball spells, etc. Optimized to generate "nice" looking circles
    ///     at small sizes.
    /// </summary>
    public class CircleX : IEnumerable
    {
        /// The position of the center of the circle.
        public VectorBase center;

        /// The radius of this Circle.
        public int radius;

        public CircleX(VectorBase center, int radius)
        {
            this.center = center;
            this.radius = radius;
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException("The radius cannot be negative.");
            }
        }

        public CircleIterator iterator
        {
            get { return new CircleIterator(this, false); }
        }

        /// Traces the outside edge of the circle.
        public CircleIterator edge
        {
            get { return new CircleIterator(this, true); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// Gets whether [pos] is in the outermost edge of the circle.
        public bool isEdge(VectorBase pos)
        {
            var leadingEdge = true;
            if (radius > 0)
            {
                leadingEdge = (pos - center) > CircleUtilities.GetRadiusSquared(radius - 1);
            }

            return leadingEdge;
        }

        public CircleIterator GetEnumerator()
        {
            return new CircleIterator(this, true);
        }
    }

    public class CircleIterator : IEnumerator
    {
        private readonly List<VectorBase> boundsIterator;
        private readonly CircleX circle;
        private readonly bool edge;
        private int currentIndex;

        public CircleIterator(CircleX circle, bool edge)
        {
            this.circle = circle;
            this.edge = edge;

            var size = circle.radius + circle.radius + 1;
            var bounds = new Rect(-circle.radius, -circle.radius, size, size);
            boundsIterator = bounds.PointsInRect().ToList();

            Reset();
        }

        public object Current
        {
            get { return boundsIterator[currentIndex]; }
        }

        public bool MoveNext()
        {
            return FindCircleVector(false);
        }

        public void Reset()
        {
            currentIndex = 0;
            FindCircleVector(true);
        }


        private bool FindCircleVector(bool includeCurrent)
        {
            if (includeCurrent == false)
            {
                currentIndex++;
            }

            for (var index = currentIndex; index < boundsIterator.Count; index++)
            {
                var position = boundsIterator[index];

                var length = position.lengthSquared();
                if (length > CircleUtilities.GetRadiusSquared(circle.radius))
                {
                    continue;
                }
                if (edge && circle.radius > 0 && length < CircleUtilities.GetRadiusSquared(circle.radius - 1))
                {
                    continue;
                }

                if (currentIndex == index)
                {
                    continue;
                }

                currentIndex = index;

                return true;
            }

            return false;
        }
    }

    public static class CircleUtilities
    {
        public static int GetRadiusSquared(int radius)
        {
            int[] radiiSquared = {0, 2, 5, 10, 18, 26, 38};

            // If small enough, use the tuned radius to look best.
            if (radius < radiiSquared.Length)
            {
                return radiiSquared[radius];
            }

            return radius*radius;
        }
    }
}