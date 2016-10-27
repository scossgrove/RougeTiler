using System;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.GameOver
{
    public class GameOverScreenSection : ScreenSectionBase, IScreenSection
    {
        public GameOverScreenSection(Storage storage) : this("Game Over", 0, 2, Console.WindowHeight - 2, Console.WindowWidth - 1, 1, storage)
        {
        }

        public GameOverScreenSection(string key, int left, int top, int bottom, int right, int renderOrder, Storage storage) : base(key, left, top, bottom, right, renderOrder)
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
            buffer.Write("Oh well, death comes to all of us!", Left + 0, Top + 1);
        }

        private void Setup()
        {
        }
    }
}