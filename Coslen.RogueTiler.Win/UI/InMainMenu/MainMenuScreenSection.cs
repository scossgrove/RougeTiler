using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InMainMenu
{
    public class MainMenuScreenSection : ScreenSectionBase, IScreenSection
    {
        private readonly List<string> charColors = new List<string> {"LLLLLL   LLLLLL                          LLLLL                               LLLLL", "ERRRRE   ERRRRE                          ERRRE                               ERRRE", " ERRE     ERRE                            ERRE                                ERRE", " ERRELLLLLERRE    LLLLLLL   LLLLL  LLLLL  ERRE LLLLL      LLLLLLL  LLLL  LLLL ERRE   LLLL", " ERRREEEEERRRE    ERRRRRRL  ERRRE  ERRRE  ERREERRRRRL    LRRRRRRRL ERRRLLRRRRLERRE  LRRE", " ERRE     ERRE   LLLLL ERRE  ERRE   ERRE  ERRE    ERRL  ERRELLLERRE ERRE   EREERRELLRE", " EOOE     EOOE  LOOOOOEEOOE  EOOE   EOOE  EOOE     EOOE EOOEEOOOOOE EOOE      EOOOOOOOL", " EGGE     EGGE EGGELLLLEGGE  EGGLLLLEGGE  EGGELLLLLGGGE EGGELLLLLL  EGGE      EGGE  EGGLL", " EYYE     EYYE  EYYYYYYEEYYE  EYYYYYEYYYLLYYYEEYYYYYYE   EYYYYYYYE LYYYYL    LYYYYL  EYYYL", " EYYE     EYYE LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL", " EYYE     EYYE EYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYL", "LEYYE     EYYEL", "EYYYE     EYYYE", " EYYE     EYYE", "  EYE     EYE", "   EE     EE"};


        private readonly List<string> chars = new List<string> {@"______   ______                          _____                               _____", @"\ .  /   \  . /                          \ . |                               \  .|", @" | .|     |. |                            | .|                                |. |", @" |. |_____| .|    _______   _____  _____  |. | _____      _______  ____  ____ | .|   ____", @" |:::_____:::|    \::::::\  \:::|  \:::|  |::|/:::::\    /:::::::\ \:::|/::::\|::|  /::/", @" |xx|     |xx|   _____ \xx|  |xx|   |xx|  |xx|    \xx\  |xx|___)xx| |xx|   \x||xx|_/x/", @" |xx|     |xx|  /xxxxx\|xx|  |xx|   |xx|  |xx|     |xx| |xx|\xxxxx| |xx|      |xxxxxxx\", @" |XX|     |XX| |XX(____|XX|  |XX\___|XX|  |XX|____/XXX| |XX|______  |XX|      |XX|  \XX\_", @" |XX|     |XX|  \XXXXXX/\XX\  \XXXX/|XXX\/XXX/\XXXXXX/   \XXXXXXX/ /XXXX\    /XXXX\  \XXX\", @" |XX|     |XX| ____________________________________________________________________________", @" |XX|     |XX| |XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX\", @"_|XX|     |XX|_", @"\XXX|     |XXX/", @" \XX|     |XX/", @"  \X|     |X/", @"   \|     |/"};

        private readonly Dictionary<string, ConsoleColor> colors = new Dictionary<string, ConsoleColor>();

        public MainMenuScreenSection(Storage storage) : this("Main Menu", 0, 2, Console.WindowHeight - 2, Console.WindowWidth - 1, 1, storage)
        {
        }

        public MainMenuScreenSection(string key, int left, int top, int bottom, int right, int renderOrder, Storage storage) : base(key, left, top, bottom, right, renderOrder)
        {
            SelectedHeroIndex = -1;
            Storage = storage;
            Setup();
        }

        public Storage Storage { get; set; }
        public int SelectedHeroIndex { get; set; }
        public int NumberOfHeroes { get; set; }

        public override void Draw(BufferContainer buffer)
        {
            for (var y = 0; y < chars.Count; y++)
            {
                for (var x = 0; x < chars[y].Length; x++)
                {
                    var charColor = charColors[y][x];
                    var color = colors[charColor.ToString()];
                    buffer.Write(chars[y][x].ToString(), Left + x + 4, Top + y + 1, color);
                }
            }


            NumberOfHeroes = Storage.Heroes.Count;

            buffer.Write("Which hero shall you play?", Left + 0, Top + 18);
            buffer.Write("[L] Select a hero, [↕] Change selection, [N] Create a new hero, [D] Delete hero", Left + 0, Top + 18 + NumberOfHeroes + 3, ConsoleColor.Gray);

            if (NumberOfHeroes == 0)
            {
                buffer.Write("(No heroes. Please create a new one.)", Left + 0, Top + 20, ConsoleColor.Gray);
            }

            for (var i = 0; i < Storage.Heroes.Count; i++)
            {
                var hero = Storage.Heroes[i];

                var fore = ConsoleColor.White;
                var secondaryFore = ConsoleColor.Gray;
                var back = ConsoleColor.Black;

                if (i == SelectedHeroIndex)
                {
                    fore = ConsoleColor.Black;
                    secondaryFore = ConsoleColor.White;
                    back = ConsoleColor.Yellow;
                }

                buffer.Write(hero.Name, Left + 6, Top + 20 + i, fore, back);
                buffer.Write($"Level {hero.Level}", Left + 25, Top + 20 + i, secondaryFore);
                buffer.Write(hero.HeroClass.Name, Left + 35, Top + 20 + i, secondaryFore);
            }
        }

        private void Setup()
        {
            colors.Add(" ", ConsoleColor.Black);
            colors.Add("L", ConsoleColor.White);
            colors.Add("E", ConsoleColor.Gray);
            colors.Add("R", ConsoleColor.DarkRed);
            colors.Add("O", ConsoleColor.Red);
            colors.Add("G", ConsoleColor.DarkYellow);
            colors.Add("Y", ConsoleColor.Yellow);
        }
    }
}