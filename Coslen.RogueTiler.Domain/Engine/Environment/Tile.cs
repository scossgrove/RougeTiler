using System;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine
{
    [Serializable]
    public class Tile
    {
        private bool visible;

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (value)
                {
                    IsExplored = true;
                }
                visible = value;
            }
        }

        public VectorBase Position { get; set; }

        public bool IsExplored { get; set; }
        public TileType Type { get; set; }

        public bool IsPassable
        {
            get { return Type.IsPassable; }
        }

        public bool IsTraversable
        {
            get { return Type.IsPassable || (Type.OpensTo != null); }
        }

        public bool IsTransparent
        {
            get { return Type.IsTransparent; }
        }

        public bool SetVisible(bool visible)
        {
            this.visible = visible;

            if (visible && !IsExplored)
            {
                IsExplored = true;
                return true;
            }

            return false;
        }
    }
}