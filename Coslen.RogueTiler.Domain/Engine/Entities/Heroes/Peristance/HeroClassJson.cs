using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    public class HeroClassJson
    {
        public string Name { get; set; }
        public List<TrainedStatJson> Properties { get; set; }
        public List<TrainedStatJson> Masteries { get; set; }
    }
}