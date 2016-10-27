using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;
using Coslen.RogueTiler.Domain.Utilities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Entities
{
    [Serializable]
    public class Monster : Actor
    {
        /// The fear level that will cause the monster to become frightened.
        private readonly double _frightenThreshold;

        /// After performing a [Move] a monster must recharge to limit the rate that
        /// it can be performed. This tracks how much recharging is left to do for
        /// each move.
        /// 
        /// When a move is performed, its rate is added to this. It then reduces over
        /// time. When it reaches zero, the move can be performed again.
        private readonly Dictionary<Move, double> _recharges = new Dictionary<Move, double>();

        public readonly Guid Id;

        /// Whether the monster wanted to melee or do a ranged attack the last time
        /// it took a step.
        public bool wantsToMelee = true;

        public Monster(Game game, Breed breed, int x, int y, int maxHealth, int generation) : base(game, x, y, maxHealth, new Noun(breed.Name), breed.Pronoun)
        {
            Id = Guid.NewGuid();
            Breed = breed;
            Generation = generation;

            Debugger.Instance.addMonster(this);
            changeState(new AsleepState());

            // Give this some random variation within monsters of the same breed so
            // they don't all become frightened at the same time.
            _frightenThreshold = Rng.Instance.Range(60, 200);
            if (Breed.Flags.Contains("cowardly"))
            {
                _frightenThreshold *= 0.7;
            }

            // Initialize the recharges. These will be set to real values when the
            // monster wakes up.
            foreach (var move in breed.Moves)
            {
                _recharges[move] = 0.0;
            }
        }

        public Breed Breed { get; set; }

        /// The monster's generation.
        /// 
        /// Breeds created directly in the level are one. Breeds that are spawned
        /// or summoned by another monster have a generation one greater than that
        /// monster.
        /// 
        /// When a monster spawns another, its generation increases too so that it
        /// also spawns less frequently over time.
        public int Generation { get; set; }

        public MonsterState State { get; set; }

        public bool IsAfraid
        {
            get { return State is AfraidState; }
        }

        public bool IsAsleep
        {
            get { return State is AsleepState; }
        }

        /// How afraid of the hero the monster currently is. If it gets high enough,
        /// the monster will switch to the afraid state and try to flee.
        public double Fear { get; set; }

        public override Appearence Appearance
        {
            get { return Breed.Appearance; }
        }

        public new string NounText
        {
            get { return "the " + Breed.Name; }
        }

        public override Pronoun Pronoun
        {
            get { return Breed.Pronoun; }
        }

        /// How much experience a level one [Hero] gains for killing this monster.
        public int ExperienceCents
        {
            get { return Breed.ExperienceCents; }
        }

        public override bool CanOpenDoors
        {
            get { return Breed.Flags.Contains("open-doors"); }
        }

        public void UseMove(Move move)
        {
            // Add some randomness to the rate. Since monsters very eagerly prefer to
            // use moves, this ensures they don't use them too predictably.
            _recharges[move] += Rng.Instance.Range((int) move.Rate, (int) (move.Rate*2));
        }

        /// Returns `true` if [move] is recharged.
        public bool canUse(Move move)
        {
            return _recharges[move] == 0;
        }

        /// Gets whether or not this monster has a line of sight to [target].
        /// 
        /// Does not take into account if there are other [Actor]s between the monster
        /// and the target.
        public bool canView(VectorBase target)
        {
            // Walk to the target.
            foreach (VectorBase step in new Los(Position, target).Points)
            {
                if (step.Equals(target))
                {
                    return true;
                }
                if (!Game.CurrentStage[step].IsTransparent)
                {
                    return false;
                }
            }

            throw new ApplicationException("unreachable");
        }

        /// Gets whether or not this monster has a line of sight to [target].
        /// 
        /// Does take into account if there are other [Actor]s between the monster
        /// and the target.
        public bool canTarget(VectorBase target)
        {
            // Walk to the target.
            foreach (VectorBase step in new Los(Position, target))
            {
                if (step == target)
                {
                    return true;
                }
                if (Game.CurrentStage.ActorAt(step) != null)
                {
                    return false;
                }
                if (!Game.CurrentStage[step].IsTransparent)
                {
                    return false;
                }
            }

            throw new ApplicationException("unreachable");
        }

        public override int OnGetSpeed()
        {
            return Energy.NormalSpeed + Breed.Speed;
        }

        public override Action OnGetAction()
        {
            // Recharge moves.
            foreach (var move in Breed.Moves)
            {
                _recharges[move] = Math.Max(0.0, _recharges[move] - 1.0);
            }

            return State.GetAction();
        }

        /// Modifies fear and then determines if it's has crossed the threshold to
        /// cause a state change.
        private void _modifyFear(Action action, double offset)
        {
            // Don't add effects if the monster already died.
            if (!IsAlive)
            {
                return;
            }

            if (Breed.Flags.Contains("fearless"))
            {
                return;
            }

            // If it can't run, there's no point in being afraid.
            if (Breed.Flags.Contains("immobile"))
            {
                return;
            }

            Fear = Math.Max(0.0, Fear + offset);

            if (State is AwakeState && Fear > _frightenThreshold)
            {
                // Clamp the fear. This is mainly to ensure that a bunch of monsters
                // don't all get over their fear at the exact same time later. Since the
                // threshold is randomized, this will make the delay before growing
                // courageous random too.
                Fear = _frightenThreshold;

                Log("{1} is afraid!", this);
                changeState(new AfraidState());
                action.AddEvent(EventType.Fear, this);
                return;
            }

            if (State is AfraidState && Fear <= 0.0)
            {
                Log("{1} grows courageous!", this);
                changeState(new AwakeState());
                action.AddEvent(EventType.Courage, this);
            }
        }

        /// Changes the monster to its awake state if sleeping.
        public void wakeUp()
        {
            if (!(State is AsleepState))
            {
                return;
            }
            changeState(new AwakeState());
        }

        public void changeState(MonsterState state)
        {
            State = state;
            State.Bind(this);

            // TODO: Move this into state?
            if (state is AwakeState)
            {
                // Randomly charge the moves. This ensures the monster doesn't
                // immediately unload everything on the hero when first spotted.
                foreach (var move in Breed.Moves)
                {
                    _recharges[move] = Rng.Instance.Range((int) move.Rate);
                }
            }
        }

        public override Attack OnGetAttack(Actor defender)
        {
            return Rng.Instance.Item(Breed.Attacks);
        }

        public override Attack Defend(Attack attack)
        {
            State.Defend();

            // TODO: Breed-specific resists.
            return base.Defend(attack);
        }

        /// Inflicting damage decreases fear.
        public override void OnDamage(Action action, Actor defender, int damage)
        {
            // The greater the power of the hit, the more emboldening it is.
            var fear = 100.0*damage/Game.Hero.Health.Max;

            _modifyFear(action, -fear);
            Debugger.Instance.logMonster(this, "Hit for " + damage + " / " + Game.Hero.Health.Max + " decreases fear by " + fear + " to " + Fear);

            Func<Monster, bool> myWitness = delegate(Monster witness)
            {
                witness._viewHeroDamage(action, damage);
                return true;
            };

            // Nearby monsters may witness it.
            _updateWitnesses(myWitness);
        }

        /// This is called when another monster in sight of this one has damaged the
        /// hero.
        public void _viewHeroDamage(Action action, int damage)
        {
            var fear = 50.0*damage/Health.Max;

            _modifyFear(action, -fear);
            Debugger.Instance.logMonster(this, "Witness " + damage + $" / {Health.Max} decreases fear by {fear} to {Fear}");
        }

        /// Taking damage increases fear.
        public override void OnDamaged(Action action, Actor attacker, int damage)
        {
            // The greater the power of the hit, the more frightening it is.
            var fear = 100.0*damage/Health.Max;

            // Getting hurt enrages it.
            if (Breed.Flags.Contains("berzerk"))
            {
                fear *= -3.0;
            }

            _modifyFear(action, fear);

            Debugger.Instance.logMonster(this, "Hit for " + damage + $" / {Health.Max} increases fear by {fear} to {Fear}");

            Func<Monster, bool> myWitness = delegate(Monster witness)
            {
                witness._viewMonsterDamage(action, this, damage);
                return true;
            };

            // Nearby monsters may witness it.
            _updateWitnesses(myWitness);
        }

        /// This is called when another monster in sight of this one has taken
        /// damage.
        private void _viewMonsterDamage(Action action, Monster monster, int damage)
        {
            var fear = 50.0*damage/Health.Max;

            if (Breed.Flags.Contains("protective") && monster.Breed == Breed)
            {
                // Seeing its own kind get hurt enrages it.
                fear *= -2.0;
            }
            else if (Breed.Flags.Contains("berzerk"))
            {
                // Seeing any monster get hurt enrages it.
                fear *= -1.0;
            }

            _modifyFear(action, fear);
            Debugger.Instance.logMonster(this, "Witness " + damage + " / " + Health.Max + " increases fear by " + fear + " to " + Fear);
        }

        /// Called when this Actor has been killed by [attackNoun].
        public override void OnDied(Noun attackNoun)
        {
            // Try to keep dropped items from overlapping.
            var flow = new Flow(Game.CurrentStage, Position, null, false, true);

            Func<VectorBase, bool> predicateNearestWhere = delegate(VectorBase position)
            {
                if (Rng.Instance.OneIn(5))
                {
                    return true;
                }
                return Game.CurrentStage.ItemAt(position) == null;
            };

            // A "Delefate" with no return type
            Action<Item> myBreedDrop = delegate(Item item)
            {
                var itemPos = Position;
                if (Game.CurrentStage.ItemAt(Position) != null)
                {
                    itemPos = flow.NearestWhere(predicateNearestWhere);
                }

                item.Position = itemPos;
                Game.CurrentStage.Items.Add(item);
                Log("{1} drop[s] {2}.", this, item);
            };

            Breed.Drop.SpawnDrop(new AddItem(myBreedDrop));

            Game.CurrentStage.removeActor(this);
            Debugger.Instance.removeMonster(this);
        }

        public override void OnFinishTurn(Action action)
        {
            DecayFear(action);
        }

        public override void ChangePosition(VectorBase from, VectorBase to)
        {
            base.ChangePosition(from, to);

            if (Game != null && Game.CurrentStage != null)
            {
                // If the monster is (or was) visible, don't let the hero rest through it
                // moving.
                if ((from != null && Game.CurrentStage[from].Visible) || (to != null && Game.CurrentStage[to].Visible))
                {
                    Game.Hero.Disturb();
                }
            }
        }

        /// Invokes [callback] on all nearby monsters that can see this one.
        private void _updateWitnesses(Func<Monster, bool> callback)
        {
            foreach (var other in Game.CurrentStage.Actors)
            {
                if (other == this)
                {
                    continue;
                }

                if (!(other is Monster))
                {
                    continue;
                }

                var monster = other as Monster;

                if (monster.State is AsleepState)
                {
                    continue;
                }

                var distance = (monster.Position - Position).kingLength();

                if (distance > 20)
                {
                    continue;
                }

                if (monster.canView(Position))
                {
                    callback(monster);
                }
            }
        }

        /// Fear decays over time, more quickly the farther the monster is from the
        /// hero.
        private void DecayFear(Action action)
        {
            // TODO: Poison should slow the decay of fear.
            var fearDecay = 5.0 + (Position - Game.Hero.Position).kingLength();

            // Fear decays more quickly if out of sight.
            if (!IsVisible)
            {
                fearDecay = 5.0 + fearDecay*2.0;
            }

            // The closer the monster is to death, the less quickly it gets over fear.
            fearDecay = 2.0 + fearDecay*Health.Current/Health.Max;

            _modifyFear(action, -fearDecay);
            Debugger.Instance.logMonster(this, "Decay fear by " + fearDecay + " to " + Fear);
        }
    }
}