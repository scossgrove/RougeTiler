using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public class TileTypeFactory
    {
        private static TileTypeFactory instance;

        private TileTypeFactory()
        {
            Initialise();
        }

        public TileType ClosedDoor => TileTypes["ClosedDoor"];

        public TileType ClosedDoorEW => TileTypes["ClosedDoorEW"];

        public TileType ClosedDoorNS => TileTypes["ClosedDoorNS"];

        public TileType Floor => TileTypes["Floor"];

        public TileType LowWall => TileTypes["LowWall"];

        public TileType Maze => TileTypes["Maze"];

        public TileType OpenDoor => TileTypes["OpenDoor"];

        public TileType OpenDoorEW => TileTypes["OpenDoorEW"];

        public TileType OpenDoorNS => TileTypes["OpenDoorNS"];

        public TileType Solid => TileTypes["Solid"];

        public TileType StairsDown => TileTypes["StairsDown"];

        public TileType StairsUp => TileTypes["StairsUp"];

        public TileType Table => TileTypes["Table"];

        public TileType Wall => TileTypes["Wall"];

        public TileType WallCornerNE => TileTypes["WallCornerNE"];

        public TileType WallCornerNW => TileTypes["WallCornerNW"];

        public TileType WallCornerSE => TileTypes["WallCornerSE"];

        public TileType WallCornerSW => TileTypes["WallCornerSW"];

        public TileType WallCross => TileTypes["WallCross"];

        public TileType WallEEnd => TileTypes["WallEEnd"];

        public TileType WallEW => TileTypes["WallEW"];

        public TileType WallNEnd => TileTypes["WallNEnd"];

        public TileType WallNS => TileTypes["WallNS"];

        public TileType WallSEnd => TileTypes["WallSEnd"];

        public TileType WallTE => TileTypes["WallTE"];

        public TileType WallTN => TileTypes["WallTN"];

        public TileType WallTS => TileTypes["WallTS"];

        public TileType WallTW => TileTypes["WallTW"];

        public TileType WallWEnd => TileTypes["WallWEnd"];

        public TileType WallColumn => TileTypes["WallColumn"];
        
        public TileType Tree => TileTypes["Tree"];

        public TileType Grass => TileTypes["Grass"];

        public Dictionary<string, TileType> TileTypes { get; set; } = new Dictionary<string, TileType>();

        public static TileTypeFactory Instance => instance ?? (instance = new TileTypeFactory());

        public void Initialise()
        {
            var ascciCoder = new ASCII();

            // Define the tile types.
            TileTypes.Add("Floor", new TileType("floor", "floor", true, true, " ", "."));
            TileTypes.Add("Tree", new TileType("tree", "tree", false, false, new Glyph("t", "green")));
            TileTypes.Add("Grass", new TileType("grass", "grass", true, true, " ", "g"));
            TileTypes.Add("Wall", new TileType("wall", "wall", false, false, null));
            TileTypes.Add("Table", new TileType("table", "table", false, false, new Glyph("t", "red")));
            TileTypes.Add("LowWall", new TileType("low wall", "wall", false, false, null));
            TileTypes.Add("Maze", new TileType("maze", "maze", false, false, null));
            TileTypes.Add("OpenDoor", new TileType("open door", "door", true, true, "o"));

            TileTypes.Add("ClosedDoor", new TileType("closed door", "door", false, false, "c"));
            OpenDoor.ClosesTo = ClosedDoor;
            ClosedDoor.OpensTo = OpenDoor;

            TileTypes.Add("OpenDoorEW", new TileType("open door ew", "door", true, true, "o", "o"));
            TileTypes.Add("ClosedDoorEW", new TileType("closed door ew", "door", false, false, "c", "c"));
            OpenDoorEW.ClosesTo = ClosedDoorEW;
            ClosedDoorEW.OpensTo = OpenDoorEW;

            TileTypes.Add("OpenDoorNS", new TileType("open door ns", "door", true, true, "o", "o"));
            TileTypes.Add("ClosedDoorNS", new TileType("closed door ns", "door", false, false, "c", "c"));
            OpenDoorNS.ClosesTo = ClosedDoorNS;
            ClosedDoorNS.OpensTo = OpenDoorNS;

            TileTypes.Add("StairsUp", new TileType("stairs up", "stairs", false, true, ">", ">", false, "LevelUp"));
            TileTypes.Add("StairsDown", new TileType("stairs down", "stairs", false, true, "<", "<", false, "LevelDown"));

            TileTypes.Add("Solid",
                new TileType("solid", "something hard", false, false, new Glyph(".", "Red", "Black"), "s"));

            TileTypes.Add("WallCornerNE",
                new TileType("WallCornerNE", "corner", false, false, null, ascciCoder[AsciiKeys.WallCornerNE], true));
            TileTypes.Add("WallCornerSE",
                new TileType("WallCornerSE", "corner", false, false, null, ascciCoder[AsciiKeys.WallCornerSE], true));
            TileTypes.Add("WallCornerSW",
                new TileType("WallCornerSW", "corner", false, false, null, ascciCoder[AsciiKeys.WallCornerSW], true));
            TileTypes.Add("WallCornerNW",
                new TileType("WallCornerNW", "corner", false, false, null, ascciCoder[AsciiKeys.WallCornerNW], true));
            TileTypes.Add("WallTN",
                new TileType("WallTN", "wall", false, false, null, ascciCoder[AsciiKeys.WallTS], true));
            TileTypes.Add("WallTE",
                new TileType("WallTE", "wall", false, false, null, ascciCoder[AsciiKeys.WallTW], true));
            TileTypes.Add("WallTS",
                new TileType("WallTS", "wall", false, false, null, ascciCoder[AsciiKeys.WallTN], true));
            TileTypes.Add("WallTW",
                new TileType("WallTW", "wall", false, false, null, ascciCoder[AsciiKeys.WallTE], true));
            TileTypes.Add("WallNS",
                new TileType("WallNS", "wall", false, false, null, ascciCoder[AsciiKeys.WallNS], true));
            TileTypes.Add("WallEW",
                new TileType("WallEW", "wall", false, false, null, ascciCoder[AsciiKeys.WallEW], true));
            TileTypes.Add("WallNEnd",
                new TileType("WallNEnd", "wall", false, false, null, ascciCoder[AsciiKeys.WallNS], true));
            TileTypes.Add("WallSEnd",
                new TileType("WallSEnd", "wall", false, false, null, ascciCoder[AsciiKeys.WallNS], true));
            TileTypes.Add("WallEEnd",
                new TileType("WallEEnd", "wall", false, false, null, ascciCoder[AsciiKeys.WallEW], true));
            TileTypes.Add("WallWEnd",
                new TileType("WallWEnd", "wall", false, false, null, ascciCoder[AsciiKeys.WallEW], true));
            TileTypes.Add("WallCross",
                new TileType("WallCross", "wall", false, false, null, ascciCoder[AsciiKeys.WallCross], true));
            TileTypes.Add("WallColumn",
                new TileType("WallColumn", "wall", false, false, null, ascciCoder[AsciiKeys.WallColumn], true));
        }

        internal TileType GetById(string type)
        {
            switch (type)
            {
                case "floor":
                {
                    return Floor;
                }
                case "tree":
                {
                    return Tree;
                }
                case "grass":
                {
                    return Grass;
                }
                case "wall":
                {
                    return Wall;
                }
                case "table":
                {
                    return Table;
                }
                case "lowwall":
                {
                    return LowWall;
                }
                case "maze":
                {
                    return Maze;
                }
                case "open door":
                {
                    return OpenDoor;
                }
                case "closed door":
                {
                    return ClosedDoor;
                }
                case "open door ew":
                {
                    return OpenDoorEW;
                }
                case "closed door ew":
                {
                    return ClosedDoorEW;
                }
                case "open door ns":
                {
                    return OpenDoorNS;
                }
                case "closed door ns":
                {
                    return ClosedDoorNS;
                }
                case "stairs up":
                {
                    return StairsUp;
                }
                case "stairs down":
                {
                    return StairsDown;
                }
                case "solid":
                {
                    return Solid;
                }
                case "WallCornerNE":
                {
                    return WallCornerNE;
                }
                case "WallCornerSE":
                {
                    return WallCornerSE;
                }
                case "WallCornerSW":
                {
                    return WallCornerSW;
                }
                case "WallCornerNW":
                {
                    return WallCornerNW;
                }
                case "WallTN":
                {
                    return WallTN;
                }
                case "WallTE":
                {
                    return WallTE;
                }
                case "WallTS":
                {
                    return WallTS;
                }
                case "WallTW":
                {
                    return WallTW;
                }
                case "WallNS":
                {
                    return WallNS;
                }
                case "WallEW":
                {
                    return WallEW;
                }
                case "WallNEnd":
                {
                    return WallNEnd;
                }
                case "WallSEnd":
                {
                    return WallSEnd;
                }
                case "WallEEnd":
                {
                    return WallEEnd;
                }
                case "WallWEnd":
                {
                    return WallWEnd;
                }
                case "WallCross":
                {
                    return WallCross;
                }
                case "WallColumn":
                {
                    return WallColumn;
                    }
                case "low wall":
                    {
                        return LowWall;
                    }
                default:
                {
                    throw new Exception("Invalid Tile Type:" + type);
                }
            }
        }
    }
}