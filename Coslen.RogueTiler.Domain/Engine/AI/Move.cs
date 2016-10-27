using System;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Utilities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.AI
{
    /// A [Move] is an action that a [Monster] can perform aside from the basic
    /// walking and melee attack actions. Moves include things like spells, breaths,
    /// and missiles.
    public abstract class Move
    {
        public Move(double rate)
        {
            Rate = rate;
        }

        /// The frequency at which the monster can perform this move (with some
        /// randomness added in).
        /// 
        /// A rate of 1 means the monster can perform the move roughly every turn.
        /// A rate of 10 means it can perform it about one in ten turns. Fractional
        /// rates are allowed.
        public double Rate { get; set; }

        /// The range of this move if it's a ranged one, or `0` otherwise.
        public virtual int Range { get; set; }

        /// The Experience gained by killing a [Monster] with this move.
        /// 
        /// This should take the power of the move into account, but not its rate.
        public virtual double Experience { get; set; }

        /// Returns `true` if the monster would reasonably perform this move right
        /// now.
        public virtual bool ShouldUse(Monster monster)
        {
            return true;
        }

        /// Called when the [Monster] has selected this move. Returns an [Action] that
        /// performs the move.
        public virtual Action GetAction(Monster monster)
        {
            monster.UseMove(this);
            return OnGetAction(monster);
        }

        /// Create the [Action] to perform this move.
        public virtual Action OnGetAction(Monster monster)
        {
            return null;
        }
    }

    public class BoltMove : Move
    {
        public BoltMove(double rate, RangedAttack attack) : base(rate)
        {
            this.attack = attack;
        }

        public RangedAttack attack { get; set; }

        public override int Range
        {
            get { return attack.Range; }
        }

        public override double Experience
        {
            get { return attack.AverageDamage*Option.ExpElement[attack.Element]*(1.0 + Range/20); }
        }

        public override bool ShouldUse(Monster monster)
        {
            var target = monster.Game.Hero.Position;

            // Don't fire if out of range.
            var toTarget = target - monster.Position;
            if (toTarget > Range)
            {
                Debugger.Instance.logMonster(monster, "Bolt move too far.");
                return false;
            }
            if (toTarget < 1.5)
            {
                Debugger.Instance.logMonster(monster, "Bolt move too close.");
                return false;
            }

            // Don't fire a bolt if it's obstructed.
            if (!monster.canTarget(target))
            {
                Debugger.Instance.logMonster(monster, "Bolt move can't target.");
                return false;
            }

            Debugger.Instance.logMonster(monster, "Bolt move OK.");
            return true;
        }

        public override Action OnGetAction(Monster monster)
        {
            return new BoltAction(monster.Game.Hero.Position, attack);
        }

        public override string ToString()
        {
            return "Bolt " + attack + " rate: " + Rate;
        }
    }

    public class ConeMove : Move
    {
        private readonly RangedAttack attack;

        public ConeMove(double rate, RangedAttack attack) : base(rate)
        {
            this.attack = attack;
        }

        public override double Experience
        {
            get { return attack.AverageDamage*3.0*Option.ExpElement[attack.Element]*(1.0 + Range/10); }
        }

        public override bool ShouldUse(Monster monster)
        {
            var target = monster.Game.Hero.Position;

            // Don't fire if out of range.
            var toTarget = target - monster.Position;
            if (toTarget > Range)
            {
                Debugger.Instance.logMonster(monster, "Cone move too far.");
                return false;
            }

            // TODO: Should minimize friendly fire.
            if (!monster.canView(target))
            {
                Debugger.Instance.logMonster(monster, "Cone move can't target.");
                return false;
            }

            Debugger.Instance.logMonster(monster, "Cone move OK.");
            return true;
        }

        public override Action OnGetAction(Monster monster)
        {
            return RayAction.Cone(monster.Position, monster.Game.Hero.Position, attack);
        }

        public override string ToString()
        {
            return "Cone " + attack + " rate: " + Rate;
        }
    }

    public class HealMove : Move
    {
        public HealMove(double rate, int amount) : base(rate)
        {
            _amount = amount;
        }

        /// How much health to restore.
        public int _amount { get; set; }

        public override double Experience
        {
            get { return _amount; }
        }

        public override bool ShouldUse(Monster monster)
        {
            // Heal if it could heal the full amount, or it's getting close to death.
            return (monster.Health.Current/monster.Health.Max < 0.25) || (monster.Health.Max - monster.Health.Current >= _amount);
        }

        public override Action OnGetAction(Monster monster)
        {
            return new HealAction(_amount);
        }

        public override string ToString()
        {
            return "Heal " + _amount + " rate: " + Rate;
        }
    }

    public class InsultMove : Move
    {
        public InsultMove(double rate) : base(rate)
        {
        }

        public override double Experience
        {
            get { return 0.0f; }
        }

        public override bool ShouldUse(Monster monster)
        {
            var target = monster.Game.Hero.Position;
            var distance = (target - monster.Position).kingLength();

            // Don't insult when in melee distance.
            if (distance <= 1)
            {
                return false;
            }

            // Don't insult someone it can't see.
            return monster.canView(target);
        }

        public override Action OnGetAction(Monster monster)
        {
            return new InsultAction(monster.Game.Hero);
        }

        public override string ToString()
        {
            return "Insult rate: " + Rate;
        }
    }

    public class HasteMove : Move
    {
        public HasteMove(double rate, int duration, int speed) : base(rate)
        {
            _duration = duration;
            _speed = speed;
        }

        public int _duration { get; set; }
        public int _speed { get; set; }

        public override double Experience
        {
            get { return _duration*_speed; }
        }

        public override bool ShouldUse(Monster monster)
        {
            // Don't use if already hasted.
            return !monster.Haste.IsActive;
        }

        public override Action OnGetAction(Monster monster)
        {
            return new HasteAction(_duration, _speed);
        }

        public override string ToString()
        {
            return "Haste " + _speed + " for " + _duration + " turns rate: " + Rate;
        }
    }

    /// Teleports the [Monster] randomly from its current position.
    public class TeleportMove : Move
    {
        public TeleportMove(double cost, int range) : base(cost)
        {
            _range = range;
        }

        public int _range { get; set; }

        public override double Experience
        {
            get { return _range*0.7; }
        }

        public override bool ShouldUse(Monster monster)
        {
            if (monster.IsAfraid)
            {
                return true;
            }

            var target = monster.Game.Hero.Position;
            var distance = (target - monster.Position).kingLength();

            // If we're next to the hero and want to start there, don't teleport away.
            if (monster.wantsToMelee && distance <= 1)
            {
                return false;
            }

            return true;
        }

        public override Action OnGetAction(Monster monster)
        {
            return new TeleportAction(_range);
        }
    }

    /// Spawns a new [Monster] of the same [Breed] adjacent to this one.
    public class SpawnMove : Move
    {
        public SpawnMove(double rate) : base(rate)
        {
        }

        public override double Experience
        {
            get { return 6.0f; }
        }

        public override bool ShouldUse(Monster monster)
        {
            // Don't breed offscreen since it can end up filling the room before the
            // hero gets there.
            if (!monster.IsVisible)
            {
                return false;
            }

            // Look for an open adjacent tile.
            foreach (var dir in Direction.All)
            {
                var here = monster.Position + dir;
                if (monster.Game.CurrentStage[here].IsPassable && monster.Game.CurrentStage.ActorAt(here) == null)
                {
                    return true;
                }
            }

            return false;
        }

        public override Action OnGetAction(Monster monster)
        {
            //// Pick an open adjacent tile.
            //var dirs = Direction.All.Where((dir)) {
            //    var here = monster.pos + dir;
            //    return monster.Game.CurrentStage[here].isPassable &&
            //        monster.Game.CurrentStage.actorAt(here) == null;
            //}).toList();

            var pos = monster.Game.CurrentStage.FindDistantOpenTileNear(monster.Position);

            return new SpawnAction(pos, monster.Breed);
        }

        public override string ToString()
        {
            return "Spawn";
        }
    }

    public class HowlMove : Move
    {
        public HowlMove(double rate, int range) : base(rate)
        {
            _range = range;
        }

        public int _range { get; set; }

        public override double Experience
        {
            get { return _range*0.5; }
        }

        public override bool ShouldUse(Monster monster)
        {
            // TODO: Is using flow here too slow?
            var flow = new Flow(monster.Game.CurrentStage, monster.Position, _range, false, true);

            // See if there are any sleeping monsters nearby.
            var detectCircle = new Circle(monster.Position, _range);
            foreach (VectorBase pos in detectCircle.Points)
            {
                if (!monster.Game.CurrentStage.Bounds().Contains(pos))
                {
                    continue;
                }
                if (flow.GetDistance(pos) == null)
                {
                    continue;
                }

                var actor = monster.Game.CurrentStage.ActorAt(pos);

                // If we found someone asleep randomly consider howling.
                if (actor is Monster && ((Monster) actor).IsAsleep)
                {
                    return Rng.Instance.OneIn(2);
                }
            }

            return false;
        }

        public override Action OnGetAction(Monster monster)
        {
            return new HowlAction(_range);
        }

        public override string ToString()
        {
            return "Howl " + _range;
        }
    }
}