using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.AI;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public static class MoveFactory
    {
        public static Move heal(int rate, int amount) => new HealMove(rate, amount);

        public static Move arrow(int rate, int damage) => new BoltMove(rate, new RangedAttack("the arrow", "hits", damage, ElementFactory.Instance.None, 8));

        public static Move waterBolt(int rate, int damage) => new BoltMove(rate, new RangedAttack("the jet", "splashes", damage, ElementFactory.Instance.Water, 8));

        public static Move sparkBolt(int rate, int damage, int range = 8) => new BoltMove(rate, new RangedAttack("the spark", "zaps", damage, ElementFactory.Instance.Lightning, range));

        public static Move iceBolt(int rate, int damage, int range = 8) =>
            new BoltMove(rate, new RangedAttack("the ice", "freezes", damage, ElementFactory.Instance.Cold, range));

        public static Move fireBolt(int rate, int damage) =>
            new BoltMove(rate, new RangedAttack("the flame", "burns", damage, ElementFactory.Instance.Fire, 8));

        public static Move darkBolt(int rate, int damage) =>
            new BoltMove(rate, new RangedAttack("the darkness", "crushes", damage, ElementFactory.Instance.Dark, 10));

        public static Move lightBolt(int rate, int damage) =>
            new BoltMove(rate, new RangedAttack("the light", "sears", damage, ElementFactory.Instance.Light, 10));

        public static Move poisonBolt(int rate, int damage) =>
            new BoltMove(rate, new RangedAttack("the poison", "engulfs", damage, ElementFactory.Instance.Poison, 8));

        public static Move fireCone(int rate = 5, int damage = 12, int range = 10) =>
            new ConeMove(rate, new RangedAttack("the flame", "burns", damage, ElementFactory.Instance.Fire, range));

        public static Move lightningCone(int rate = 5, int damage = 12, int range = 10) =>
            new ConeMove(rate, new RangedAttack("the lightning", "shocks", damage, ElementFactory.Instance.Lightning, range));

        public static Move insult(int rate = 5) => new InsultMove(rate);
        public static Move howl(int rate = 10, int range = 10) => new HowlMove(rate, range);

        public static Move haste(int rate = 5, int duration = 10, int speed = 1) =>
            new HasteMove(rate, duration, speed);

        public static Move teleport(int rate = 5, int range = 10) =>
            new TeleportMove(rate, range);

        public static Move spawn(int rate) => new SpawnMove(rate);
    }
}
