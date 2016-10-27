using System;
using System.Diagnostics;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Win.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame
{
    internal class VisibleActorsScreenSection : ScreenSectionBase, IScreenSection
    {
        public VisibleActorsScreenSection(string key, int left, int top, int bottom, int right, int renderOrder, Game game) : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; set; }

        public override void Draw(BufferContainer buffer)
        {
            //ClearArea(buffer);

            var hero = Game.Hero;
            var actors = Game.CurrentStage.Actors;

            var foreGroundColour = hero.Appearance.ForeGroundColor == null ? ConsoleColor.White : ColourUtilities.ConvertToConsoleColor(hero.Appearance.ForeGroundColor);
            var backGroundColour = hero.Appearance.BackGroundColor == null ? ConsoleColor.Black : ColourUtilities.ConvertToConsoleColor(hero.Appearance.BackGroundColor);

            int lineCounter = 0;

            WriteAt(buffer, 0, lineCounter++, "Visible Creatures:", ConsoleColor.DarkYellow, backGroundColour);

            WriteAt(buffer, 0, lineCounter, hero.Appearance.Glyph, foreGroundColour, backGroundColour);
            WriteAt(buffer, 2, lineCounter, Game.Hero.Name);
            WriteAt(buffer, 22, lineCounter, $"({hero.Position.x},{hero.Position.y})");
            WriteAt(buffer, 32, lineCounter++, $"{hero.Health.Current}/{hero.Health.Max}");

            
            foreach (var actor in actors)
            {
                if (actor is Monster)
                {
                    var monster = actor as Monster;

                    if (Option.ShowAll)
                    {
                        foreGroundColour = monster.Appearance.ForeGroundColor == null
                            ? ConsoleColor.White
                            : ColourUtilities.ConvertToConsoleColor(monster.Appearance.ForeGroundColor);
                        backGroundColour = monster.Appearance.BackGroundColor == null
                            ? ConsoleColor.Black
                            : ColourUtilities.ConvertToConsoleColor(monster.Appearance.BackGroundColor);

                        WriteAt(buffer, 0, lineCounter, " ".PadRight(MaxWidth, ' '), ConsoleColor.White,
                            ConsoleColor.Black);

                        if (monster.Appearance.Glyph.Length > 1)
                        {
                            Debugger.Break();
                        }

                        WriteAt(buffer, 0, lineCounter, monster.Appearance.Glyph, foreGroundColour, backGroundColour);
                        WriteAt(buffer, 2, lineCounter, monster.NounText);
                        WriteAt(buffer, 22, lineCounter, $"({monster.Position.x},{monster.Position.y})");
                        WriteAt(buffer, 32, lineCounter, $"{monster.Health.Current}/{monster.Health.Max}");

                        lineCounter++;
                    }
                    else
                    {
                        if (!Game.CurrentStage.Shadows[monster.Position.x, monster.Position.y].IsInShadow)
                        {

                            foreGroundColour = monster.Appearance.ForeGroundColor == null
                                ? ConsoleColor.White
                                : ColourUtilities.ConvertToConsoleColor(monster.Appearance.ForeGroundColor);
                            backGroundColour = monster.Appearance.BackGroundColor == null
                                ? ConsoleColor.Black
                                : ColourUtilities.ConvertToConsoleColor(monster.Appearance.BackGroundColor);

                            WriteAt(buffer, 0, lineCounter, " ".PadRight(MaxWidth, ' '), ConsoleColor.White,
                                ConsoleColor.Black);

                            if (monster.Appearance.Glyph.Length > 1)
                            {
                                Debugger.Break();
                            }

                            WriteAt(buffer, 0, lineCounter, monster.Appearance.Glyph, foreGroundColour, backGroundColour);
                            WriteAt(buffer, 2, lineCounter, monster.NounText);
                            WriteAt(buffer, 22, lineCounter, $"({monster.Position.x},{monster.Position.y})");
                            WriteAt(buffer, 32, lineCounter, $"{monster.Health.Current}/{monster.Health.Max}");

                            lineCounter++;
                        }
                    }

                    
                }

            }
        }

        public new void WriteAt(BufferContainer buffer, int x, int y, string message, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black, bool ignorePadding = false)
        {
            base.WriteAt(buffer, x + Left, y + Top, message, foregroundColor, backgroundColor, ignorePadding);
        }
    }
}