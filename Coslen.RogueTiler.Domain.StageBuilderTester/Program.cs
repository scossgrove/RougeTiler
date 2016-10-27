using System;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.StageBuilders;
using Coslen.RogueTiler.Domain.Utilities;

namespace Coslen.RogueTiler.Domain.StageBuilderTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var content = GameContent.Instance;

            var debugger = Coslen.RogueTiler.Domain.Utilities.Debugger.Instance;

            //TestTileTypes(debugger);
            TestStageBuilders(debugger);
            //TestPointIntersection(debugger);
            //TestLos(debugger);

            Console.WriteLine("Press enter key to exit....");
            Console.ReadKey();
        }

        private static void TestTileTypes(Debugger debugger)
        {
            foreach (var tileType in TileTypeFactory.Instance.TileTypes)
            {
                var message =
                    $"{tileType.Key} (P/T/W): {tileType.Value.IsPassable} / {tileType.Value.IsTransparent} / {tileType.Value.IsWall}";
                debugger.Info(message);
            }
        }

        private static void TestStageBuilders(Debugger debugger)
        {
            for (int index = 0; index < 1; index++)
            {
                debugger.Info("Creating Goblin Stronghold....");
                BuildStage(new GoblinStronghold());
                debugger.Info("Creating Training Ground....");
                BuildStage(new TrainingGrounds());
                debugger.Info("Creating Forrest....");
                BuildStage(new Forrest());
                debugger.Info("Creating Dungeon2FactionNoMaze....");
                BuildStage(new Dungeon2FactionNoMaze());
                debugger.Info("Creating Dungeon2FactionWithMaze....");
                BuildStage(new Dungeon2FactionWithMaze());
                debugger.Info("Creating Dungeon4FactionNoMaze....");
                BuildStage(new Dungeon4FactionNoMaze());
                debugger.Info("Creating Dungeon4FactionWithMaze....");
                BuildStage(new Dungeon4FactionWithMaze());
                debugger.Info("Creating Dungeon....");
                BuildStage(new Dungeon());
                debugger.Info("Creating Arena....");
                BuildStage(new Arena());
            }
        }

        private static void TestPointIntersection(Debugger debugger)
        {
            var rect = new Rect(3, 3, 3, 3);
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < 9; columnIndex++)
                {
                    var pointOfInquiry = new VectorBase(columnIndex, rowIndex);
                    var pointOfIntersect = rect.GetIntersect(pointOfInquiry);
                    debugger.Info(
                        $"origin: [{pointOfInquiry.x},{pointOfInquiry.y}], intersects: [{pointOfIntersect?.x},{pointOfIntersect?.y}]");
                }
            }
        }

        private static void TestLos(Debugger debugger)
        {
            // Line of Sight Testing
            var position = new VectorBase(67, 14);
            var target = new VectorBase(69, 16);
            var los = new Los(position, target);

            foreach (var point in los.Points)
            {
                debugger.Info($"point: [{point.x},{point.y}]");
            }
        }

        private static void BuildStage(StageBuilder dungeonBuilder)
        {
            var newStage = new Stage(81, 35);
            newStage.HasExitDown = true;
            newStage.HasExitUp = true;

            dungeonBuilder.Generate(newStage);
            var appearances = newStage.Appearances;
            var debugger = Coslen.RogueTiler.Domain.Utilities.Debugger.Instance;
            debugger.addStage(newStage);
        }
    }
}
