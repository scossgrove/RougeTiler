using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.AI
{
    public class PathResult
    {
        /// The direction to move on the first step of the path.
        public Direction direction;

        /// The total number of steps in the path.
        public int length;

        public PathResult(Direction direction, int length)
        {
            this.direction = direction;
            this.length = length;
        }
    }

    public class PathNode
    {
        public PathNode(PathNode parent, Direction direction, VectorBase pos, int cost, int guess)
        {
            this.parent = parent;
            this.direction = direction;
            this.pos = pos;
            this.cost = cost;
            this.guess = guess;
        }

        public PathNode parent { get; set; }
        public Direction direction { get; set; }
        public VectorBase pos { get; set; }

        /// The cost to get to this node from the starting point. This is roughly the
        /// distance, but may be a little different if we start weighting tiles in
        /// interesting ways (i.e. make it more expensive for light-abhorring
        /// monsters to walk through lit tiles).
        public int cost { get; set; }

        /// The guess as to the total cost from the start node to the end node going
        /// along this path. In other words, this is [cost] plus the heuristic.
        public int guess { get; set; }
    }

    /// A* pathfinding algorithm.
    public class AStar
    {
        /// Tries to find a path from [start] to [end], searching up to [maxLength]
        /// steps from [start]. Returns the [Direction] of the first step from [start]
        /// along that path (or [Direction.NONE] if it determines there is no path
        /// possible.
        public static Direction findDirection(Stage stage, VectorBase start, VectorBase end, int maxLength, bool canOpenDoors)
        {
            var path = _findPath(stage, start, end, maxLength, canOpenDoors);
            if (path == null)
            {
                return Direction.None;
            }

            while (path.parent != null && path.parent.parent != null)
            {
                path = path.parent;
            }

            return path.direction;
        }

        public static PathResult findPath(Stage stage, VectorBase start, VectorBase end, int maxLength, bool canOpenDoors)
        {
            //stage.Debugger();

            var path = _findPath(stage, start, end, maxLength, canOpenDoors);
            if (path == null)
            {
                return new PathResult(Direction.None, 0);
            }

            var length = 1;
            while (path.parent != null && path.parent.parent != null)
            {
                path = path.parent;
                length++;
            }

            return new PathResult(path.direction, length);
        }

        public static PathNode _findPath(Stage stage, VectorBase start, VectorBase end, int maxLength, bool canOpenDoors)
        {
            //var AStarState = stage.Appearances;
            var logicCount = 0;

            //GameBoard.Debugger.Instance.LogToDisk(string.Format("Processing _findPath"));

            // TODO: More optimal data structure.
            var startPath = new PathNode(null, Direction.None, start, 0, heuristic(start, end));
            var open = new List<PathNode> {startPath};
            var closed = new List<VectorBase>();

            while (open.Count > 0)
            {
                logicCount++;

                // Debugger the state to disc..
                //if (logicCount % 1 == 0)
                //{
                //    stage.Debugger(AStarState, "AStar Calculations");
                //}

                if (logicCount >= 50)
                {
                    return null;
                }

                // Pull out the best potential candidate.
                var lastIndex = open.Count - 1;
                var current = open[lastIndex];
                open.RemoveAt(lastIndex);

                //GameBoard.Debugger.Instance.LogToDisk(string.Format(" * current = [" + current.pos.x + "," + current.pos.y + "] at " + current.cost));

                if ((current.pos.x == end.x && current.pos.y == end.y) || (current.cost > Option.AStarFloorCost*maxLength))
                {
                    // Found the path.
                    return current;
                }

                closed.Add(current.pos);

                foreach (var dir in Direction.All)
                {
                    //GameBoard.Debugger.Instance.LogToDisk(string.Format("   * testing = [" + dir.x + "," + dir.y + "]"));

                    var neighbor = current.pos + dir;

                    //GameBoard.Debugger.Instance.LogToDisk(string.Format("     * neighbor = [" + neighbor.x + "," + neighbor.y + "]"));

                    // Skip impassable tiles.
                    if (!stage[neighbor].IsTraversable)
                    {
                        //GameBoard.Debugger.Instance.LogToDisk(string.Format("     * traversable - N"));
                        continue;
                    }

                    //GameBoard.Debugger.Instance.LogToDisk(string.Format("     * traversable - Y "));

                    // Given how far the current tile is, how far is each neighbor?
                    var stepCost = Option.AStarFloorCost;
                    if (stage[neighbor].Type.OpensTo != null)
                    {
                        if (canOpenDoors)
                        {
                            // One to open the door and one to enter the tile.
                            stepCost = Option.AStarFloorCost*2;
                        }
                        else
                        {
                            // Even though the monster can't open doors, we don't consider it
                            // totally impassable because there's a chance the door will be
                            // opened by someone else.
                            stepCost = Option.AStarDoorCost;
                        }
                    }
                    else if (stage.ActorAt(neighbor) != null)
                    {
                        stepCost = Option.AStarOccupiedCost;
                    }

                    var cost = current.cost + stepCost;
                    //GameBoard.Debugger.Instance.LogToDisk(string.Format("     * cost = [" + cost + "]"));

                    // See if we just found a better path to a tile we're already
                    // considering. If so, remove the old one and replace it (below) with
                    // this new better path.
                    var inOpen = false;

                    for (var i = 0; i < open.Count; i++)
                    {
                        var alreadyOpen = open[i];
                        if (alreadyOpen.pos.x == neighbor.x && alreadyOpen.pos.y == neighbor.y)
                        {
                            if (alreadyOpen.cost > cost)
                            {
                                open.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                inOpen = true;
                            }
                            break;
                        }
                    }

                    //GameBoard.Debugger.Instance.LogToDisk(string.Format("     * inOpen = [" + inOpen + "]"));

                    var inClosed = closed.Any(v => v.x == neighbor.x && v.y == neighbor.y);

                    //GameBoard.Debugger.Instance.LogToDisk(string.Format("     * inClosed = [" + inClosed + "]"));

                    // If we have a new path, add it.
                    if (!inOpen && !inClosed)
                    {
                        var guess = cost + heuristic(neighbor, end);
                        var path = new PathNode(current, dir, neighbor, cost, guess);

                        //GameBoard.Debugger.Instance.LogToDisk(string.Format("   * new Path = [" + path.pos.x + "," + path.pos.y + "] at " + path.cost));
                        //AStarState[path.pos.x + 1, path.pos.y + 1].Glyph = path.cost.ToString("00");

                        // Insert it in sorted order (such that the best node is at the *end*
                        // of the list for easy removal).
                        var inserted = false;
                        for (var i = open.Count - 1; i >= 0; i--)
                        {
                            if (open[i].guess > guess)
                            {
                                open.Insert(i + 1, path);
                                inserted = true;
                                break;
                            }
                        }

                        // If we didn't find a node to put it after, put it at the front.
                        if (!inserted)
                        {
                            open.Insert(0, path);
                        }
                    }
                }
            }

            // No path.
            return null;
        }

        /// The estimated cost from [pos] to [end].
        public static int heuristic(VectorBase pos, VectorBase end)
        {
            // A simple heuristic would just be the kingLength. The problem is that
            // diagonal moves are as "fast" as straight ones, which means many
            // zig-zagging paths are as good as one that looks "straight" to the player.
            // But they look wrong. To avoid this, we will estimate straight steps to
            // be a little cheaper than diagonal ones. This avoids paths like:
            //
            // ...*...
            // s.*.*.g
            // .*...*.
            var offset = (end - pos).abs();
            var numDiagonal = Math.Min(offset.x, offset.y);
            var numStraight = Math.Max(offset.x, offset.y) - numDiagonal;
            return (numDiagonal*Option.AStarFloorCost) + (numStraight*Option.aStarStraightCost);
        }
    }
}