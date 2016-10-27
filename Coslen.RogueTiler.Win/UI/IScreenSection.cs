using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win
{
    public interface IScreenSection
    {
        string Key { get; set; }
        int Left { get; set; }
        int Top { get; set; }
        int Bottom { get; set; }
        int Right { get; set; }
        int RenderOrder { get; set; }
        bool Enabled { get; set; }
        bool ForceRedraw { get; set; }

        void Draw(BufferContainer buffer);
    }
}