using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public abstract class DungeonBuilder : StageBuilder
    {
        public List<Rect> Rooms { get; set; } = new List<Rect>();

        /// <summary>
        ///     Generate a room and maze dungeon, http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
        /// </summary>
        public override void Generate(Stage stage)
        {
            //check size
            if (stage.Width % 2 == 0 || stage.Height % 2 == 0)
            {
                throw new ApplicationException("The stage must be odd-sized.");
            }

            GenerateMaze(stage);
            GenerateMazeFeatures(stage);
            GenerateStairways(stage);
        }

        public virtual void GenerateMazeFeatures(Stage stage)
        {}

        private void GenerateStairways(Stage stage1)
        {
            VectorBase posUp = null;
            if (stage1.HasExitUp)
            {
                var roomUp = Rooms[Rng.Instance.Range(0, Rooms.Count)];

                while (posUp == null)
                {
                    posUp = roomUp.PointsInsideRect()[Rng.Instance.Range((roomUp.width - 2) * (roomUp.height -2))];
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
                var roomDown = Rooms[Rng.Instance.Range(0, Rooms.Count)];

                VectorBase posDown = null;
                while (posDown == null)
                {
                    posDown = roomDown.PointsInsideRect()[Rng.Instance.Range((roomDown.width - 2) * (roomDown.height - 2))];
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

            BeforeGeneration();

            AddRooms(); //randomly place rooms

            AddMaze();

            AfterGeneration();
            // Randomly switch some tiles around.
            //Erode(10000, floor: TileTypeFactory.Instance.Floor, wall: TileTypeFactory.Instance.Wall);

            CleanOutSideBoundary(TileTypeFactory.Instance.Wall);

            CreateSolidTiles(floor: TileTypeFactory.Instance.Floor, wall: TileTypeFactory.Instance.Wall);

            ReplaceGenericWallWithDirectionalWalls();
        }

        public virtual void BeforeGeneration() { }

        public virtual void AfterGeneration() { }

        public abstract void AddRooms();

        public abstract void AddMaze();

        public virtual void OnDecorateRoom(Rect room) { }

        public void AddRoomToMap(Rect room, int roomSizeWidth, int roomSizeHeight)
        {
            //add non-overlapping room
            Rooms.Add(room);

            for (var ix = room.left; ix < room.left + roomSizeWidth; ix++)
            {
                for (var iy = room.top; iy < room.top + roomSizeHeight; iy++)
                {
                    SetTile(new VectorBase(ix, iy), TileTypeFactory.Instance.Floor);
                }
            }
        }
        #endregion
    }
}