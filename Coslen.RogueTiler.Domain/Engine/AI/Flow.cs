using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.AI
{
    /// A lazy, generic pathfinder.
    /// 
    /// It can be used to find the distance from a starting point to a goal, or
    /// find the directions to reach the nearest goals meeting some predicate.
    /// 
    /// Internally, it lazily runs a breadth-first search. It only processes outward
    /// as far as needed to answer the query. In practice, this means it often does
    /// less than 10% of the iterations of a full eager search.
    [Serializable]
    public class Flow
    {
        private const int _unknown = -2;
        private const int _unreachable = -1;
        private readonly bool _canOpenDoors;

        private readonly int[,] _distances;

        private readonly bool _ignoreActors;
        private readonly int? _maxDistance;

        /// The position of the array's top-level corner relative to the stage.
        private readonly VectorBase _offset;

        private readonly Stage _stage;

        /// The list of reachable cells that have been found so far, in order of
        /// increasing distance.
        /// 
        /// Coordinates are local to [_distances], not the [Stage].
        public List<VectorBase> _found = new List<VectorBase>();

        /// The cells whose neighbors still remain to be processed.
        public Queue<VectorBase> _open = new Queue<VectorBase>();

        /// Gets the starting position in stage coordinates.
        public VectorBase Start;

        public Flow(Stage stage, VectorBase start, bool canOpenDoors, bool ignoreActors) : this(stage, start, null, canOpenDoors, ignoreActors)
        {
        }

        public Flow(Stage stage, VectorBase start, int? maxDistance, bool? canOpenDoors, bool ignoreActors = true)
        {
            Start = start;
            _stage = stage;
            Start = start;
            _maxDistance = maxDistance;
            _canOpenDoors = canOpenDoors ?? false;
            _ignoreActors = ignoreActors;

            var width = 0;
            var height = 0;

            if (_maxDistance == null)
            {
                // Inset by one since we can assume the edges are impassable.
                //_offset = new VectorBase(1, 1);
                //width = _stage.Width - 2;
                //height = _stage.Height - 2;

                _offset = new VectorBase(0, 0);
                width = _stage.Width;
                height = _stage.Height;
            }
            else
            {
                var left = Math.Max(1, Start.x - _maxDistance.Value);
                var top = Math.Max(1, Start.y - _maxDistance.Value);
                var right = Math.Min(_stage.Width - 1, Start.x + _maxDistance.Value + 1);
                var bottom = Math.Min(_stage.Height - 1, Start.y + _maxDistance.Value + 1);
                _offset = new VectorBase(left, top);
                width = right - left;
                height = bottom - top;
            }

            _distances = new int[width, height];
            for (var widthIndex = 0; widthIndex < width; widthIndex++)
            {
                for (var heigthIndex = 0; heigthIndex < height; heigthIndex++)
                {
                    _distances[widthIndex, heigthIndex] = _unknown;
                }
            }

            // Seed it with the starting position.
            _open.Enqueue(Start - _offset);
            var first = _open.First();
            _distances[first.x, first.y] = 0;
        }

        /// Gets the bounds of the [Flow] in stage coordinates.
        public Rect Bounds
        {
            get
            {
                var size = new VectorBase(_distances.GetUpperBound(0), _distances.GetUpperBound(1));
                return new Rect(_offset, size);
            }
        }

        /// Returns the nearest position to start that meets [predicate].
        /// 
        /// If there are multiple equidistance positions, chooses one randomly. If
        /// there are none, returns the starting position.
        public VectorBase NearestWhere(Func<VectorBase, bool> predicate)
        {
            var results = FindAllNearestWhere(predicate);
            if (results.Count == 0)
            {
                return Start;
            }

            return Rng.Instance.Item(results) + _offset;
        }

        /// Gets the distance from the starting position to [pos], or `null` if there
        /// is no path to it.
        public int? GetDistance(VectorBase pos)
        {
            pos -= _offset;

            var bounds = new Rect(0, 0, _distances.GetUpperBound(0), _distances.GetUpperBound(1));
            if (!bounds.Contains(pos))
            {
                return null;
            }

            // Lazily search until we reach the tile in question or run out of paths to
            // try.
            while (_open.Count != 0 && _distances[pos.x, pos.y] == _unknown)
            {
                ProcessNext();
            }

            // Debugger the distances

            Debug(_distances, pos);

            var distance = _distances[pos.x, pos.y];
            if (distance == _unknown || distance == _unreachable)
            {
                return null;
            }

            return distance;
        }

        public void Debug<T>(T[,] matrix, VectorBase to)
        {
            //var mazeDebug = string.Empty;

            //mazeDebug += "\r\n";
            //mazeDebug += "\r\n--------------------------------------------------------------";
            //mazeDebug += "\r\n-- Distances : To = [" + to.x + "," + to.y + "]";
            //mazeDebug += "\r\n--------------------------------------------------------------";
            //mazeDebug += "\r\n";

            //mazeDebug += " ++ ";

            //int width = matrix.GetUpperBound(0);
            //int height = matrix.GetUpperBound(0);
            //for (var columnIndex = 0; columnIndex < width; columnIndex++)
            //{
            //    mazeDebug += string.Format(" {0,2} ", columnIndex);
            //}
            //mazeDebug += "\r\n";

            //for (var rowIndex = 0; rowIndex < height; rowIndex++)
            //{
            //    mazeDebug += string.Format(" {0,2} ", rowIndex);

            //    for (var columnIndex = 0; columnIndex < width; columnIndex++)
            //    {
            //        var tile = matrix[columnIndex, rowIndex];
            //        mazeDebug += string.Format(" {0,2} ", tile);
            //    }

            //    mazeDebug += "\r\n";
            //}

            //logger.Info(mazeDebug);
        }

        /// Chooses a random direction from [start] that gets closer to [pos].
        private Direction DirectionTo(VectorBase pos)
        {
            var goals = new List<VectorBase>();
            goals.Add(pos - _offset);
            var directions = DirectionsTo(goals);
            if (directions.Count == 0)
            {
                return Direction.None;
            }
            return Rng.Instance.Item(directions);
        }

        /// Chooses a random direction from [start] that gets closer to one of the
        /// nearest positions matching [predicate].
        /// 
        /// Returns [Direction.NONE] if no matching positions were found.
        public Direction DirectionToNearestWhere(Func<VectorBase, bool> predicate)
        {
            var directions = DirectionsToNearestWhere(predicate);
            if (directions.Count == 0)
            {
                return Direction.None;
            }
            return Rng.Instance.Item(directions);
        }

        /// Find all directions from [start] that get closer to one of the nearest
        /// positions matching [predicate].
        /// 
        /// Returns an empty list if no matching positions were found.
        private List<Direction> DirectionsToNearestWhere(Func<VectorBase, bool> predicate)
        {
            var goals = FindAllNearestWhere(predicate);
            if (goals == null)
            {
                return new List<Direction>();
            }

            return DirectionsTo(goals);
        }

        /// Get the positions closest to [start] that meet [predicate].
        /// 
        /// Only returns more than one position if there are multiple equidistance
        /// positions meeting the criteria. Returns an empty list if no valid
        /// positions are found. Returned positions are local to [_distances], not
        /// the [Stage].
        private List<VectorBase> FindAllNearestWhere(Func<VectorBase, bool> predicate)
        {
            var goals = new List<VectorBase>();

            int? nearestDistance = null;
            for (var i = 0;; i++)
            {
                // Lazily find the next open tile.
                while (_open.Count != 0 && i >= _found.Count)
                {
                    ProcessNext();
                }

                // If we flowed everywhere and didn't find anything, give up.
                if (_open.Count == 0 && i >= _found.Count)
                {
                    return goals;
                }

                var pos = _found[i];
                if (!predicate(pos + _offset))
                {
                    continue;
                }

                var distance = _distances[pos.x, pos.y];

                // Since pos was from _found, it should be reachable.
                if (distance < 0)
                {
                    throw new ApplicationException();
                }

                if (nearestDistance.HasValue == false || distance == nearestDistance)
                {
                    // Consider all goals at the nearest distance.
                    nearestDistance = distance;
                    goals.Add(pos);
                }
                else
                {
                    // We hit a tile that's farther than a valid goal, so we can stop
                    // looking.
                    break;
                }
            }

            return goals;
        }


        // Starting at [pos], recursively walk along all paths that proceed towards
        // [start].
        private void WalkBack(VectorBase pos, List<VectorBase> walked, List<Direction> directions)
        {
            if (walked.Contains(pos))
            {
                return;
            }
            walked.Add(pos);

            foreach (var dir in Direction.All)
            {
                var here = pos + dir;

                var bounds = new Rect(0, 0, _distances.GetUpperBound(0), _distances.GetUpperBound(1));
                if (!bounds.Contains(here))
                {
                    continue;
                }

                if (here == Start - _offset)
                {
                    // If this step reached the target, mark the direction of the step.
                    directions.Add(dir.rotate180());
                }
                else if (_distances[here.x, here.y] >= 0 && _distances[here.x, here.y] < _distances[pos.x, pos.y])
                {
                    WalkBack(here, walked, directions);
                }
            }
        }

        /// Find all directions from [start] that get closer to one of positions in
        /// [goals].
        /// 
        /// Returns an empty list if none of the goals can be reached.
        private List<Direction> DirectionsTo(List<VectorBase> goals)
        {
            var walked = new List<VectorBase>();
            var directions = new List<Direction>();

            // Trace all paths from the goals back to the target.
            foreach (var goal in goals)
            {
                WalkBack(goal, walked, directions);
            }

            return directions;
        }

        /// Runs one iteration of the search.
        private void ProcessNext()
        {
            // Should only call this while there's still work to do.
            if (_open.Count == 0)
            {
                throw new ApplicationException();
            }

            var start = _open.Dequeue();
            var distance = _distances[start.x, start.y];

            // Update the neighbor's distances.
            foreach (var dir in Direction.All)
            {
                var here = start + dir;

                var bounds = new Rect(0, 0, _distances.GetUpperBound(0), _distances.GetUpperBound(1));
                if (!bounds.Contains(here))
                {
                    continue;
                }

                // Ignore tiles we've already reached.
                if (_distances[here.x, here.y] != _unknown)
                {
                    continue;
                }

                // Can't reach impassable tiles.
                var tile = _stage[here + _offset];
                var canEnter = tile.IsPassable || (tile.IsTraversable && _canOpenDoors);

                // Can't walk through other actors.
                if (!_ignoreActors && _stage.ActorAt(here + _offset) != null)
                {
                    canEnter = false;
                }

                if (!canEnter)
                {
                    _distances[here.x, here.y] = _unreachable;
                    continue;
                }

                _distances[here.x, here.y] = distance + 1;
                _open.Enqueue(here);
                _found.Add(here);
            }
        }

        /*
        void _dump() {
          var buffer = new StringBuffer();
          for (var y = 0; y < _distances.height; y++) {
            for (var x = 0; x < _distances.width; x++) {
              var distance = _distances.get(x, y);
              if (distance == _unknown) {
                buffer.write("?");
              } else if (distance == _unreachable) {
                buffer.write("#");
              } else {
                buffer.write(distance % 10);
              }
            }
            buffer.writeln();
          }

          print(buffer.toString());
        }
        */

        /// Prints the distances array for debugging.
    }
}