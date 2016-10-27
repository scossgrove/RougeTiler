using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI
{
    public abstract class ScreenLayoutBase
    {
        public ScreenLayoutBase()
        {
            InsertStandardSections();
        }

        public Dictionary<string, IScreenSection> LayoutSections { get; } = new Dictionary<string, IScreenSection>();

        public abstract void Initialise();

        public void Draw(BufferContainer buffer)
        {
            foreach (var segment in LayoutSections.Select(x => x.Value).Where(x => x.Enabled).OrderBy(x => x.RenderOrder))
            {
                segment.Draw(buffer);
            }
            var bufferRender = new BufferRender();
            bufferRender.Render(buffer);
        }

        private void InsertStandardSections()
        {
            var header = new TextListScreenSection("Header", 0, 0, 2, Console.WindowWidth, 2);
            header.RenderOrder = 2;
            header.ForegroundColor = ConsoleColor.Yellow;
            header.Lines.Add("You're playing Rogue Tiler... live and let die :)");
            header.Padding_Top = 0;
            header.Padding_Bottom = 0;
            LayoutSections.Add("Header", header);

            var footer = new TextListScreenSection("Footer", 0, Console.WindowHeight - 3, Console.WindowHeight - 2, Console.WindowWidth - 1, 3);
            footer.RenderOrder = 3;
            footer.ForegroundColor = ConsoleColor.Gray;
            footer.Lines.Add("");
            footer.Lines.Add("Rogue Tiler v0.1");
            footer.Enabled = true;
            footer.Padding_Top = 0;
            footer.Padding_Bottom = 0;
            LayoutSections.Add("Footer", footer);
        }
    }
}