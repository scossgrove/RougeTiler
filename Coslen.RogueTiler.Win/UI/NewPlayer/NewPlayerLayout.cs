using System;
using Coslen.RogueTiler.Domain.Utilities;

namespace Coslen.RogueTiler.Win.UI.InMainMenu
{
    public class NewPlayerLayout : ScreenLayoutBase
    {
        public NewPlayerLayout(Storage storage)
        {
            Storage = storage;
            Initialise();
        }

        public Storage Storage { get; set; }

        public string PlayerName
        {
            get
            {
                var mainSection = LayoutSections["MainSection"] as NewPlayerScreenSection;
                return mainSection.PlayerName;
            }
            set
            {
                var mainSection = LayoutSections["MainSection"] as NewPlayerScreenSection;
                mainSection.PlayerName = value;
            }
        }

        public string DefaultName
        {
            get
            {
                var mainSection = LayoutSections["MainSection"] as NewPlayerScreenSection;
                return mainSection.DefaultName;
            }
        }

        public override void Initialise()
        {
            var section = new NewPlayerScreenSection("MainSection", 20, 4, Console.WindowHeight - 4, Console.WindowWidth - 42, 1, Storage);
            section.Enabled = true;
            if (LayoutSections.ContainsKey("MainSection"))
            {
                LayoutSections["MainSection"] = section;
            }
            else
            {
                LayoutSections.Add("MainSection", section);
            }
        }

        internal void RandomName()
        {
            var mainSection = LayoutSections["MainSection"] as NewPlayerScreenSection;
            mainSection.NewRandomName();
        }
    }
}