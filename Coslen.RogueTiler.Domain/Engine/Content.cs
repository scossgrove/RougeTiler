using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;
using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// Defines the actual content for the game: the breeds, items, etc. that
    /// define the play experience.
    public abstract class Content
    {
        public List<Area> Areas { get; set; }
        public Dictionary<string, Breed> Breeds { get; set; }
        public Dictionary<string, ItemType> Items { get; set; }

        //public List<Recipe> get recipes;
        //public List<Shop> get shops;

        public HeroSave CreateHero(string name, HeroClass heroClass)
        {
            return null;
        }
    }
}