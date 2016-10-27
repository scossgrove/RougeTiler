using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    /// The random dungeon generator.
    /// 
    /// Starting with a stage of solid walls, it works like so:
    /// 
    /// 1. Place a number of randomly sized and positioned rooms. If a room
    /// overlaps an existing room, it is discarded. Any remaining rooms are
    /// carved out.
    /// 2. Any remaining solid areas are filled in with mazes. The maze generator
    /// will grow and fill in even odd-shaped areas, but will not touch any
    /// rooms.
    /// 3. The result of the previous two steps is a series of unconnected rooms
    /// and mazes. We walk the stage and find every tile that can be a
    /// "connector". This is a solid tile that is adjacent to two unconnected
    /// regions.
    /// 4. We randomly choose connectors and open them or place a door there until
    /// all of the unconnected regions have been joined. There is also a slight
    /// chance to carve a connector between two already-joined regions, so that
    /// the dungeon isn't single connected.
    /// 5. The mazes will have a lot of dead ends. Finally, we remove those by
    /// repeatedly filling in any open tile that's closed on three sides. When
    /// this is done, every corridor in a maze actually leads somewhere.
    /// 
    /// The end result of this is a multiply-connected dungeon with rooms and lots
    /// of winding corridors.
    public class Dungeon : DungeonBuilder
    {
        private int _currentRegion = -1; // The index of the current region (=connected carved area) being carved, -1 = default, wall

        public int[,] _regions { get; set; }
        public int extraConnectorChance = 0; //The inverse chance of adding a connector between two regions that have already been joined for more interconnection between regions

        //Hauberk Dungeon variables
        public int numRoomTries = 250; //Room placement tries
        public int roomExtraSize = 0; //Increasing this allows rooms to be larger.
        public bool streamLine = true; //streamline corridors between branchpoints and doors
        public bool tryRoomsFirst = false; //try to make room-to-room connections before making corridor-to-room connections (corridor-to-corridor are impossible)
        public int windingPercent = 20; //chance a maze will make a turn which will make it more winding
        
        /// <summary>
        ///     Generate a room and maze dungeon, http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
        /// </summary>
        public override void Generate(Stage stage)
        {
            numRoomTries = (stage.Width * stage.Height) / 36;
            base.Generate(stage);
        }
        
        #region "Maze Generation Functions"

        public override void BeforeGeneration()
        {
            //init grids
            _regions = new int[stage.Width, stage.Height];
            _currentRegion = -1; //reset

            for (var x = 0; x < stage.Width; x++)
            {
                for (var y = 0; y < stage.Height; y++)
                {
                    _regions[x, y] = _currentRegion; //-1
                }
            }
        }

        public override void AddMaze()
        {
            // Want to make the maze ?
            if (true)
            {
                // Fill in all of the empty space with mazes.
                for (var x = 1; x < stage.Width; x += 2)
                {
                    for (var y = 1; y < stage.Height; y += 2)
                    {
                        var pos = new VectorBase(x, y);
                        if (GetTile(pos).Type != TileTypeFactory.Instance.Wall)
                        {
                            continue; //ignore already carved spaces
                        }
                        _growMaze(pos);
                    }
                }

                _connectRegions();
                _removeDeadEnds();

                foreach (var room in Rooms)
                {
                    OnDecorateRoom(room);
                }
            }

            //function Marrt added to streamline corridors
            if (streamLine)
            {
                _streamLineCorridors();
            }
        }

        /// Places rooms ignoring the existing maze corridors.
        public override void AddRooms()
        {
            // Say we want rooms to be 50% of the total area of the map
            var totalNumberOfTiles = stage.Width * stage.Height;
            var totalNumberOfTilesForRooms = totalNumberOfTiles / 2;
            var averageNumberOfTilesPerRoom = totalNumberOfTilesForRooms * 2 / numRoomTries; // 25% success rate?
            //var size = (int)Math.Sqrt(averageNumberOfTilesPerRoom);

            Rooms = new List<Rect>();
            
            for (var i = 0; i < numRoomTries; i++)
            //for (var i = 0; i < numberOfRoomsToAdd; i++)
            {
                // Pick a random room size. The funny math here does two things:
                // - It makes sure rooms are odd-sized to line up with maze.
                // - It avoids creating rooms that are too rectangular: too tall and
                //   narrow or too wide and flat.
                // TODO: This isn't very flexible or tunable. Do something better here.

                var size = Rng.Instance.Range(1, 3 + roomExtraSize) * 2 + 1;
                var rectangularity = Rng.Instance.Range(0, 1 + size / 2) * 2;
                var width = size;
                var height = size;

                if (Rng.Instance.Range(0, 2) == 0)
                {
                    //50% chance
                    width += rectangularity;
                }
                else
                {
                    height += rectangularity;
                }

                var x = Rng.Instance.Range(1, (int)Math.Floor((stage.Width - width) * 0.5)) * 2 + 1;
                var y = Rng.Instance.Range(1, (int)Math.Floor((stage.Height - height) * 0.5)) * 2 + 1;

                var room = new Rect(x, y, width, height);

                var overlaps = false;
                foreach (var other in Rooms)
                {
                    if (room.DistanceTo(other) <= 0)
                    {
                        overlaps = true;
                        break; //break this foreach
                    }
                }

                if (overlaps)
                {
                    continue; //don't add room and retry
                }

                if (x + width > stage.Width || y + height > stage.Width)
                {
                    return;
                }

                //add non-overlapping room
                Rooms.Add(room);

                StartRegion();

                for (var ix = x; ix < x + width; ix++)
                {
                    for (var iy = y; iy < y + height; iy++)
                    {
                        _carve(new VectorBase(ix, iy), TileTypeFactory.Instance.Floor);
                    }
                }
            }
        }
        
        /// Implementation of the "growing tree" algorithm from here:
        /// http://www.astrolog.org/labyrnth/algrithm.htm.
        private void _growMaze(VectorBase start)
        {
            var cells = new List<VectorBase>();
            VectorBase lastDir = null;


            StartRegion();
            _carve(start);

            cells.Add(start);

            while (cells.Count > 0)
            {
                var cell = cells[cells.Count - 1]; //last element in list

                // See which adjacent cells are open.
                var unmadeCells = new List<VectorBase>();

                foreach (VectorBase dir in Direction.Cardinal)
                {
                    if (_canCarve(cell, dir))
                    {
                        unmadeCells.Add(dir);
                    }
                }

                if (unmadeCells.Count > 0)
                {
                    // Based on how "windy" passages are, try to prefer carving in the
                    // same direction.
                    VectorBase dir;
                    if (unmadeCells.Contains(lastDir) && Rng.Instance.Range(0, 100) < windingPercent)
                    {
                        dir = lastDir; //keep previous direction
                    }
                    else
                    {
                        dir = unmadeCells[Rng.Instance.Range(0, unmadeCells.Count)]; //pick new direction out of possible ones
                    }

                    _carve(cell + dir); //carve out wall between the valid cells
                    _carve(cell + dir * 2); //carve out valid cell

                    cells.Add(cell + dir * 2);
                    lastDir = dir;
                }
                else
                {
                    // No adjacent uncarved cells.
                    cells.RemoveAt(cells.Count - 1); //Remove Last element

                    // This path has ended.
                    lastDir = null;
                }
            }
        }

        //Marrt: was the hardest function for me to convert, there maybe errors
        private void _connectRegions()
        {
            // Find all of the tiles that can connect two (or more) regions.
            var connectorRegions = new Dictionary<VectorBase, HashSet<int>>(); //var connectorRegions = <Vec, Set<int>>{};

            //check each wall if it sits between 2 different regions and assign a Hashset to them
            //foreach (VectorBase pos in bounds.inflate(-1)) {
            for (var ix = 1; ix < stage.Width - 1; ix++)
            {
                for (var iy = 1; iy < stage.Height - 1; iy++)
                {
                    var pos = new VectorBase(ix, iy);

                    // Can't already be part of a region.
                    if (GetTile(pos).Type != TileTypeFactory.Instance.Wall)
                    {
                        continue;
                    }

                    var regions = new HashSet<int>();

                    foreach (VectorBase dir in Direction.Cardinal)
                    {
                        var indexer = (pos + dir);
                        var region = _regions[indexer.x, indexer.y];
                        //if (region != null) regions.Add(region);
                        if (region != -1)
                        {
                            regions.Add(region);
                        }
                    }

                    if (regions.Count < 2)
                    {
                        continue;
                    }

                    connectorRegions[pos] = regions; //add Hashset to current position
                }
            }

            var connectors = connectorRegions.Keys.ToList(); //var connectors = connectorRegions.keys.toList();

            //Marrt: I think it would make for nicer dungeons if all room-to-room connections would be tried first, therefore sort List
            if (tryRoomsFirst)
            {
                //bring connectors that have two rooms attached, to front		
                connectors.OrderBy(delegate (VectorBase con)
                {
                    var connectedRooms = 0;
                    foreach (VectorBase dir in Direction.Cardinal)
                    {
                        var indexer = (con + dir);
                        if (GetTile(indexer).Type == TileTypeFactory.Instance.Floor)
                        {
                            connectedRooms++;
                        }
                    }
                    return 2 - connectedRooms;
                });
            }

            // Keep track of which regions have been merged. This maps an original
            // region index to the one it has been merged to.
            var merged = new Dictionary<int, int>();
            var openRegions = new HashSet<int>();
            for (var i = 0; i <= _currentRegion; i++)
            {
                merged[i] = i;
                openRegions.Add(i);
            }

            // Keep connecting regions until we're down to one.
            while (openRegions.Count > 1)
            {
                //print (openRegions.Count+"|"+connectors.Count);
                VectorBase connector;
                if (tryRoomsFirst)
                {
                    connector = connectors[0]; //room-to-room are ordered first in list
                }
                else
                {
                    connector = connectors[Rng.Instance.Range(0, connectors.Count)]; //Rng.Instance.Item(connectors);
                }

                // Carve the connection.
                _addJunction(connector);

                // Merge the connected regions. We'll pick one region (arbitrarily) and
                // map all of the other regions to its index.
                //var regions = connectorRegions[connector].map((region) => merged[region]);
                var regions = connectorRegions[connector].Select(region => merged[region]);

                var dest = regions.First();
                var sources = regions.Skip(1).ToList();

                // Merge all of the affected regions. We have to look at *all* of the
                // regions because other regions may have previously been merged with
                // some of the ones we're merging now.
                for (var i = 0; i <= _currentRegion; i++)
                {
                    if (sources.Contains(merged[i]))
                    {
                        merged[i] = dest;
                    }
                }

                // The sources are no longer in use.
                //openRegions.removeAll(sources);
                openRegions.RemoveWhere(source => sources.Contains(source));

                // Remove any connectors that aren't needed anymore.
                connectors.RemoveAll(delegate (VectorBase pos)
                {
                    //	connectors.removeWhere((pos) {

                    // If the connector no long spans different regions, we don't need it.
                    //var regionss = connectorRegions[pos].map((region) => merged[region]).toSet();
                    var regionss = connectorRegions[pos].Select(region => merged[region]).ToHashSet(); //Extension Method to hashset

                    if (regionss.Count > 1)
                    {
                        return false;
                    }

                    // This connecter isn't needed, but connect it occasionally so that the dungeon isn't singly-connected.
                    if (Rng.Instance.Range(0, 100) < extraConnectorChance)
                    {
                        // Don't allow connectors right next to each other.				
                        foreach (VectorBase dir in Direction.Cardinal)
                        {
                            var indexer = (pos + dir);
                            if (GetTile(indexer).Type == TileTypeFactory.Instance.OpenDoor)
                            {
                                return true;
                            }
                        }

                        //if no connectors are adjacent, add additional connector
                        _addJunction(pos);
                    }
                    return true;
                });
            }
        }

        private void _addJunction(VectorBase pos)
        {
            //open / closedness of a door can be determined in a later manipulations, so i removed it
            if (Rng.Instance.OneIn(4))
            {
                SetTile(pos, Rng.Instance.OneIn(3) ? TileTypeFactory.Instance.OpenDoor : TileTypeFactory.Instance.Floor);
            }
            else
            {
                SetTile(pos, TileTypeFactory.Instance.ClosedDoor);
            }
        }

        private void _removeDeadEnds()
        {
            var done = false;

            while (!done)
            {
                done = true;

                //foreach (VectorBase pos in bounds.inflate(-1)) {
                for (var ix = 1; ix < stage.Width - 1; ix++)
                {
                    for (var iy = 1; iy < stage.Height - 1; iy++)
                    {
                        var pos = new VectorBase(ix, iy);

                        if (GetTile(pos).Type == TileTypeFactory.Instance.Wall)
                        {
                            continue;
                        }

                        // If it only has one exit, it's a dead end.
                        var exits = 0;
                        foreach (VectorBase dir in Direction.Cardinal)
                        {
                            if (GetTile(pos + dir).Type != TileTypeFactory.Instance.Wall)
                            {
                                exits++;
                            }
                        }

                        if (exits != 1)
                        {
                            continue;
                        }

                        done = false;
                        SetTile(pos, TileTypeFactory.Instance.Wall);
                        _regions[pos.x, pos.y] = -1;
                    }
                }
            }
        }

        private void _streamLineCorridors()
        {
            /*Added by Marrt taken from this user comment on the source page:
                Peeling • 7 months ago
                As regards the disagreeable windiness between rooms: looking at the output you could get rid of most of it thus:
                Trace each linear corridor section (terminated by branches or rooms)
                Once you have the start and end of a section, retrace your steps. If you find a point where you could dig through one block to make a shortcut to an earlier part of the section, do so, and fill in the unwanted part. Continue until you reach the start of the section.
                Repeat for all linear corridor sections.
            */

            //STEP 1: gather all TileTypeFactory.Floor, these are all corridor TileTypeFactory
            var corridors = new List<VectorBase>();
            var traces = new List<List<VectorBase>>();

            for (var ix = 1; ix < stage.Width - 1; ix++)
            {
                for (var iy = 1; iy < stage.Height - 1; iy++)
                {
                    if (GetTile(new VectorBase(ix, iy)).Type == TileTypeFactory.Instance.Floor)
                    {
                        corridors.Add(new VectorBase(ix, iy));
                    }
                }
            }

            //STEP 2: gather corridor traces, these are all line segments that are between doors or branching points which themselves are fixed now		
            //extract Line Segments seperated by branching points or doorsteps		

            var failsave = 1000;
            while (corridors.Count > 0 && failsave > 0)
            {
                if (failsave == 1)
                {
                }
                failsave--;

                // See which adjacent cells are open.
                var segment = new List<VectorBase>();
                var current = corridors[0]; //arbitrary start		
                buildLineSegment(current, ref corridors, ref segment, 0, true); //recursive search

                if (segment.Count > 4)
                {
                    //lineSegment has to have at least 5 parts to potentially contain a shortcut
                    traces.Add(segment);
                    //debug	//	int g = Random.Range(100,300);	foreach(VectorBase pos in segment){	_regions[pos.x, pos.y] = g;	}
                }
            }

            //STEP 3: backtrace traces and check for shortcuts within short range (1 wall in between), then carve a shortcut and uncarve the trace up to that point		
            foreach (var trace in traces)
            {
                var finalTrace = new List<VectorBase>();
                var skipIndex = 0; //shortcut skips iterations			

                for (var i = 0; i < trace.Count; i++)
                {
                    if (i < skipIndex)
                    {
                        continue;
                    }

                    finalTrace.Add(trace[i]); //add current position to final path

                    foreach (VectorBase dir in Direction.Cardinal)
                    {
                        if (GetTile(trace[i] + dir).Type == TileTypeFactory.Instance.Wall)
                        {
                            //if we see a wall in test direction

                            var shortcut = trace[i] + dir + dir;
                            if (trace.Contains(shortcut) && !finalTrace.Contains(shortcut))
                            {
                                //and behind that wall an already visited pos of this trace that has not been removed

                                //get index of shortcut so we know how and if to skip
                                skipIndex = trace.FindIndex(delegate (VectorBase x) { return x == shortcut; }); //implicit predicate							
                                if (i > skipIndex)
                                {
                                    continue;
                                } //detected an already obsolete path, we cannot make a shortcut to it
                                finalTrace.Add(trace[i] + dir); //new shortcut connection is added to final sum
                                //print ("shortcut"+i+"->"+skipIndex);
                            }
                        }
                    }
                }

                //uncarve old trace
                foreach (var pos in trace)
                {
                    SetTile(pos, TileTypeFactory.Instance.Wall);
                    _regions[pos.x, pos.y] = -1;
                }

                //recarve trace
                foreach (var pos in finalTrace)
                {
                    _carve(pos);
                    _regions[pos.x, pos.y] = 100;
                }
            }
        }

        //recursive line builder
        private int buildLineSegment(VectorBase current, ref List<VectorBase> source, ref List<VectorBase> target, int currentDepth, bool addAtEnd)
        {
            if (currentDepth > 1000)
            {
                return currentDepth + 1;
            } //failsave

            //check if we are a doorstep or branch, these must not be moved or else
            var exits = 0;
            foreach (VectorBase dir in Direction.Cardinal)
            {
                if (GetTile(current + dir).Type != TileTypeFactory.Instance.Wall)
                {
                    //if there is anything other than a wall we have an exit or else, doorsteps will have at least 2 non walls (door + path)
                    exits++;
                }
            }
            if (exits > 2)
            {
                source.Remove(current); //never look at this tile again
                return currentDepth;
            }
            if (addAtEnd)
            {
                target.Insert(0, current); //at least part of a valid lineSegment
            }
            else
            {
                target.Add(current);
            }

            //find adjacent fields, there are only up to 2 directions possible on any lineSegment point, we can only ever find one valid after the first		
            foreach (VectorBase dir in Direction.Cardinal)
            {
                if (source.Contains(current + dir) && !target.Contains(current + dir))
                {
                    //depth first
                    currentDepth = buildLineSegment(current + dir, ref source, ref target, currentDepth, addAtEnd);
                    //only first call can run twice because it may start in the middle of a segment
                    addAtEnd = false; //we want an ordered list, so initial depthsearch will be added at start, the following at the end
                }
            }

            source.Remove(current);
            return currentDepth + 1;
        }

        public void StartRegion()
        {
            _currentRegion++;
        }

        /// Gets whether or not an opening can be carved from the given starting
        /// [Cell] at [pos] to the adjacent Cell facing [direction]. Returns `true`
        /// if the starting Cell is in bounds and the destination Cell is filled
        /// (or out of bounds).
        /// </returns>
        private bool _canCarve(VectorBase pos, VectorBase direction)
        {
            // Must end in bounds.

            var iv2 = pos + direction * 3;
            var v2 = new VectorBase(iv2.x, iv2.y);
            var bounds = new Rect(0, 0, stage.Width, stage.Height);

            if (!bounds.Contains(v2))
            {
                return false;
            }

            // Destination must not be open.
            return GetTile(pos + direction * 2).Type == TileTypeFactory.Instance.Wall;
        }

        private void _carve(VectorBase pos, TileType type = null)
        {
            SetTile(pos, type ?? TileTypeFactory.Instance.Floor); // if non is stated, default is floor

            //print (pos.x +","+ pos.y);
            _regions[pos.x, pos.y] = _currentRegion;
        }

        #endregion
    }
}