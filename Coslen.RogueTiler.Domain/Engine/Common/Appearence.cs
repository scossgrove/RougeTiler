using System.Diagnostics;
using Newtonsoft.Json;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public enum AppearenceType
    {
        None = 0,
        Actor,
        Hero,
        Monster,
        Tile,
        Item
    }

    public class Appearence
    {
        public Appearence()
        {}

        public Appearence(string glyph, string backGroundColor, string foreGroundColor, bool isExplored, bool isHidden, bool isInShadow, int x, int y, string slug, AppearenceType type)
        {
            _glyph = glyph;
            BackGroundColor = backGroundColor;
            ForeGroundColor = foreGroundColor;
            IsExplored = isExplored;
            IsHidden = isHidden;
            IsInShadow = isInShadow;
            Position = new VectorBase(x, y);
            Slug = slug;
            Type = type;
        }

        private string _glyph;
        // should not be more that 1 character
        public string Glyph
        {
            get { return _glyph; }
            set
            {
                if (value.Length > 1)
                {
                    Debugger.Break();
                }
                _glyph = value;
            }
        }

        public string BackGroundColor { get; set; }
        public string ForeGroundColor { get; set; }
        public VectorBase Position { get; set; }


        public bool IsHidden { get; set; }

        public bool IsExplored { get; set; }

        public bool IsInShadow { get; set; }


        public string Slug { get; set; }
        public AppearenceType Type { get; set; }

        public Appearence Clone()
        {
            //// Clone using Serialisation
            //var json = JsonConvert.SerializeObject(this);
            //var clone = JsonConvert.DeserializeObject<Appearence>(json);

            // Manual Constructor
            var clone = new Appearence(
                    this.Glyph, 
                    this.BackGroundColor,
                    this.ForeGroundColor,
                    this.IsExplored,
                    this.IsHidden,
                    this.IsInShadow, 
                    this.Position == null ? 0 : Position.x,
                    this.Position == null ? 0 : Position.y,
                    this.Slug,
                    this.Type
                );

            return clone;
        }
    }
}