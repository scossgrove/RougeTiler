using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InMainMenu
{
    public class NewPlayerScreenSection : ScreenSectionBase, IScreenSection
    {
        // From: http://medieval.stormthecastle.com/medieval-names.htm.
        private readonly List<string> defaultNames = new List<string> {"Merek", "Carac", "Ulric", "Tybalt", "Borin", "Sadon", "Terrowin", "Rowan", "Forthwind", "Althalos", "Fendrel", "Brom", "Hadrian", "Crewe", "Bolbec", "Fenwick", "Mowbray", "Drake", "Bryce", "Leofrick", "Letholdus", "Lief", "Barda", "Rulf", "Robin", "Gavin", "Terrin", "Jarin", "Cedric", "Gavin", "Josef", "Janshai", "Doran", "Asher", "Quinn", "Xalvador", "Favian", "Destrian", "Dain", "Millicent", "Alys", "Ayleth", "Anastas", "Alianor", "Cedany", "Ellyn", "Helewys", "Malkyn", "Peronell", "Thea", "Gloriana", "Arabella", "Hildegard", "Brunhild", "Adelaide", "Beatrix", "Emeline", "Mirabelle", "Helena", "Guinevere", "Isolde", "Maerwynn", "Catrain", "Gussalen", "Enndolynn", "Krea", "Dimia", "Aleida"};

        public NewPlayerScreenSection(Storage storage) : this("New Player", 0, 2, Console.WindowHeight - 4, Console.WindowWidth - 1, 1, storage)
        {
        }

        public NewPlayerScreenSection(string key, int left, int top, int bottom, int right, int renderOrder, Storage storage) : base(key, left, top, bottom, right, renderOrder)
        {
            Storage = storage;
            NewRandomName();
        }

        public Storage Storage { get; set; }
        public string PlayerName { get; set; }
        public string DefaultName { get; set; }

        public override void Draw(BufferContainer buffer)
        {
            buffer.Write("What name shall the bards use to sing of", Left + 0, Top + 1);
            buffer.Write("your hero's adventures?", 0, Top + 2);

            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                buffer.Write(DefaultName, Left + 0, Top + 4, ConsoleColor.Black, ConsoleColor.Yellow);
                buffer.Write(string.Empty.PadRight(Right - Left - 2 - DefaultName.Length, ' '), Left + 0, Top + 4 + DefaultName.Length, ConsoleColor.White, ConsoleColor.Black);
            }
            else
            {
                buffer.Write(string.Empty.PadRight(Right - Left - 2, ' '), Left + 0, Top + 4);
                buffer.Write(PlayerName, Left + 0, Top + 4);
                buffer.Write(" ", Left + 0 + PlayerName.Length, Top + 4, ConsoleColor.Black, ConsoleColor.Yellow);
            }

            buffer.Write("[A-Z] Enter name, [Del] Delete letter", Left + 0, Top + 6, ConsoleColor.Gray);
            buffer.Write("[Tab] Generate Random Name", Left + 0, Top + 7, ConsoleColor.Gray);
            buffer.Write("[Enter] Create hero, [Esc] Cancel", Left + 0, Top + 8, ConsoleColor.Gray);
        }

        public void NewRandomName()
        {
            DefaultName = Rng.Instance.Item(defaultNames);
        }
    }
}