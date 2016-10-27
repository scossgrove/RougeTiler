using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public class RoomDecorator : Dungeon
    {
        public override void Generate(Stage stage)
        {
            base.Generate(stage);
        }
        
        /// Places a few tables in the room.
        public bool decorateTable(Rect room)
        {
            var pos = Rng.Instance.vectorInRect(room);

            // Don't block an exit.
            if (pos.x == room.left && GetTile(pos.offsetX(-1)).Type != TileTypeFactory.Instance.Wall)
            {
                return false;
            }

            if (pos.y == room.top && GetTile(pos.offsetY(-1)).Type != TileTypeFactory.Instance.Wall)
            {
                return false;
            }

            if (pos.x == room.right && GetTile(pos.offsetX(1)).Type != TileTypeFactory.Instance.Wall)
            {
                return false;
            }

            if (pos.y == room.bottom && GetTile(pos.offsetY(1)).Type != TileTypeFactory.Instance.Wall)
            {
                return false;
            }

            SetTile(pos, TileTypeFactory.Instance.Table);
            return true;
        }

        /// Add rows of pillars to the edge(s) of the room.
        public bool decoratePillars(Rect room)
        {
            if (room.width < 5) return false;
            if (room.height < 5) return false;

            // Only odd-sized sides get them, so make sure at least one side is.
            if ((room.width % 2 == 0) && (room.height % 2 == 0)) return false;

            var type = Rng.Instance.OneIn(2) ? TileTypeFactory.Instance.Wall : TileTypeFactory.Instance.LowWall;

            if (room.width % 2 == 1)
            {
                for (var x = room.left + 1; x < room.right - 1; x += 2)
                {
                    SetTile(new VectorBase(x, room.top + 1), type);
                    SetTile(new VectorBase(x, room.bottom - 2), type);
                }
            }

            if (room.height % 2 == 1)
            {
                for (var y = room.top + 1; y < room.bottom - 1; y += 2)
                {
                    SetTile(new VectorBase(room.left + 1, y), type);
                    SetTile(new VectorBase(room.right - 2, y), type);
                }
            }

            return true;
        }

        /// If [room] is big enough, adds a floating room inside of it with a single
        /// entrance.
        public bool decorateInnerRoom(Rect room)
        {
            if (room.width < 5) return false;
            if (room.height < 5) return false;

            var width = Rng.Instance.Inclusive(3, room.width - 2);
            var height = Rng.Instance.Inclusive(3, room.height - 2);
            var x = Rng.Instance.Range(room.x + 1, room.right - width);
            var y = Rng.Instance.Range(room.y + 1, room.bottom - height);

            // Trace the room.
            var type = Rng.Instance.OneIn(3) ? TileTypeFactory.Instance.Wall : TileTypeFactory.Instance.LowWall;
            foreach (var pos in new Rect(x, y, width, height).PointsTracingRect())
            {
                SetTile(pos, type);
            }

            // Make an entrance. If it's a narrow room, always place the door on the
            // wider side.
            List<VectorBase> directions;
            if ((width == 3) && (height > 3))
            {
                directions = new List<VectorBase> { Direction.East, Direction.West };
            }
            else if ((height == 3) && (width > 3))
            {
                directions = new List<VectorBase> { Direction.North, Direction.South};
            }
            else
            {
                directions = new List<VectorBase> { Direction.North, Direction.South, Direction.East, Direction.West};
            }

            VectorBase door = null;
            var testDirection = Rng.Instance.Item(directions);
            if (testDirection == Direction.North)
            {
                door = new VectorBase(Rng.Instance.Range(x + 1, x + width - 1), y);
            }else if (testDirection == Direction.South)
            {
                door = new VectorBase(Rng.Instance.Range(x + 1, x + width - 1), y + height - 1);
            }else if (testDirection == Direction.West)
            {
                door = new VectorBase(x, Rng.Instance.Range(y + 1, y + height - 1));
            } else if (testDirection == Direction.East)
            { 
                    door = new VectorBase(x + width - 1, Rng.Instance.Range(y + 1, y + height - 1));
            }
            SetTile(door, TileTypeFactory.Instance.Floor);

            return true;
        }

        /// Tries to randomly bring in the corners and round off the room.
        public bool decorateRoundedCorners(Rect room)
        {
            if (room.width <= 3 || room.height <= 3) return false;

            var modified = false;

            // Try the top-left corner.
            if (GetTile(room.topLeft + Direction.West).Type == TileTypeFactory.Instance.Wall &&
                GetTile(room.topLeft + Direction.North).Type == TileTypeFactory.Instance.Wall)
            {
                SetTile(room.topLeft, TileTypeFactory.Instance.Wall);
                modified = true;

                if (room.height > 5 &&
                    GetTile(room.topLeft + Direction.SouthWest).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(room.topLeft + Direction.South, TileTypeFactory.Instance.Wall);
                }

                if (room.width > 5 &&
                    GetTile(room.topLeft + Direction.NorthEast).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(room.topLeft + Direction.East, TileTypeFactory.Instance.Wall);
                }
            }

            // Try the top-right corner.
            var topRight = room.topRight + Direction.West;
            if (GetTile(topRight + Direction.East).Type == TileTypeFactory.Instance.Wall &&
                GetTile(topRight + Direction.North).Type == TileTypeFactory.Instance.Wall)
            {
                SetTile(topRight, TileTypeFactory.Instance.Wall);
                modified = true;

                if (room.height > 5 &&
                    GetTile(topRight + Direction.SouthEast).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(topRight + Direction.South, TileTypeFactory.Instance.Wall);
                }

                if (room.width > 5 &&
                    GetTile(topRight + Direction.NorthWest).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(topRight + Direction.West, TileTypeFactory.Instance.Wall);
                }
            }

            // Try the bottom-left corner.
            var bottomLeft = room.bottomLeft + Direction.North;
            if (GetTile(bottomLeft + Direction.West).Type == TileTypeFactory.Instance.Wall &&
                GetTile(bottomLeft + Direction.South).Type == TileTypeFactory.Instance.Wall)
            {
                SetTile(bottomLeft, TileTypeFactory.Instance.Wall);
                modified = true;

                if (room.height > 5 &&
                    GetTile(bottomLeft + Direction.NorthWest).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(bottomLeft + Direction.North, TileTypeFactory.Instance.Wall);
                }

                if (room.width > 5 &&
                    GetTile(bottomLeft + Direction.SouthEast).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(bottomLeft + Direction.East, TileTypeFactory.Instance.Wall);
                }
            }

            // Try the bottom-right corner.
            var bottomRight = room.bottomRight + Direction.NorthWest;
            if (GetTile(bottomRight + Direction.East).Type == TileTypeFactory.Instance.Wall &&
                GetTile(bottomRight + Direction.South).Type == TileTypeFactory.Instance.Wall)
            {
                SetTile(bottomRight, TileTypeFactory.Instance.Wall);
                modified = true;

                if (room.height > 5 &&
                    GetTile(bottomRight + Direction.SouthWest).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(bottomRight + Direction.South, TileTypeFactory.Instance.Wall);
                }

                if (room.width > 5 &&
                    GetTile(bottomRight + Direction.NorthEast).Type == TileTypeFactory.Instance.Wall)
                {
                    SetTile(bottomRight + Direction.East, TileTypeFactory.Instance.Wall);
                }
            }

            return modified;
        }
    }
}
