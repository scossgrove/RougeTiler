using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// This contains all of the tunable game engine parameters. Tweaking these can
    /// massively affect all aspects of gameplay.
    public class Option
    {
        public static bool ShowAll = false;

        public static int HeroSightRange = 6;

        /// The max health of a new hero.
        public static int HeroHealthStart = 40;

        /// The max charge of a new hero.
        public static int HeroChargeStart = 20;

        /// How much max health is increased when the hero levels up.
        public static int HeroHealthGain = 12;

        /// How much damage an unarmed hero does.
        public static int HeroPunchDamage = 5;

        /// The highest level the hero can reach.
        public static int HeroLevelMax = 50;

        /// How much each level costs. This is multiplied by the (zero-based) level
        /// (squared) to determine how much experience is required to reach that
        /// level.
        public static int HeroLevelCost = 40;

        /// The amount of gold a new hero starts with.
        public static int HeroGoldStart = 60;

        /// The maximum number of items the hero's [Inventory] can contain.
        public static int InventoryCapacity = 20;

        /// The maximum number of items the hero's home [Inventory] can contain.
        /// Note: To make this is more than 26, the home screen UI will need to be
        /// changed.
        public static int HomeCapacity = 20;

        /// The maximum number of items the hero's crucible can contain.
        public static int CrucibleCapacity = 8;

        /// When calculating pathfinding, how much it "costs" to move one step on
        /// an open floor tile.
        public static int AStarFloorCost = 10;

        /// When calculating pathfinding, how much it costs to move one step on a
        /// tile already occupied by an actor. For pathfinding, we consider occupied
        /// tiles as accessible but expensive. The idea is that by the time the
        /// pathfinding monster gets there, the occupier may have moved, so the tile
        /// is "sorta" empty, but still not as desirable as an actually empty tile.
        public static int AStarOccupiedCost = 60;

        /// When calculating pathfinding, how much it costs cross a currently-closed
        /// door. Instead of considering them completely impassable, we just have them
        /// be expensive, because it still may be beneficial for the monster to get
        /// closer to the door (for when the hero opens it later).
        public static int AStarDoorCost = 80;

        /// When applying the pathfinding heuristic, straight steps (NSEW) are
        /// considered a little cheaper than diagonal ones so that straighter paths
        /// are preferred over equivalent but uglier zig-zagging ones.
        public static int aStarStraightCost = 9;

        /// How much noise different kinds of actions make.
        public static int NoiseNormal = 10;

        public static int NoiseHit = 50;
        public static int NoiseRest = 1;

        /// The chance of trying to spawn a new monster in the unexplored dungeon
        /// each turn.
        public static int SpawnMonsterChance = 50;

        /// The maximum distance at which a monster will attempt a bolt attack.
        public static int MaxBoltDistance = 12;

        /// The experience point multipliers for each breed flag.
        public static Dictionary<string, double> ExpFlag = new Dictionary<string, double> {{"horde", 1.5}, {"swarm", 1.4}, {"pack", 1.3}, {"group", 1.2}, {"few", 1.1}, {"open-doors", 1.1}, {"fearless", 1.2}, {"protective", 1.1}, {"cowardly", 0.8}, {"berzerk", 1.2}, {"immobile", 0.7}};

        /// The experience point multipliers for an attack or move using a given
        /// element.
        public static Dictionary<Element, double> ExpElement = new Dictionary<Element, double> {{ ElementFactory.Instance.None, 1.0}, { ElementFactory.Instance.Air, 1.2}, { ElementFactory.Instance.Earth, 1.2}, { ElementFactory.Instance.Fire, 1.1}, { ElementFactory.Instance.Water, 1.3}, { ElementFactory.Instance.Acid, 1.4}, { ElementFactory.Instance.Cold, 1.2}, { ElementFactory.Instance.Lightning, 1.1}, { ElementFactory.Instance.Poison, 2.0}, { ElementFactory.Instance.Dark, 1.5}, { ElementFactory.Instance.Light, 1.5}, { ElementFactory.Instance.Spirit, 3.0}};

        /// The more a monster meanders, the less experience it's worth. This number
        /// should be larger than the largest meander value, and affects experience
        /// like so:
        /// 
        /// exp *= (expMeander - meander) / expMeander
        public static int ExpMeander = 30;
    }
}