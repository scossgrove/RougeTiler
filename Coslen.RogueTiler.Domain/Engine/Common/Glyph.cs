using System;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public class Glyph
    {
        public string Appearance { get; set; }
        public string Fore { get; set; }
        public string Back { get; set; }

        public Glyph(string appearance, string fore = "White", string back = "Black")
        {
            if (appearance.Length > 1)
            {
                throw new ApplicationException($"Invalid Glyph appearance. attempt to set as [{appearance}].");
            }
            Appearance = appearance;
            Fore = fore;
            Back = back;
        }

        public static implicit operator Glyph(string source)
        {
            return new Glyph(source);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
