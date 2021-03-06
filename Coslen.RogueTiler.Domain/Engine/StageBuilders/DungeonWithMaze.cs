﻿using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public abstract class DungeonWithMaze : StageBuilder
    {
        private int _currentRegion = -1;
            // The index of the current region (=connected carved area) being carved, -1 = default, wall

        public int _dungeonHeight = 21;

        public int _dungeonWidth = 21;

        public int[,] _regions; //2D-Array
        public List<Rect> _rooms { get; set; } = new List<Rect>(); //list of placed rooms

        public int extraConnectorChance = 0;
            //The inverse chance of adding a connector between two regions that have already been joined for more interconnection between regions

        //Hauberk Dungeon variables
        public int numRoomTries = 250; //Room placement tries
        public int roomExtraSize = 0; //Increasing this allows rooms to be larger.
        public bool streamLine = true; //streamline corridors between branchpoints and doors

        public bool tryRoomsFirst = false;
            //try to make room-to-room connections before making corridor-to-room connections (corridor-to-corridor are impossible)

        public int windingPercent = 20; //chance a maze will make a turn which will make it more winding

        /// <summary>
        ///     Generate a room and maze dungeon, http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
        /// </summary>
        public override void Generate(Stage stage)
        {
            //Debugger.Info(" * {0} = [{1}]", "Generate", "Begin");

            _dungeonWidth = stage.Width;
            _dungeonHeight = stage.Height;

            //check size
            if (_dungeonWidth%2 == 0 || _dungeonHeight%2 == 0)
            {
                throw new ApplicationException("The stage must be odd-sized.");
            }

            GenerateMaze(stage);
            GenerateMazeFeatures(stage);
        }

        private void GenerateMazeFeatures(Stage stage)
        {
            GenerateStairways(stage);
        }

        private void GenerateStairways(Stage stage1)
        {
            VectorBase posUp = null;
            if (stage1.HasExitUp)
            {
                var roomUp = _rooms[Rng.Instance.Range(0, _rooms.Count)];

                while (posUp == null)
                {
                    posUp = roomUp.PointsInRect()[Rng.Instance.Range(roomUp.width*roomUp.height)];
                    if (GetTile(posUp).Type == TileTypeFactory.Instance.Wall)
                    {
                        posUp = null;
                    }
                }
                SetTile(posUp, TileTypeFactory.Instance.StairsUp);
                stage1.StairUpPosition = posUp;
            }

            if (stage1.HasExitDown)
            {
                var roomDown = _rooms[Rng.Instance.Range(0, _rooms.Count)];

                VectorBase posDown = null;
                while (posDown == null)
                {
                    posDown = roomDown.PointsInRect()[Rng.Instance.Range(roomDown.width*roomDown.height)];
                    if (posDown == posUp || GetTile(posDown).Type == TileTypeFactory.Instance.Wall)
                    {
                        posDown = null;
                    }
                }
                SetTile(posDown, TileTypeFactory.Instance.StairsDown);
                stage1.StairDownPosition = posDown;
            }
        }

        #region "Maze Generation Functions"

        private void GenerateMaze(Stage stage)
        {
            BindStage(stage);

            Fill(TileTypeFactory.Instance.Wall);

            //init grids
            _regions = new int[_dungeonWidth, _dungeonHeight];
            _currentRegion = -1; //reset

            for (var x = 0; x < _dungeonWidth; x++)
            {
                for (var y = 0; y < _dungeonHeight; y++)
                {
                    _regions[x, y] = _currentRegion; //-1
                }
            }

            AddRooms(); //randomly place rooms

            addMaze();

            CreateSolidTiles(TileTypeFactory.Instance.Floor, TileTypeFactory.Instance.Wall);

            ReplaceGenericWallWithDirectionalWalls();
        }

        private void addMaze()
        {
            // Fill in all of the empty space with mazes.
            for (var x = 1; x < _dungeonWidth; x += 2)
            {
                for (var y = 1; y < _dungeonHeight; y += 2)
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

            //function Marrt added to streamline corridors
            if (streamLine)
            {
                _streamLineCorridors();
            }
        }

        /// Places rooms ignoring the existing maze corridors.
        public abstract void AddRooms();

        public void AddRoomToMap(Rect room, int roomSizeWidth, int roomSizeHeight)
        {
            //add non-overlapping room
            _rooms.Add(room);

            _startRegion();

            for (var ix = room.left; ix < room.left + roomSizeWidth; ix++)
            {
                for (var iy = room.top; iy < room.top + roomSizeHeight; iy++)
                {
                    _carve(new VectorBase(ix, iy), TileTypeFactory.Instance.Floor);
                }
            }
        }

        /// Implementation of the "growing tree" algorithm from here:
        /// http://www.astrolog.org/labyrnth/algrithm.htm.
        private void _growMaze(VectorBase start)
        {
            var cells = new List<VectorBase>();
            VectorBase lastDir = null;


            _startRegion();
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
                        dir = unmadeCells[Rng.Instance.Range(0, unmadeCells.Count)];
                            //pick new direction out of possible ones
                    }

                    _carve(cell + dir); //carve out wall between the valid cells
                    _carve(cell + dir*2); //carve out valid cell

                    cells.Add(cell + dir*2);
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
            var connectorRegions = new Dictionary<VectorBase, HashSet<int>>();
                //var connectorRegions = <Vec, Set<int>>{};

            //check each wall if it sits between 2 different regions and assign a Hashset to them
            //foreach (VectorBase pos in bounds.inflate(-1)) {
            for (var ix = 1; ix < _dungeonWidth - 1; ix++)
            {
                for (var iy = 1; iy < _dungeonHeight - 1; iy++)
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
                connectors.OrderBy(delegate(VectorBase con)
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
                connectors.RemoveAll(delegate(VectorBase pos)
                {
                    //	connectors.removeWhere((pos) {

                    // If the connector no long spans different regions, we don't need it.
                    //var regionss = connectorRegions[pos].map((region) => merged[region]).toSet();
                    var regionss = connectorRegions[pos].Select(region => merged[region]).ToHashSet();
                        //Extension Method to hashset

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
                for (var ix = 1; ix < _dungeonWidth - 1; ix++)
                {
                    for (var iy = 1; iy < _dungeonHeight - 1; iy++)
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

            for (var ix = 1; ix < _dungeonWidth - 1; ix++)
            {
                for (var iy = 1; iy < _dungeonHeight - 1; iy++)
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
                                skipIndex = trace.FindIndex(delegate(VectorBase x) { return x == shortcut; });
                                    //implicit predicate							
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
        private int buildLineSegment(VectorBase current, ref List<VectorBase> source, ref List<VectorBase> target,
            int currentDepth, bool addAtEnd)
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
                    addAtEnd = false;
                        //we want an ordered list, so initial depthsearch will be added at start, the following at the end
                }
            }

            source.Remove(current);
            return currentDepth + 1;
        }

        private void _startRegion()
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

            var iv2 = pos + direction*3;
            var v2 = new VectorBase(iv2.x, iv2.y);
            var bounds = new Rect(0, 0, _dungeonWidth, _dungeonHeight);

            if (!bounds.Contains(v2))
            {
                return false;
            }

            // Destination must not be open.
            return GetTile(pos + direction*2).Type == TileTypeFactory.Instance.Wall;
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