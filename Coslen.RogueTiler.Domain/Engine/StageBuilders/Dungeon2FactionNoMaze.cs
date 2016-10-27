using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public class Dungeon2FactionNoMaze : DungeonNoMaze
    {
        public override void AddRooms()
        {
            var roomSizeWidth = stage.Width / 3;
            var roomSizeHeight = stage.Height / 3;

            var room = new Rect(1, 1, roomSizeWidth, roomSizeHeight);
            AddRoomToMap(room, roomSizeWidth, roomSizeHeight);

            room = new Rect(stage.Width - roomSizeWidth - 1, stage.Height - roomSizeHeight - 1, roomSizeWidth, roomSizeHeight);
            AddRoomToMap(room, roomSizeWidth, roomSizeHeight);
        }
    }
}