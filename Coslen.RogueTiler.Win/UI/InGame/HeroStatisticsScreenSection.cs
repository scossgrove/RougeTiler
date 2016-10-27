using System;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Win.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame
{
    internal class HeroStatisticsScreenSection : ScreenSectionBase, IScreenSection
    {
        public HeroStatisticsScreenSection(string key, int left, int top, int bottom, int right, int renderOrder, Game game) : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; set; }

        public override void Draw(BufferContainer buffer)
        {
            var hero = Game.Hero;

            var foreGroundColour = hero.Appearance.ForeGroundColor == null ? ConsoleColor.White : ColourUtilities.ConvertToConsoleColor(hero.Appearance.ForeGroundColor);
            var backGroundColour = hero.Appearance.BackGroundColor == null ? ConsoleColor.Black : ColourUtilities.ConvertToConsoleColor(hero.Appearance.BackGroundColor);

            var line = $"Lv: {Game.Hero.Level}[{Game.Hero.LevelPercentage}%] HP: {Game.Hero.Health.Current}/{Game.Hero.Health.Max} Charge:{Game.Hero.Charge.Current}/{Game.Hero.Charge.Max} ";
            WriteAt(buffer, 0, 0, line, foreGroundColour, backGroundColour);

            var secondLine = $"Armour: {Game.Hero.Armor} Weapon: {Game.Hero.GetAttack(null).BaseDamage} Food: {Math.Floor(Game.Hero.Food)}";
            WriteAt(buffer, 0, 1, secondLine, foreGroundColour, backGroundColour);
        }

        public new void WriteAt(BufferContainer buffer, int x, int y, string message, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black, bool ignorePadding = false)
        {
            base.WriteAt(buffer, x + Left, y + Top, message, foregroundColor, backgroundColor, ignorePadding);
        }
    }
}
