using System;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Classes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public enum HeroStatisticTab
    {
        Primary =0 ,
        Secondard
    }

    public class HeroStatisicsDialog : DialogBase
    {
        public HeroStatisicsDialog(Game game) : this("ItemDialog", 20, 10, Console.WindowHeight - 10, Console.WindowWidth - 20, -1, game)
        {
        }

        public HeroStatisicsDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game) : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        /// The current location being shown to the player.
        private HeroStatisticTab statisticTab = HeroStatisticTab.Primary;
        public Game Game { get; }

        public override void Draw(BufferContainer buffer)
        {
            var currentLine = 0;
            switch (statisticTab)
            {
                case HeroStatisticTab.Primary:
                {
                        currentLine = DrawPrimaryStatistics(buffer, currentLine);
                        break;
                }
                case HeroStatisticTab.Secondard:
                    {

                        currentLine = DrawSecondaryStatistics(buffer, currentLine);
                        break;
                    }
            }
            

            // Options for Item Dialog
            String helpText = "[Tab] Switch view";
            WriteAt(buffer, 0, currentLine + 3, $"{helpText}", ConsoleColor.Gray);
        }

        private int DrawPrimaryStatistics(BufferContainer buffer, int currentLine)
        {
            var currentStageHero = Game.Hero;
            WriteAt(buffer, 0, currentLine++, "Hero Statistics: Primary", ConsoleColor.White, ConsoleColor.Black);
            currentLine++;

            WriteAt(buffer, 0, currentLine, "Name".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, Game.Hero.Name, ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Level".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, currentStageHero.Level.ToString(), ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Experience".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, currentStageHero.ExperienceCents.ToString(), ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Health".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, currentStageHero.Health.Current + "/" + currentStageHero.Health.Max,ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Charge".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, currentStageHero.Charge.Current + "/" + currentStageHero.Charge.Max, ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Armour".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, currentStageHero.Armor.ToString(), ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Gold".PadLeft(18, ' '));
            WriteAt(buffer, 20, currentLine++, currentStageHero.Gold.ToString(), ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Food".PadLeft(18, ' '));
            var tmpFood = Math.Floor(currentStageHero.Food);
            WriteAt(buffer, 20, currentLine++, tmpFood.ToString(), ConsoleColor.Yellow);

            return currentLine;
        }

        private int DrawSecondaryStatistics(BufferContainer buffer, int currentLine)
        {
            var currentStageHero = Game.Hero;
            WriteAt(buffer, 0, currentLine++, "Hero Statistics: Secondary", ConsoleColor.White, ConsoleColor.Black);
            currentLine++;

            var currentWarrior = currentStageHero.HeroClass as Warrior;
            // Combat, Toughness, Fighting
            WriteAt(buffer, 0, currentLine, "Toughness");
            WriteAt(buffer, 20, currentLine++, currentWarrior.Toughness.Level + " [" + currentWarrior.Toughness.percentUntilNext + "%]", ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Fighting");
            WriteAt(buffer, 20, currentLine++, currentWarrior.Fighting.Level + " [" + currentWarrior.Fighting.percentUntilNext + "%]", ConsoleColor.Yellow);

            WriteAt(buffer, 0, currentLine, "Combat");
            WriteAt(buffer, 20, currentLine++, currentWarrior.Combat.Level + " [" + currentWarrior.Combat.percentUntilNext + "%]", ConsoleColor.Yellow);

            currentLine++;
            WriteAt(buffer, 0, currentLine++, "Masteries:", ConsoleColor.White, ConsoleColor.Black);

            foreach (var mastery in currentWarrior.Masteries.OrderBy(x => x.Key))
            {
                WriteAt(buffer, 0, currentLine, mastery.Key);

                WriteAt(buffer, 20, currentLine++, mastery.Value.Level + " [" + mastery.Value.percentUntilNext + "%]", ConsoleColor.Yellow);

            }
            return currentLine;
        }

        protected override bool HandleInput()
        {
            var exitInputLoop = false;

            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    return true;
                }

                if (input == Inputs.tab)
                {
                    AdvanceLocation();
                    return false;
                }

            } while (exitInputLoop == false);

            return false;
        }

        private void AdvanceLocation()
        {
            var index = (int)statisticTab;
            var lastValue = (int)Enum.GetValues(typeof(HeroStatisticTab)).Cast<HeroStatisticTab>().Max();

            index = index + 1;
            if (index > lastValue)
            {
                index = 0;
            }
            statisticTab = (HeroStatisticTab) index;
        }
    }
}