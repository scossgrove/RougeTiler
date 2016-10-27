using System;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine
{
    [Serializable]
    public class TileType
    {
        [NonSerialized] public readonly Action AlternateAction;

        public readonly string AlternateActionTemplate;
        public readonly Glyph Appearance;
        public readonly string DebugCharacter;
        public readonly bool IsPassable;
        public readonly bool IsTransparent;
        public readonly bool IsWall;
        public readonly string Name;
        public readonly string DisplayName;

        public TileType(string name, string displayName, bool isPassable, bool isTransparent, object appearance, string debugCharacter = "", bool isWall = false, string alternateAction = null)
        {
            Name = name;
            this.DisplayName = displayName;
            IsPassable = isPassable;
            IsTransparent = isTransparent;

            if (appearance is Glyph)
            {
                Appearance = appearance as Glyph;
            }

            if (appearance is string)
            {
                Appearance = new Glyph(appearance as string);
            }

            if (appearance == null)
            {
                if (!string.IsNullOrEmpty(DebugCharacter))
                {
                    Appearance = new Glyph(DebugCharacter);
                }
                else
                {
                    Appearance = new Glyph("?");
                }
                
            }

            
            IsWall = isWall;
            if (debugCharacter == "")
            {
                DebugCharacter = name[0].ToString();
            }
            else
            {
                DebugCharacter = debugCharacter;
            }

            AlternateActionTemplate = alternateAction;
            if (alternateAction != null)
            {
                switch (alternateAction)
                {
                    case "LevelUp":
                    {
                        AlternateAction = new LevelUpAction();
                        break;
                    }
                    case "LevelDown":
                    {
                        AlternateAction = new LevelDownAction();
                        break;
                    }
                }
            }
        }

        public TileType OpensTo { get; set; }

        public TileType ClosesTo { get; set; }
    }
}