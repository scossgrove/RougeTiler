using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    public class ItemAffixJson
    {
        public string Name { get; set; }
        public Dictionary<string, object> AttackData { get; set; }
    }
}