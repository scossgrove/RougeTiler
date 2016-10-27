using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Utilities;

/// <summary>
/// https://www.dartlang.org/
/// https://github.com/munificent/piecemeal/blob/master/lib/src/rect.dart
/// https://github.com/munificent/hauberk/tree/master/lib/src/engine
/// https://github.com/munificent/piecemeal/blob/master/lib/src/direction.dart
/// </summary>

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    /// The random dungeon generator.
    public class Forrest : StageBuilder
    {
        /// A forest is a collection of grassy meadows surrounded by trees and
        /// connected by passages.
        public int numMeadows { get; set; } = 10;

        /// The number of iterations of Lloyd's algorithm to run on the points.
        ///
        /// Fewer results in clumpier, less evenly spaced points. More results in
        /// more evenly spaced but can eventually look too regular.
        public int voronoiIterations { get; set; } = 5;

        public List<VectorBase> Meadows { get; set; } = new List<VectorBase>();

        /// <summary>
        /// 
        /// </summary>
        public override void Generate(Stage stage)
        {
            BindStage(stage);

            Fill(TileTypeFactory.Instance.Tree);

            // Randomly position the meadows.
            for (var i = 0; i < numMeadows; i++)
            {
                var x = Rng.Instance.Range(0, stage.Width);
                var y = Rng.Instance.Range(0, stage.Height);
                Meadows.Add(new VectorBase(x, y));
            }

            FindClosestMeadow(stage);

            ConnectMeadows();

            // Carve out the meadows.
            foreach (var point in Meadows)
            {
                var meadowWidth = Rng.Instance.Range(3, 9);
                var meadowHeight = Rng.Instance.Range(3, 9);
                CarveEllipse(point, meadowWidth, meadowHeight, TileTypeFactory.Instance.Grass);

                if (meadowWidth > 4 && meadowHeight > 4)
                {
                    var chance = Rng.Instance.Range(0, 100);
                    if (chance > 75)
                    {
                        placeHouse(point);
                    }
                }
            }

            // Randomly switch some tiles around.
            Erode(10000, floor: TileTypeFactory.Instance.Grass, wall: TileTypeFactory.Instance.Tree);

            CleanOutSideBoundary(TileTypeFactory.Instance.Tree);
            
            CreateSolidTiles(floor: TileTypeFactory.Instance.Grass, wall: TileTypeFactory.Instance.Tree);

            ReplaceGenericWallWithDirectionalWalls();

            // TODO: Randomly vary the tree type.
            //var trees = [TileTypeFactory.tree, TileTypeFactory.treeAlt1, TileTypeFactory.treeAlt2];
            //for (var pos in stage.bounds)
            //{
            //    if (getTile(pos) == TileTypeFactory.tree)
            //    {
            //        setTile(pos, rng.item(trees));
            //    }
            //}

            // TODO: need to put in a stair up and a stair down
            if (stage.HasExitDown)
            {
                var meadowForStairsDown = Rng.Instance.Range(0, numMeadows);
                var stairVector = Meadows[meadowForStairsDown];
                var stairPosition = stage.FindDistantOpenTileNear(stairVector);
                stage.Tiles[stairPosition.x, stairPosition.y] = new Tile()
                {
                    IsExplored = false,
                    Position = stairPosition,
                    Type = TileTypeFactory.Instance.StairsDown,
                    Visible = false
                };
                stage.StairDownPosition = stairPosition;
            }
            if (stage.HasExitUp)
            {
                var meadowForStairsUp = Rng.Instance.Range(0, numMeadows);
                var stairVector = Meadows[meadowForStairsUp];
                var stairPosition = stage.FindDistantOpenTileNear(stairVector);
                stage.Tiles[stairPosition.x, stairPosition.y] = new Tile()
                {
                    IsExplored = false,
                    Position = stairPosition,
                    Type = TileTypeFactory.Instance.StairsUp,
                    Visible = false
                };
                stage.StairUpPosition = stairPosition;
            }

        }

        private void placeHouse(VectorBase position)
        {
            var widthOfHouse = Rng.Instance.Range(3, 6);
            var heightOfHouse = Rng.Instance.Range(3, 6);

            var directionOfDoor = Rng.Instance.Range(0, 4);
            for (int rowIndex = 0; rowIndex < heightOfHouse; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < widthOfHouse; columnIndex++)
                {
                    bool setWallTile = false;
                    bool setDoorTile = false;
                    if (rowIndex == 0 || rowIndex == heightOfHouse - 1)
                    {
                        if (directionOfDoor == 0 && rowIndex == 0)
                        {
                            CalculateWallOrDoor(widthOfHouse, columnIndex, ref setDoorTile, ref setWallTile);
                        }
                        else if (directionOfDoor == 2 && rowIndex == heightOfHouse - 1)
                        {
                            CalculateWallOrDoor(widthOfHouse, columnIndex, ref setDoorTile, ref setWallTile);
                        }
                        else
                        {
                            setWallTile = true;
                        }
                    }
                    else if (columnIndex == 0 || columnIndex == widthOfHouse - 1)
                    {
                        if (directionOfDoor == 1 && columnIndex == 0)
                        {
                            CalculateWallOrDoor(heightOfHouse, rowIndex, ref setDoorTile, ref setWallTile);
                        }
                        else if (directionOfDoor == 3 && columnIndex == widthOfHouse - 1)
                        {
                            CalculateWallOrDoor(heightOfHouse, rowIndex, ref setDoorTile, ref setWallTile);
                        }
                        else
                        {
                            setWallTile = true;
                        }
                    }

                    if (setWallTile)
                    {
                        SetTile(new VectorBase(position.x - 2 + columnIndex, position.y - 2 + rowIndex), TileTypeFactory.Instance.Wall);
                    }

                    if (setDoorTile)
                    {
                        if (widthOfHouse == 3 && heightOfHouse == 3)
                        {
                            SetTile(new VectorBase(position.x - 2 + columnIndex, position.y - 2 + rowIndex), TileTypeFactory.Instance.Grass);
                        }
                        else
                        {
                            SetTile(new VectorBase(position.x - 2 + columnIndex, position.y - 2 + rowIndex), TileTypeFactory.Instance.ClosedDoor);
                        }
                    }
                }
            }

            // todo: maybe drop some loot here?
        }

        private void CalculateWallOrDoor(int size, int index, ref bool setDoorTile, ref bool setWallTile)
        {
            switch (size)
            {
                case 3:
                {
                    if (index == 1)
                    {
                        setDoorTile = true;
                    }
                    else
                    {
                        setWallTile = true;
                    }
                    break;
                }
                case 4:
                {
                    if (index == 2)
                    {
                        setDoorTile = true;
                    }
                    else
                    {
                        setWallTile = true;
                    }
                    break;
                }
                case 5:
                {
                    if (index == 3)
                    {
                        setDoorTile = true;
                    }
                    else
                    {
                        setWallTile = true;
                    }
                    break;
                }
                case 6:
                {
                    if (index == 4)
                    {
                        setDoorTile = true;
                    }
                    else
                    {
                        setWallTile = true;
                    }
                    break;
                }
                default:
                {
                    setWallTile = true;
                    break;
                }
            }
        }

        private void ConnectMeadows()
        {
            // Connect all of the points together.
            var connected = new List<VectorBase>() { Meadows.Last() };

            var toConnect = new List<VectorBase>();
            toConnect = Meadows.Clone().ToList();

            while (toConnect.Any())
            {
                VectorBase bestFrom = null;
                var bestToIndex = -1;
                var bestDistance = int.MaxValue;
                foreach (var from in connected)
                {
                    for (var i = 0; i < toConnect.Count; i++)
                    {
                        var distance = (from - toConnect[i]).lengthSquared();
                        if (bestDistance == int.MaxValue ||
                            distance < bestDistance)
                        {
                            bestFrom = from;
                            bestToIndex = i;
                            bestDistance = distance;
                        }
                    }
                }

                var to = toConnect[bestToIndex];
                toConnect.RemoveAt(bestToIndex);

                connected.Add(to);
                CarvePath(bestFrom, to, TileTypeFactory.Instance.Grass);
            }
        }

        private void FindClosestMeadow(Stage stage)
        {
            // Space them out more evenly by moving each point to the centroid of its
            // cell in the Voronoi diagram of the points. In other words, for each
            // point, we find the (approximate) region of the stage where that point
            // is the closest one. Then we move the point to the center of that region.
            // http://en.wikipedia.org/wiki/Lloyd%27s_algorithm
            for (int outerLooper = 0; outerLooper < voronoiIterations; outerLooper++)
            {
                // For each cell in the stage, determine which point it's nearest to.
                var regions = new List<List<VectorBase>>();
                foreach (var meadow in Meadows)
                {
                    regions.Add(new List<VectorBase>() { meadow });
                }

                // Find the closest cell to a meadow position
                foreach (var cell in stage.Bounds().PointsInRect())
                {
                    var nearest = 0;
                    var nearestDistanceSquared = 99999999;
                    for (var i = 0; i < numMeadows; i++)
                    {
                        var offset = Meadows[i] - cell;
                        if (offset.lengthSquared() < nearestDistanceSquared)
                        {
                            nearestDistanceSquared = offset.lengthSquared();
                            nearest = i;
                        }
                    }

                    regions[nearest].Add(cell);
                }

                // Now move each point to the centroid of its region. The centroid is
                // just the average of all of the cells in the region.
                for (int meadowIndex = 0; meadowIndex < numMeadows; meadowIndex++)
                {
                    var region = regions[meadowIndex];
                    Meadows[meadowIndex] = AverageVectorInList(region);
                }
            }
        }

        private VectorBase AverageVectorInList(List<VectorBase> region)
        {
            var meanVector = new VectorBase(0, 0);
            if (region.Count == 0)
            {
                return meanVector;
            }

            for (int listIndex = 0; listIndex < region.Count; listIndex++)
            {
                meanVector = meanVector + region[listIndex];
            }

            meanVector.x = meanVector.x / region.Count;
            meanVector.y = meanVector.y / region.Count;

            return meanVector;
        }
    }
}