using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance
{
    public class TileJson
    {
        public bool IsExplored { get; set; }
        public string TileType { get; set; }
        public bool Visible { get; set; }
        public VectorBase Position { get; set; }
    }

    public class ShadowJson
    {
        public bool InShadow { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActorJson
    {
        public string ActorType { get; set; }
        public VectorBase Position { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class StageJson
    {
        public TileJson[,] Tiles { get; set; }
        public ShadowJson[,] Shadows { get; set; }
        public bool[,] Explored { get; set; }
        public List<ItemDataJson> Items { get; set; }
        public List<ActorJson> Actors { get; set; }
        public VectorBase LastHeroPosition { get; set; }
        public VectorBase StairDownPosition { get; set; }
        public VectorBase StairUpPosition { get; set; }
        public double Abundance { get; set; }
    }
}