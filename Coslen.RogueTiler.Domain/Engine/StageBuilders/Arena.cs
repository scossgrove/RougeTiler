using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public class Arena : StageBuilder
    {
        
        public override void Generate(Stage stage)
        {
            //Debugger.Info(" * {0} = [{1}]", "Generate", "Begin");

            var width = stage.Width;
            var height = stage.Height;

            //check size
            if (width % 2 == 0 || height % 2 == 0)
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
            if (stage1.HasExitUp)
            {
                var posUp = stage.FindOpenTile();
                SetTile(posUp, TileTypeFactory.Instance.StairsUp);
                stage1.StairUpPosition = posUp;
            }

            if (stage1.HasExitDown)
            {
                var positionDown = stage.FindOpenTile();
                SetTile(positionDown, TileTypeFactory.Instance.StairsDown);
                stage1.StairDownPosition = positionDown;
            }
        }

        #region "Maze Generation Functions"

        private void GenerateMaze(Stage stage)
        {
            BindStage(stage);

            Fill(TileTypeFactory.Instance.Wall);

            _addRooms(); 

            CreateSolidTiles(TileTypeFactory.Instance.Floor, TileTypeFactory.Instance.Wall);

            ReplaceGenericWallWithDirectionalWalls();
        }

        /// Places rooms ignoring the existing maze corridors.
        public virtual void _addRooms()
        {
            var width = stage.Width;
            var height = stage.Height;

            var centerX = width / 2;
            var centerY = height / 2;

            //var radius = (Math.Min(width, height) - 4)/2;
            //CarveCircle(new VectorBase(centerX, centerY), radius, TileTypeFactory.Instance.Floor);

            CarveEllipse(new VectorBase(centerX, centerY), centerX - 2, centerY - 2, TileTypeFactory.Instance.Floor);
        }
        
        #endregion
    }
}