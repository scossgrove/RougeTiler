using System;
using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    /// The Random Number God: deliverer of good and ill fortune alike.
    public class Rng
    {
        private static Rng instance;

        private readonly Random _random;

        private Rng(int seed)
        {
            _random = new Random(seed);
        }

        public static Rng Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Rng(Guid.NewGuid().GetHashCode());
                }
                return instance;
            }
        }

        /// Gets a random int within a given range. If [max] is given, then it is
        /// in the range `[minOrMax, max)`. Otherwise, it is `[0, minOrMax)`. In
        /// other words, `range(3)` returns a `0`, `1`, or `2`, and `range(2, 5)`
        /// returns `2`, `3`, or `4`.
        public int Range(int minOrMax, int? max = null)
        {
            if (max.HasValue == false)
            {
                max = minOrMax;
                minOrMax = 0;
            }

            return _random.Next(minOrMax, max.Value);
        }


        /// Gets a random int within a given range. If [max] is given, then it is
        /// in the range `[minOrMax, max]`. Otherwise, it is `[0, minOrMax]`. In
        /// other words, `inclusive(2)` returns a `0`, `1`, or `2`, and
        /// `inclusive(2, 4)` returns `2`, `3`, or `4`.
        public int Inclusive(int minOrMax, int? max = null)
        {
            if (max.HasValue == false)
            {
                max = minOrMax;
                minOrMax = 0;
            }

            max++;

            return _random.Next(minOrMax, max.Value);
        }


        /// Returns `true` if a random int chosen between 1 and chance was 1.
        public bool OneIn(int chance)
        {
            var testInt = Range(chance);
            return testInt == 0;
        }

        /// Gets a random item from the given list.
        public T Item<T>(List<T> items)
        {
            return items[Range(items.Count)];
        }

        public T Item<T>(T[] items)
        {
            return items[Range(items.Length)];
        }

        ///// Removes a random item from the given list.
        //take(List items)
        //      {
        //          final index = range(items.length);
        //          final item = items[index];
        //          items.removeRange(index, 1);
        //          return item;
        //      }

        /// Gets a random [Vec] within the given [Rect] (half-inclusive).
        public Vector vectorInRect(Rect rect)
        {
            return new Vector(Range(rect.left, rect.right), Range(rect.top, rect.bottom));
        }


        /// Gets a random number centered around [center] with [range] (inclusive)
        /// using a triangular distribution. For example `triangleInt(8, 4)` will
        /// return values between 4 and 12 (inclusive) with greater distribution
        /// towards 8.
        /// 
        /// This means output values will range from `(center - range)` to
        /// `(center + range)` inclusive, with most values near the center, but not
        /// with a normal distribution. Think of it as a poor man's bell curve.
        /// 
        /// The algorithm works essentially by choosing a random point inside the
        /// triangle, and then calculating the x coordinate of that point. It works
        /// like this:
        /// 
        /// Consider Center 4, Range 3:
        /// 
        /// *
        /// * | *
        /// * | | | *
        /// * | | | | | *
        /// --+-----+-----+--
        /// 0 1 2 3 4 5 6 7 8
        /// -r     c     r
        /// 
        /// Now flip the left half of the triangle (from 1 to 3) vertically and move
        /// it over to the right so that we have a square.
        /// 
        /// .-------.
        /// |       V
        /// |
        /// |   R L L L
        /// | . R R L L
        /// . . R R R L
        /// . . . R R R R
        /// --+-----+-----+--
        /// 0 1 2 3 4 5 6 7 8
        /// 
        /// Choose a point in that square. Figure out which half of the triangle the
        /// point is in, and then remap the point back out to the original triangle.
        /// The result is the *x* coordinate of the point in the original triangle.
        public int triangleInt(int center, int range)
        {
            if (range < 0)
            {
                throw new ArgumentException("The argument \"range\" must be zero or greater.");
            }

            // Pick a point in the square.
            var x = Inclusive(range);
            var y = Inclusive(range);

            // Figure out which triangle we are in.
            if (x <= y)
            {
                // Larger triangle.
                return center + x;
            }
            // Smaller triangle.
            return center - range - 1 + x;
        }

        public int taper(int start, int chanceOfIncrement)
        {
            while (OneIn(chanceOfIncrement))
            {
                start++;
            }
            return start;
        }
    }
}