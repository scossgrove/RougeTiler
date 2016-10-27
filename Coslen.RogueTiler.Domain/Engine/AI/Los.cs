using System;
using System.Collections;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.AI
{
    /// Line-of-sight object for tracing a straight line from a [start] to [end]
    /// and determining which intermediate tiles are touched.
    public class Los
    {
        public VectorBase Start { get; set; }
        public VectorBase End { get; set; }

        public List<VectorBase> Points { get; set; } = new List<VectorBase>();

        public Los(VectorBase start, VectorBase end)
        {
            End = end;
            Start = start;

            CalculatePoints();
        }

        private void CalculatePoints()
        {
            CalculatePoints(Start.x, Start.y, End.x, End.y,
                (x, y) =>
                {
                    Points.Add(new VectorBase(x, y));
                    return true;
                });
        }

        private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        /// <summary>
        /// The plot function delegate
        /// </summary>
        /// <param name="x">The x co-ord being plotted</param>
        /// <param name="y">The y co-ord being plotted</param>
        /// <returns>True to continue, false to stop the algorithm</returns>
        public delegate bool PlotFunction(int x, int y);

        private void CalculatePoints(int x0, int y0, int x1, int y1, PlotFunction plot)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
            if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
            int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (!(steep ? plot(y, x) : plot(x, y))) return;
                err = err - dY;
                if (err < 0) { y += ystep; err += dX; }
            }
        }

        public IEnumerator<VectorBase> GetEnumerator()
        {
            int position = 0; // state
            while (position < Points.Count)
            {
                yield return Points[position];
                position++;
            }
        }
    }
}