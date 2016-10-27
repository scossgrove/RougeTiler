using System;
using System.CodeDom;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;

/// <summary>/// 
/// http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
/// </summary>

namespace Coslen.RogueTiler.Domain.Engine
{
    public abstract class StageBuilder
    {
        public Stage stage;

        public Rect Bounds()
        {
            return stage.Bounds();
        }

        public abstract void Generate(Stage stage);


        public void BindStage(Stage stage)
        {
            this.stage = stage;
        }

        public Tile GetTile(VectorBase pos)
        {
            return stage[pos];
        }

        public void SetTile(VectorBase pos, TileType type = null)
        {
            if (pos.x >= stage.Width || pos.y >= stage.Height || pos.x < 0 || pos.y < 0)
            {
                throw new ApplicationException($"Out of Bounds exception. Target = [{pos.x},{pos.y}]");
            }

            if (stage[pos.x, pos.y] == null)
            {
                stage[pos.x, pos.y] = new Tile();
            }
            stage[pos].Type = type;
        }

        public void Fill(TileType tile)
        {
            for (var y = 0; y < stage.Height; y++)
            {
                for (var x = 0; x < stage.Width; x++)
                {
                    SetTile(new Vector(x, y), tile);
                }
            }
        }

        /// Randomly turns some [wall] tiles into [floor] and vice versa.
        public void Erode(int iterations, TileType floor, TileType wall)
        {
            if (floor == null)
            {
                floor = TileTypeFactory.Instance.Floor;
            }
            if (wall == null)
            {
                wall = TileTypeFactory.Instance.Wall;
            }

            var bounds = stage.Bounds();
            bounds.Inflate(-1);

            for (var i = 0; i < iterations; i++)
            {
                // TODO: This way this works is super inefficient. Would be better to
                // keep track of the floor tiles near open ones and choose from them.
                var pos = Rng.Instance.vectorInRect(bounds);

                var here = GetTile(pos).Type;
                if (here != wall)
                {
                    continue;
                }

                // Keep track of how many floors we're adjacent too. We will only erode
                // if we are directly next to a floor.
                var floors = 0;

                foreach (var dir in Direction.All)
                {
                    var targetPosition = pos + dir;

                    if (targetPosition.x <= 0 || targetPosition.x >= stage.Width)
                        continue;

                    if (targetPosition.y <= 0 || targetPosition.y >= stage.Height)
                        continue;

                    var tile = GetTile(targetPosition).Type;
                    if (tile == floor)
                    {
                        floors++;
                    }
                }

                // Prefer to erode tiles near more floor tiles so the erosion isn't too
                // spiky.
                if (floors < 2)
                {
                    continue;
                }
                if (Rng.Instance.OneIn(9 - floors))
                {
                    SetTile(pos, floor);
                }
            }
        }

        public void CarvePath(VectorBase from, VectorBase to, TileType floorTile)
        {
            var los = new Los(from, to);
            foreach (var pos in los)
            {
                if (pos == to) break;

                VectorBase position = (VectorBase)pos;

                // Make slightly wider passages.
                SetTile(position, floorTile);
                if (position.x + 1 < stage.Width)
                {
                    SetTile(position.offsetX(1), floorTile);
                }
                if (position.y + 1 < stage.Height)
                {
                    SetTile(position.offsetY(1), floorTile);
                }
            }
        }

        public void CarveCircle(VectorBase center, int radius, TileType floorTile)
        {
            CarveEllipse(center, radius, radius, floorTile);
        }

        public void CarveEllipse(VectorBase center, int width, int heigth, TileType floorTile)
        {
            for (int y = -heigth; y <= heigth; y++)
            {
                for (int x = -width; x <= width; x++)
                {
                    double dx = (double)x / (double)width;
                    double dy = (double)y / (double)heigth;
                    if (dx*dx + dy*dy <= 1)
                    {
                        try
                        {
                            SetTile(new Vector(center.x + x, center.y + y), floorTile);
                        }
                        catch (ApplicationException)
                        {}
                    }
                }
            }
        }

        public void CarveLine(VectorBase start, VectorBase end, TileType floorTile)
        {
            var los = new Los(start, end);
            foreach (var point in los.Points)
            {
                SetTile(point, floorTile);
            }
        }


        /// 
        /// These functions will transform walls into the ascii line art we want
        /// to use.
        /// 
        
        public void CreateSolidTiles(TileType floor, TileType wall)
        {
            if (floor == null)
            {
                floor = TileTypeFactory.Instance.Floor;
            }

            for (var widthIndex = 0; widthIndex < stage.Width; widthIndex++)
            {
                for (var heightIndex = 0; heightIndex < stage.Height; heightIndex++)
                {
                    var connectedToAWall = false;
                    var pos = new VectorBase(widthIndex, heightIndex);
                    foreach (VectorBase dir in Direction.All)
                    {
                        if ((pos + dir).x < 0 || (pos + dir).x >= stage.Width - 1 || (pos + dir).y < 0 || (pos + dir).y >= stage.Height - 1)
                        {
                            continue;
                        }

                        if (GetTile((pos + dir)).Type == floor)
                        {
                            connectedToAWall = true;
                            break;
                        }
                    }

                    if (!connectedToAWall)
                    {
                        SetTile(pos, TileTypeFactory.Instance.Solid);
                    }
                }
            }
        }
        
        public void CleanOutSideBoundary(TileType wallTile)
        {
            for (int rowIndex = 0; rowIndex < stage.Height; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < stage.Width; columnIndex++)
                {
                    bool setWallTile = false;
                    if (rowIndex == 0 || rowIndex == stage.Height - 1)
                    {
                        setWallTile = true;
                    }
                    else if (columnIndex == 0 || columnIndex == stage.Width - 1)
                    {
                        setWallTile = true;
                    }
                    if (setWallTile)
                    {
                        SetTile(new VectorBase(columnIndex, rowIndex), wallTile);
                    }
                }
            }
        }

        public void ReplaceGenericWallWithDirectionalWalls()
        {
            for (var widthIndex = 0; widthIndex < stage.Width; widthIndex++)
            {
                for (var heightIndex = 0; heightIndex < stage.Height; heightIndex++)
                {
                    var pos = new VectorBase(widthIndex, heightIndex);
                    var tile = GetTile(pos);

                    if (tile.Type.Name == "wall")
                    {
                        var numberOfSurroundingWalls = CalculateSurroundingWalls(pos);
                        ReformateWall(numberOfSurroundingWalls, pos);
                    }

                    if (tile.Type.Name == "open door")
                    {
                        var numberOfSurroundingWalls = CalculateSurroundingWalls(pos);
                        ReformateDoor(numberOfSurroundingWalls, pos, true);
                    }

                    if (tile.Type.Name == "closed door")
                    {
                        var numberOfSurroundingWalls = CalculateSurroundingWalls(pos);
                        ReformateDoor(numberOfSurroundingWalls, pos, false);
                    }
                }
            }
        }

        private void ReformateWall(CleanDirection surroundingWalls, VectorBase pos)
        {
            switch ((int)surroundingWalls)
            {
                case 0:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallColumn);
                        break;
                    }
                case 1:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallSEnd);
                        break;
                    }
                case 2:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallWEnd);
                        break;
                    }
                case 3:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallCornerSW);
                        break;
                    }
                case 4:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallNEnd);
                        break;
                    }
                case 5:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallNS);
                        break;
                    }
                case 6:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallCornerNW);
                        break;
                    }
                case 7:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallTE);
                        break;
                    }
                case 8:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallEEnd);
                        break;
                    }
                case 9:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallCornerSE);
                        break;
                    }
                case 10:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallEW);
                        break;
                    }
                case 11:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallTN);
                        break;
                    }
                case 12:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallCornerNE);
                        break;
                    }
                case 13:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallTW);
                        break;
                    }
                case 14:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallTS);
                        break;
                    }
                case 15:
                    {
                        SetTile(pos, TileTypeFactory.Instance.WallCross);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void ReformateDoor(CleanDirection surroundingWalls, VectorBase pos, bool isOpen)
        {
            switch ((int)surroundingWalls)
            {
                case 5:
                    {
                        if (isOpen)
                        {
                            SetTile(pos, TileTypeFactory.Instance.OpenDoorNS);
                        }
                        else
                        {
                            SetTile(pos, TileTypeFactory.Instance.ClosedDoorNS);
                        }
                        break;
                    }
                case 10:
                    {
                        if (isOpen)
                        {
                            SetTile(pos, TileTypeFactory.Instance.OpenDoorEW);
                        }
                        else
                        {
                            SetTile(pos, TileTypeFactory.Instance.ClosedDoorEW);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        [Flags]
        private enum CleanDirection
        {
            None = 0,
            North = 1,
            East = 2,
            South = 4,
            West = 8
        }

        private CleanDirection CalculateSurroundingWalls(VectorBase pos)
        {
            var numberOfSurroundingWalls = CleanDirection.None;
            foreach (VectorBase dir in Direction.Cardinal)
            {
                var targetPos = pos + dir;
                if (targetPos.x < 0 || targetPos.x > stage.Width - 1 || targetPos.y < 0 || targetPos.y > stage.Height - 1)
                {
                    continue;
                }

                var isWall = GetTile(targetPos).Type.IsWall;
                var targetTypeName = GetTile(targetPos).Type.Name;
                if (targetTypeName == "wall" || targetTypeName == "closed door" || targetTypeName == "open door" || targetTypeName == "closed door ns" || targetTypeName == "open door ns" || targetTypeName == "closed door ew" || targetTypeName == "open door ew" || isWall)
                {
                    if (dir == Direction.North)
                    {
                        numberOfSurroundingWalls = numberOfSurroundingWalls | CleanDirection.South;
                    }
                    if (dir == Direction.South)
                    {
                        numberOfSurroundingWalls = numberOfSurroundingWalls | CleanDirection.North;
                    }
                    if (dir == Direction.West)
                    {
                        numberOfSurroundingWalls = numberOfSurroundingWalls | CleanDirection.East;
                    }
                    if (dir == Direction.East)
                    {
                        numberOfSurroundingWalls = numberOfSurroundingWalls | CleanDirection.West;
                    }
                }
            }
            return numberOfSurroundingWalls;
        }



    }
}