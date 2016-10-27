using System;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame
{
    public class GameBoardScreenSection : ScreenSectionBase, IScreenSection
    {
        private Appearence[,] PreviousTiles;

        public GameBoardScreenSection(string key, int left, int top, int bottom, int right, int renderOrder) : base(key, left, top, bottom, right, renderOrder)
        {
        }

        public Appearence[,] Tiles { get; set; }
        public VectorBase HeroPosition { get; set; }

        // UI Console Setup
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public string Title { get; set; }

        public bool CanWrap { get; set; } = false;
        public string LinePrefix { get; set; }


        // This is the view port for the console (keep these as odd numbers so the hero can be in the center of the screen).
        public static Rect ViewPort = new Rect(0, 0, 71, 41);

        private Appearence[,] GetMatrixViewPort()
        {
            var matrixWidth = Tiles.GetUpperBound(0) + 1;
            var matrixHeight = Tiles.GetUpperBound(1) + 1;

            // This is to ensure that the view port is not bigger than the size of the tile matrix
            var adjustedViewPost = new Rect(0, 0, Math.Min(matrixWidth, ViewPort.width), Math.Min(matrixHeight, ViewPort.height));

            int halfViewPortWidth = (int)Math.Floor((double)adjustedViewPost.width / 2);
            int halfViewPortHeight = (int)Math.Floor((double)adjustedViewPost.height / 2);

            // Adjustments for the x axis
            int viewPortLeft = HeroPosition.x - halfViewPortWidth;
            if (viewPortLeft + adjustedViewPost.width > matrixWidth)
            {
                viewPortLeft = matrixWidth - adjustedViewPost.width;
            }
            if (viewPortLeft < 0)
            {
                viewPortLeft = 0;
            }

            // Adjuments for the y axis
            int viewPortTop = HeroPosition.y - halfViewPortHeight;
            if (viewPortTop + adjustedViewPost.height > matrixHeight)
            {
                viewPortTop = matrixHeight - adjustedViewPost.height;
            }
            if (viewPortTop < 0)
            {
                viewPortTop = 0;
            }

            var alteredViewPortMatrix = new Appearence[adjustedViewPost.width, adjustedViewPost.height];

            for (var rowIndex = 0; rowIndex < adjustedViewPost.height; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < adjustedViewPost.width; columnIndex++)
                {
                    int sourceColumnIndex = columnIndex + viewPortLeft;
                    int sourceRowIndex = rowIndex + viewPortTop;
                    // make sure the rulers are in the view port
                    if (rowIndex < 2)
                    {
                        sourceRowIndex = rowIndex;
                    }

                    if (columnIndex < 3)
                    {
                        sourceColumnIndex = columnIndex;
                    }

                    var currentTile = Tiles[sourceColumnIndex, sourceRowIndex];
                    alteredViewPortMatrix[columnIndex, rowIndex] = currentTile.Clone();
                }
            }

            return alteredViewPortMatrix;;
        }

        public override void Draw(BufferContainer buffer)
        {
            var debugger = Debugger.Instance;

            var startOfGetMatrixViewPort = DateTime.Now;
            var viewPortMatrix = GetMatrixViewPort();
            var endOfGetMatrixViewPort = DateTime.Now;

            var matrixWidth = viewPortMatrix.GetUpperBound(0) + 1;
            var matrixHeight = viewPortMatrix.GetUpperBound(1) + 1;

            // TODO: view port tranformation...
            // 1. the top two row for the ruler
            // 2. the left three rows for the ruler
            // 3. the rest of the matrix cut down.

            var startOfDrawingMatrixViewPort = DateTime.Now;

            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    var currentTile = viewPortMatrix[columnIndex, rowIndex];

                    if (Option.ShowAll)
                    {
                        var foreGroundColour = currentTile.ForeGroundColor == null ? ConsoleColor.White : ColourUtilities.ConvertToConsoleColor(currentTile.ForeGroundColor);
                        var backGroundColour = currentTile.BackGroundColor == null ? ConsoleColor.Black : ColourUtilities.ConvertToConsoleColor(currentTile.BackGroundColor);
                        buffer.Write(currentTile.Glyph, Left + columnIndex, Top + rowIndex, foreGroundColour, backGroundColour);
                    }
                    else
                    {

                        if (!currentTile.IsExplored)
                        {
                            buffer.Write($".", Left + columnIndex, Top + rowIndex, ConsoleColor.DarkRed);
                        }
                        else if (currentTile.IsInShadow)
                        {
                            buffer.Write(currentTile.Glyph, Left + columnIndex, Top + rowIndex, ConsoleColor.DarkGray);
                        }
                        else if (currentTile.IsHidden)
                        {
                            buffer.Write($".", Left + columnIndex, Top + rowIndex, ConsoleColor.DarkGreen);
                        }
                        else
                        {
                            var foreGroundColour = currentTile.ForeGroundColor == null ? ConsoleColor.White : ColourUtilities.ConvertToConsoleColor(currentTile.ForeGroundColor);
                            var backGroundColour = currentTile.BackGroundColor == null ? ConsoleColor.Black : ColourUtilities.ConvertToConsoleColor(currentTile.BackGroundColor);
                            buffer.Write(currentTile.Glyph, Left + columnIndex, Top + rowIndex, foreGroundColour, backGroundColour);
                        }
                    }

                }
            }

            // Stage Statistics
            var currentLine = 0;
            buffer.Write($"Number of Unexplored: {GameState.Instance.Game.CurrentStage.numberAlreadyExplored} [{GameState.Instance.Game.CurrentStage.percentageAlreadyExplored}%].", Left, Top + matrixHeight + currentLine++);
            buffer.Write($"Current Level: {GameState.Instance.Game.CurrentStage.StageNumber}.", Left, Top + matrixHeight + currentLine++);

            var endOfDrawingMatrixViewPort = DateTime.Now;


            var timeTakenToGettingMatrixViewPort = endOfGetMatrixViewPort - startOfGetMatrixViewPort;
            var timeTakenToDrawingMatrixViewPort = endOfDrawingMatrixViewPort - startOfDrawingMatrixViewPort;

            debugger.LogToDisk($"  $ Time for getting viewport: {timeTakenToGettingMatrixViewPort.TotalMilliseconds}", LogLevel.Debug);
            debugger.LogToDisk($"  $ Time for drawing: {timeTakenToDrawingMatrixViewPort.TotalMilliseconds}", LogLevel.Debug);
            
        }
    }
}