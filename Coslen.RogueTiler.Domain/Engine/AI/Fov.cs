using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.AI
{
    /// Represents the 1D projection of a 2D shadow onto a normalized line. In
    /// other words, a range from 0.0 to 1.0.
    public class FovShadow
    {
        public FovShadow(decimal start, decimal end)
        {
            Start = start;
            End = end;
        }

        public decimal Start { get; set; }
        public decimal End { get; set; }

        public override string ToString()
        {
            return $"({Start}-{End})";
        }

        public bool Contains(FovShadow projection)
        {
            return (Start <= projection.Start) && (End >= projection.End);
        }
    }

    /// Calculates the [Hero]'s field of view of the dungeon.
    public class Fov
    {
        private readonly Game game;

        private List<FovShadow> shadows;

        public Fov(Game game)
        {
            this.game = game;
        }

        public Stage Stage => game.CurrentStage;


        /// Updates the visible flags in [stage] given the [Hero]'s [pos].
        public void Refresh(VectorBase pos)
        {
            var numExplored = 0;

            // Sweep through the octants.
            for (var octant = 0; octant < 8; octant++)
            {
                numExplored += RefreshOctant(pos, octant);
            }

            // The starting position is always visible.
            if (Stage[pos.x, pos.y].SetVisible(true))
            {
                numExplored++;
            }

            game.Hero.Explore(numExplored);
        }

        private int RefreshOctant(VectorBase start, int octant)
        {
            var numExplored = 0;
            VectorBase rowInc = null;
            VectorBase colInc = null;

            // Figure out which direction to increment based on the octant. Octant 0
            // starts at 12 - 2 o'clock, and octants proceed clockwise from there.
            switch (octant)
            {
                case 0:
                    rowInc = new VectorBase(0, -1);
                    colInc = new VectorBase(1, 0);
                    break;
                case 1:
                    rowInc = new VectorBase(1, 0);
                    colInc = new VectorBase(0, -1);
                    break;
                case 2:
                    rowInc = new VectorBase(1, 0);
                    colInc = new VectorBase(0, 1);
                    break;
                case 3:
                    rowInc = new VectorBase(0, 1);
                    colInc = new VectorBase(1, 0);
                    break;
                case 4:
                    rowInc = new VectorBase(0, 1);
                    colInc = new VectorBase(-1, 0);
                    break;
                case 5:
                    rowInc = new VectorBase(-1, 0);
                    colInc = new VectorBase(0, 1);
                    break;
                case 6:
                    rowInc = new VectorBase(-1, 0);
                    colInc = new VectorBase(0, -1);
                    break;
                case 7:
                    rowInc = new VectorBase(0, -1);
                    colInc = new VectorBase(-1, 0);
                    break;
            }

            if (rowInc == null || colInc == null)
            {
                throw new ArgumentNullException();
            }

            shadows = new List<FovShadow>();

            var bounds = Stage.Bounds();
            var fullShadow = false;

            // Sweep through the rows ('rows' may be vertical or horizontal based on
            // the incrementors). Start at row 1 to skip the center position.
            for (var row = 1;; row++)
            {
                var pos = start + (rowInc*row);

                // If we've traversed out of bounds, bail.
                // Note: this improves performance, but works on the assumption that the
                // starting tile of the FOV is in bounds.
                if (!bounds.Contains(pos))
                {
                    break;
                }

                for (var col = 0; col <= row; col++)
                {
                    var blocksLight = false;
                    var visible = false;
                    FovShadow projection = null;

                    // If we know the entire row is in shadow, we don't need to be more
                    // specific.
                    if (!fullShadow)
                    {
                        blocksLight = !Stage[pos].IsTransparent;
                        projection = GetProjection(col, row);
                        visible = !IsInShadow(projection);
                    }

                    // Set the visibility of this tile.
                    if (Stage[pos].SetVisible(visible))
                    {
                        numExplored++;
                    }

                    // Add any opaque tiles to the shadow map.
                    if (blocksLight)
                    {
                        fullShadow = AddShadow(projection);
                    }

                    // Move to the next column.
                    pos += colInc;

                    // If we've traversed out of bounds, bail on this row.
                    // note: this improves performance, but works on the assumption that
                    // the starting tile of the FOV is in bounds.
                    if (!bounds.Contains(pos))
                    {
                        break;
                    }
                }
            }

            return numExplored;
        }

        /// Creates a [Shadow] that corresponds to the projected silhouette of the
        /// given tile. This is used both to determine visibility (if any of the
        /// projection is visible, the tile is) and to add the tile to the shadow map.
        /// 
        /// The maximal projection of a square is always from the two opposing
        /// corners. From the perspective of octant zero, we know the square is
        /// above and to the right of the viewpoint, so it will be the top left and
        /// bottom right corners.
        private static FovShadow GetProjection(int col, int row)
        {
            // The top edge of row 0 is 2 wide.
            var topLeft = col/(row + 2);

            // The bottom edge of row 0 is 1 wide.
            var bottomRight = (col + 1)/(row + 1);

            return new FovShadow(topLeft, bottomRight);
        }

        private bool IsInShadow(FovShadow projection)
        {
            // Check the shadow list.
            return shadows.Any(shadow => shadow.Contains(projection));
        }

        private bool AddShadow(FovShadow shadow)
        {
            int index;
            for (index = 0; index < shadows.Count; index++)
            {
                // See if we are at the insertion point for this shadow.
                if (shadows[index].Start > shadow.Start)
                {
                    // Break out and handle inserting below.
                    break;
                }
            }

            // The new shadow is going here. See if it overlaps the previous or next.
            var overlapsPrev = ((index > 0) && (shadows[index - 1].End > shadow.Start));
            var overlapsNext = ((index < shadows.Count) && (shadows[index].Start < shadow.End));

            // Insert and unify with overlapping shadows.
            if (overlapsNext)
            {
                if (overlapsPrev)
                {
                    // Overlaps both, so unify one and delete the other.
                    shadows[index - 1].End = Math.Max(shadows[index - 1].End, shadows[index].End);
                    shadows.RemoveAt(index);
                }
                else
                {
                    // Just overlaps the next shadow, so unify it with that.
                    shadows[index].Start = Math.Min(shadows[index].Start, shadow.Start);
                }
            }
            else
            {
                if (overlapsPrev)
                {
                    // Just overlaps the previous shadow, so unify it with that.
                    shadows[index - 1].End = Math.Max(shadows[index - 1].End, shadow.End);
                }
                else
                {
                    // Does not overlap anything, so insert.
                    shadows.Insert(index, shadow);
                }
            }

            // See if we are now shadowing everything.
            return (shadows.Count == 1) && (shadows[0].Start == 0) && (shadows[0].End == 1);
        }
    }
}