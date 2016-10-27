using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance
{
    public class HeroSaveJson
    {
        public string Name { get; set; }

        public List<ItemDataJson> Inventory { get; set; }
        public List<ItemDataJson> Equipment { get; set; }

        public List<ItemDataJson> BackPack { get; set; }
        public List<ItemDataJson> Crucible { get; set; }

        public HeroClassJson HeroClass { get; set; }

        public List<StageJson> Stages { get; set; }

        public Dictionary<string, object> Properties { get; set; }
        
    }
}