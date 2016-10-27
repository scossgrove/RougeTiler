using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Coslen.RogueTiler.Domain.UIConnector;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.Utilities;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    

    public delegate int ScoreDelegate<in T>(T item);
    
    public class TargetDialog : DialogBase
    {
        public new Actor DialogResult { get; private set; }
        public double Range { get; private set; }
        public TargetOnSelectDelegate OnSelect { get; }
        public Game Game { get; }
        public List<Monster> Monsters { get; set; } = new List<Monster>();
        public Monster Target { get; set; }

        public TargetDialog(double range, TargetOnSelectDelegate onSelect, Game game) : 
            this("TargetDialog", 0, 5, Console.WindowHeight - 4, Console.WindowWidth - 42, 1, range, onSelect, game)
        {}

        public TargetDialog(string key, int left, int top, int bottom, int right, int renderOrder, double range, TargetOnSelectDelegate onSelect, Game game) 
            : base(key, left, top, bottom, right, renderOrder)
        {
            this.NoFrame = true;
            Game = game;
            Range = range;
            OnSelect = onSelect;

            // Default to targeting the nearest monster.
            Actor nearest = null;
            foreach (var actor in game.CurrentStage.Actors)
            {
                if (!(actor is Monster)) continue;

                if (!game.CurrentStage[actor.Position].Visible) continue;

                // Must be within range.
                var hero = Game.Hero;
                var toMonster = actor.Position - hero.Position;
                if (toMonster > Range) continue;

                Monsters.Add(actor as Monster);

                if (nearest == null || hero.Position - actor.Position < hero.Position - nearest.Position)
                {
                    nearest = actor;
                }
            }

            if (nearest != null)
            {
                Target = nearest as Monster;
            }
        }


        public override void Draw(BufferContainer buffer)
        {
            ClearArea(buffer);
            
            var appearenaces = Game.CurrentStage.Appearances;
            var heroPosition = Game.CurrentStage.LastHeroPosition;
            DrawVisibleItems(buffer, appearenaces, heroPosition);
        }

        // This is the view port for the console (keep these as odd numbers so the hero can be in the center of the screen).
        public static Rect ViewPort = new Rect(0, 0, 71, 41);

        private Appearence[,] GetMatrixViewPort(Appearence[,] source, VectorBase heroPosition)
        {
            var matrixWidth = source.GetUpperBound(0) + 1;
            var matrixHeight = source.GetUpperBound(1) + 1;

            // This is to ensure that the view port is not bigger than the size of the tile matrix
            var adjustedViewPost = new Rect(0, 0, Math.Min(matrixWidth, ViewPort.width), Math.Min(matrixHeight, ViewPort.height));

            int halfViewPortWidth = (int)Math.Floor((double)adjustedViewPost.width / 2);
            int halfViewPortHeight = (int)Math.Floor((double)adjustedViewPost.height / 2);

            // Adjustments for the x axis
            int viewPortLeft = heroPosition.x - halfViewPortWidth;
            if (viewPortLeft + adjustedViewPost.width > matrixWidth)
            {
                viewPortLeft = matrixWidth - adjustedViewPost.width;
            }
            if (viewPortLeft < 0)
            {
                viewPortLeft = 0;
            }

            // Adjuments for the y axis
            int viewPortTop = heroPosition.y - halfViewPortHeight;
            if (viewPortTop + adjustedViewPost.height > matrixHeight)
            {
                viewPortTop = matrixHeight - adjustedViewPost.height;
            }
            if (viewPortTop < 0)
            {
                viewPortTop = 0;
            }

            var ascii = new ASCII();

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

                    var currentTile = source[sourceColumnIndex, sourceRowIndex].Clone();
                    alteredViewPortMatrix[columnIndex, rowIndex] = currentTile.Clone();

                    // Mark the Current Target
                    if (Target != null)
                    {
                        if (
                            (Target.Position.x - 1 + 3 == sourceColumnIndex && Target.Position.y + 2 == sourceRowIndex) ||
                            (Target.Position.x + 1 + 3 == sourceColumnIndex && Target.Position.y + 2 == sourceRowIndex)
                        )
                        {
                            alteredViewPortMatrix[columnIndex, rowIndex].ForeGroundColor = "LightGold";
                            alteredViewPortMatrix[columnIndex, rowIndex].Glyph = "-";
                        }
                        if (
                            (Target.Position.x + 3 == sourceColumnIndex && Target.Position.y - 1 + 2 == sourceRowIndex) ||
                            (Target.Position.x + 3 == sourceColumnIndex && Target.Position.y + 1 + 2 == sourceRowIndex)
                        )
                        {
                            alteredViewPortMatrix[columnIndex, rowIndex].Glyph = "|";
                            alteredViewPortMatrix[columnIndex, rowIndex].ForeGroundColor = "LightGold";
                        }

                        if (Game.CurrentStage.getHeroDistanceTo(currentTile.Position) > Range)
                        {
                            alteredViewPortMatrix[columnIndex, rowIndex].ForeGroundColor = "Red";
                        }
                    }
                }
            }

            return alteredViewPortMatrix; ;
        }

        public void DrawVisibleItems(BufferContainer buffer, Appearence[,] source, VectorBase heroPosition)
        {
            var viewPortMatrix = GetMatrixViewPort(source, heroPosition);
            
            var matrixWidth = viewPortMatrix.GetUpperBound(0) + 1;
            var matrixHeight = viewPortMatrix.GetUpperBound(1) + 1;

            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    var currentTile = viewPortMatrix[columnIndex, rowIndex].Clone();
                    
                    if (!currentTile.IsExplored || currentTile.IsInShadow || currentTile.IsHidden)
                    {
                        buffer.Write($" ", columnIndex, rowIndex, ConsoleColor.DarkRed);
                    }
                    else
                    {
                        var foreGroundColour = currentTile.ForeGroundColor == null ? ConsoleColor.White : ColourUtilities.ConvertToConsoleColor(currentTile.ForeGroundColor);
                        var backGroundColour = currentTile.BackGroundColor == null ? ConsoleColor.Black : ColourUtilities.ConvertToConsoleColor(currentTile.BackGroundColor);

                        buffer.Write(currentTile.Glyph, columnIndex, rowIndex, foreGroundColour, backGroundColour);
                    }
                }
            }
        }

        private int GetMonsterDistance(Monster source)
        {
            return (source.Position - Target.Position).lengthSquared();
        }

        
        private void ChangeTarget(Direction direction)
        {
            var ahead = new List<Monster>();
            var behind = new List<Monster>();

            var perp = direction.rotateLeft90();
            foreach (var monster in Monsters)
            {
                var relative = monster.Position - Target.Position;
                var dotProduct = perp.x * relative.y - perp.y * relative.x;
                if (dotProduct > 0)
                {
                    ahead.Add(monster);
                }
                else
                {
                    behind.Add(monster);
                }
            }

            var nearest = FindLowest<Monster>(ahead, GetMonsterDistance);
            if (nearest != null)
            {
                Target = nearest;
                return;
            }

            var farthest = FindHighest<Monster>(behind, GetMonsterDistance);
            if (farthest != null)
            {
                Target = farthest;
            }
        }

        /// Finds the item in [collection] whose score is lowest.
        ///
        /// The score for an item is determined by calling [callback] on it. Returns
        /// `null` if the [collection] is `null` or empty.
        public T FindLowest<T>(List<T> collection, ScoreDelegate<T> callback)
            where T: class 
        {
            if (collection == null) return null;

            T bestItem = null;
            var bestScore = int.MaxValue;

            foreach(T item in collection) {
                var score = callback(item);
                if (score < bestScore) {
                  bestItem = item;
                  bestScore = score;
                }
            }

          return bestItem;
        }

        /// Finds the item in [collection] whose score is highest.
        ///
        /// The score for an item is determined by calling [callback] on it. Returns
        /// `null` if the [collection] is `null` or empty.
        public T FindHighest<T>(List<T> collection, ScoreDelegate<T> callback)
            where T : class
        { 
            if (collection == null) return null;

            T bestItem = null;
            int bestScore = 0;

            foreach (T item in collection) {
                var score = callback(item);
                if (score > bestScore) {
                    bestItem = item;
                    bestScore = score;
                }
            }

            return bestItem;
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();
                
                if (input == Inputs.cancel)
                {
                    return true;
                }
                if (input == Inputs.ok)
                {
                    ProcessCommand();
                    return true;
                }
                if (input == Inputs.nw)
                {
                    ChangeTarget(Direction.NorthWest);
                    return false;
                }
                if (input == Inputs.n)
                {
                    ChangeTarget(Direction.North);
                    return false;
                }
                if (input == Inputs.ne)
                {
                    ChangeTarget(Direction.NorthEast);
                    return false;
                }
                if (input == Inputs.w)
                {
                    ChangeTarget(Direction.West);
                    return false;
                }
                if (input == Inputs.e)
                {
                    ChangeTarget(Direction.East);
                    return false;
                }
                if (input == Inputs.sw)
                {
                    ChangeTarget(Direction.SouthWest);
                    return false;
                }
                if (input == Inputs.s)
                {
                    ChangeTarget(Direction.South);
                    return false;
                }
                if (input == Inputs.se)
                {
                    ChangeTarget(Direction.SouthEast);
                    return false;
                }
            } while (true);
        }

        private void ProcessCommand()
        {
            if (Target != null)
            {
                // Let engine know the focus of the last targeted monster.
                Game.Target = Target;

                // Perform the action
                OnSelect(Target.Position);
            }
            else
            {
                OnSelect(Game.Hero.Position);
            }
        }
    }
}