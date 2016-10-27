using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    /// When the player is playing the game inside a dungeon, he is using a [Hero].
    /// When outside of the dungeon on the menu screens, though, only a subset of
    /// the hero's data persists (for example, there is no position when not in a
    /// dungeon). This class stores that state.
    public class HeroSave
    {
        public Equipment Equipment = new Equipment();

        public Inventory Inventory = new Inventory(Option.InventoryCapacity);
        ///// The index of the highest [Level] that the [Hero] has completed in each
        ///// [Area]. The key will be the [Area] name. The value will be the one-based
        ///// index of the level. No key means the hero has not completed any levels in
        ///// that area.
        //final Map<String, int> completedLevels;

        public HeroSave(string name, HeroClass heroClass)
        {
            Name = name;
            HeroClass = heroClass;
            //completedLevels = < String, int>{ };
        }

        public HeroSave(
                    string name,
                    HeroClass heroClass,
                    Inventory inventory,
                    Equipment equipment,
                    //this.home, this.crucible, 
                    int experienceCents,
                    //this.completedLevels, 
                    int gold)
        {
            this.Name = name;
            this.HeroClass = heroClass;
            this.Inventory = inventory;
            this.Equipment = equipment;
            this.ExperienceCents = experienceCents;
            this.Gold = gold;
        }

        public string Name { get; set; }

        public int Level
        {
            get { return LevelUtilties.CalculateLevel(ExperienceCents); }
        }

        public HeroClass HeroClass { get; set; }

        ///// Items in the hero's home.
        //public Inventory home = new Inventory(Option.homeCapacity);

        ///// Items in the hero's crucible.
        //public Inventory crucible = new Inventory(Option.crucibleCapacity);

        public int ExperienceCents { get; set; }

        /// How much gold the hero has.
        public int Gold { get; set; } // = Option.heroGoldStart;


        /// Copies data from [hero] into this object. This should be called when the
        /// [Hero] has successfully completed a [Stage] and his changes need to be
        /// "saved".
        public void CopyFrom(Hero hero)
        {
            HeroClass = hero.HeroClass;
            Inventory = hero.Inventory;
            Equipment = hero.Equipment;
            ExperienceCents = hero.ExperienceCents;
            Gold = hero.Gold;
        }
    }
}