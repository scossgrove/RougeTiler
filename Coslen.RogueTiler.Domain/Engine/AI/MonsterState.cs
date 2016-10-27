using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Logging;
using Coslen.RogueTiler.Domain.Utilities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.AI
{
    /// This defines the monster AI. AI is broken into a three level hierarchy.
    /// 
    /// The top sort-of level is the monster's "mood". This is a set of variables
    /// that describe how the monster is "feeling". How afraid they are, how bored
    /// they are, etc. These are the monster's senses and memory.
    /// 
    /// At the beginning of each turn, the monster uses these and some hysteresis
    /// to determine it's *state*, which is how their mood manifests in behavior.
    /// Where the "fear" mood fluctuates every turn, only when it reaches a high
    /// enough point to trigger a transition to the "afraid" *state* does its
    /// behavior change.
    /// 
    /// Most monsters the hero is interacting with are in the "awake" state.
    /// Breeds off in the distances are usually asleep. Other states may be added
    /// later: confused, blind, charmed, etc.
    /// 
    /// When awake, the monster has to decide what to do. It has a few options:
    /// 
    /// - It can perform a [Move], which are the "special" things different breeds
    /// can do: teleportation, bolt attacks, etc.
    /// - It can try to walk up to the hero and engage in melee combat.
    /// - If it has a ranged attack move, it can try to get to a good vantage point
    /// (not near the hero but still in range, with open LOS) and use a ranged
    /// move.
    /// 
    /// Each move carries with it a little bit of logic to determine if it's a good
    /// idea to use it. For example, the [HealMove] won't let itself be used if the
    /// monster is at max health. In order to use a move, the monster must be
    /// "recharged". Each move has a cost, and after using it, the monster must
    /// recharge before another move can be performed. (Melee attacks have no cost.)
    /// 
    /// If a monster is recharged and does have a usable move, it will always prefer
    /// to do that first. Once it's got no moves to do, it has to determine how it
    /// wants to fight.
    /// 
    /// To choose between melee and ranged attacks, it decides how "cautious" it is.
    /// The more damaging its ranged attacks are relative to melee, the more
    /// cautious it is. Greater fear and lower health also make it more cautious.
    /// If caution is above a threshold, the monster will prefer a ranged attack.
    /// 
    /// To get in position for that, it pathfinds to the nearest tile that's in
    /// range and has an open line of sight to the hero. Checking for an open line
    /// of sight obviously avoids friendly fire, but also makes monsters spread out
    /// and flank the hero, which plays and looks cool.
    /// 
    /// Once it's on a good targeting tile, it will keep walking towards adjacent
    /// tiles that are farther from the hero but still in range until it's fully
    /// charged.
    /// 
    /// If it decides to go melee, it simply pathfinds to the hero and goes for it.
    /// In either case, the end result is walking one tile (or possibly standing
    /// in place.)
    public abstract class MonsterState
    {
        public Monster Monster { get; set; }

        public Breed Breed
        {
            get { return Monster.Breed; }
        }

        public Game Game
        {
            get { return Monster.Game; }
        }

        public VectorBase Position
        {
            get { return Monster.Position; }
        }

        public bool isVisible
        {
            get { return Monster.IsVisible; }
        }

        public bool CanOpenDoors
        {
            get { return Monster.CanOpenDoors; }
        }

        public void Bind(Monster monster)
        {
            Monster = monster;
        }

        public void Log(string message, params Noun[] nouns)
        {
            Monster.Log(message, nouns);
        }

        public virtual void Defend()
        {
        }

        public virtual Action GetAction()
        {
            return null;
        }

        public void ChangeState(MonsterState state)
        {
            Monster.changeState(state);
        }

        public Action GetNextStateAction(MonsterState state)
        {
            Monster.changeState(state);
            return state.GetAction();
        }

        /// Applies the monster's meandering to [dir].
        public Direction Meander(Direction dir)
        {
            var chance = 10;

            // Breeds are (mostly) smart enough to not meander when they're about to
            // melee. A small chance of meandering is still useful to get a monster out
            // of a doorway sometimes.
            if (Position + dir == Game.Hero.Position)
            {
                chance = 20;
            }

            var meander = Breed.Meander;

            // Being dazzled makes the monster stumble around.
            if (Monster.Dazzle.IsActive)
            {
                meander += Math.Min(6, Monster.Dazzle.Duration/4);
            }

            if (Rng.Instance.Range(chance) >= meander)
            {
                return dir;
            }

            List<Direction> dirs;
            if (dir == Direction.None)
            {
                // Since the monster has no direction, any is equally valid.
                dirs = Direction.All.ToList();
            }
            else
            {
                dirs = new List<Direction>();

                // Otherwise, bias towards the direction the monster is headed.
                for (var i = 0; i < 4; i++)
                {
                    dirs.Add(dir);
                }

                for (var i = 0; i < 3; i++)
                {
                    dirs.Add(dir.rotateLeft45());
                    dirs.Add(dir.rotateRight45());
                }

                for (var i = 0; i < 2; i++)
                {
                    dirs.Add(dir.rotateLeft90());
                    dirs.Add(dir.rotateRight90());
                }

                dirs.Add(dir.rotateLeft90().rotateLeft45());
                dirs.Add(dir.rotateRight90().rotateRight45());
                dirs.Add(dir.rotate180());
            }

            dirs = GetMeanderDirections(dirs);

            if (dirs.Count == 0)
            {
                return dir;
            }

            return Rng.Instance.Item(dirs);
        }

        private List<Direction> GetMeanderDirections(List<Direction> dirs)
        {
            var result = new List<Direction>();

            foreach (var dir in dirs)
            {
                var here = Position + dir;
                if (!Monster.CanOccupy(here))
                {
                    continue;
                }
                var actor = Game.CurrentStage.ActorAt(here);

                if (actor == null || actor == Game.Hero)
                {
                    result.Add(dir);
                }
            }
            return result;
        }
    }

    public class AsleepState : MonsterState
    {
        public override void Defend()
        {
            // Don't sleep through a beating!
            Debugger.Instance.logMonster(Monster, "Wake on hit.");
            Monster.wakeUp();
        }

        public override Action GetAction()
        {
            var distance = (Game.Hero.Position - Position).kingLength();

            // TODO: Make this more cumulative over time. Getting in a drawn out fight
            // next to a monster should definitely wake it up, not subject to a large
            // number of random chances failing.

            // Don't wake up it very far away.
            if (distance > 30)
            {
                Debugger.Instance.logMonster(Monster, "Sleep: Distance " + distance + " is too far to see.");
                return new RestAction();
            }

            // If the monster can see the hero, there's a good chance it will wake up.
            if (isVisible)
            {
                // TODO: Breed-specific sight/alertness.
                if (Rng.Instance.OneIn(distance + 1))
                {
                    Log("{1} notice[s] {2}!", Monster, Game.Hero);
                    Debugger.Instance.logMonster(Monster, "Sleep: In LOS, awoke.");
                    return GetNextStateAction(new AwakeState());
                }

                Debugger.Instance.logMonster(Monster, "Sleep: In LOS, failed oneIn(" + (distance + 1) + ".");
                return new RestAction();
            }

            if (distance > 20)
            {
                Debugger.Instance.logMonster(Monster, "Sleep: Distance " + distance + " is too far to hear");
                return new RestAction();
            }

            // Otherwise, if sound can travel to it from the hero, it may wake up.
            // TODO: Breed-specific hearing.
            // Sound attenuates based on the inverse square of the distance.
            var flowDistance = Game.CurrentStage.getHeroDistanceTo(Position);
            var noise = 0;
            if (flowDistance != null)
            {
                noise = Game.Hero.LastNoise*100/(flowDistance.Value*flowDistance.Value);
            }

            if (noise > Rng.Instance.Range(500))
            {
                Game.Log.Message("Something stirs in the darkness.");
                Debugger.Instance.logMonster(Monster, "Sleep: Passed noise check, flow distance: " + flowDistance + ", noise: " + noise);
                return GetNextStateAction(new AwakeState());
            }

            // Keep sleeping.
            Debugger.Instance.logMonster(Monster, "Sleep: Failed noise check, flow distance: " + flowDistance + ", noise: " + noise);
            return new RestAction();
        }
    }

    public class AIChoice
    {
        public Action CreateAction;
        public string Description;
        public double Score;

        public AIChoice(double score, string description, Action createAction)
        {
            Score = score;
            Description = description;
            CreateAction = createAction;
        }

        public override string ToString()
        {
            return Score + " - " + Description;
        }
    }

    public class AwakeState : MonsterState
    {
        /// How many turns the monster has taken while awake since it last saw the
        /// hero. If it goes too long, it will eventually get bored and fall back
        /// asleep.
        private int _turnsSinceLastSawHero;


        private int maxRange = 9999;

        public override Action GetAction()
        {
            // See if things are quiet enough to fall asleep.
            if (isVisible)
            {
                _turnsSinceLastSawHero = 0;
            }
            else
            {
                _turnsSinceLastSawHero++;

                // The longer it goes without seeing the hero the more likely it will
                // fall asleep.
                if (_turnsSinceLastSawHero > Rng.Instance.Range(10, 20))
                {
                    Debugger.Instance.logMonster(Monster, "Haven't seen hero in " + _turnsSinceLastSawHero + ", sleeping");
                    return GetNextStateAction(new AsleepState());
                }
            }

            // If there is a worthwhile move, use it.
            var moves = Breed.Moves.Where(move => Monster.canUse(move) && move.ShouldUse(Monster)).ToList();
            if (moves.Count != 0)
            {
                return Rng.Instance.Item(moves).GetAction(Monster);
            }

            // If the monster can't walk, then it does melee or waits.
            if (Breed.Flags.Contains("immobile"))
            {
                var toHero = Game.Hero.Position - Position;
                if (toHero.kingLength() == 1)
                {
                    return new WalkAction(toHero);
                }
                return new WalkAction(Direction.None);
            }

            // The monster doesn't have a move to use, so they are going to attack.
            // It needs to decide if it wants to do a ranged attack or a melee attack.
            Monster.wantsToMelee = true;

            // First, it determines how "cautious" it is. Being more cautious makes the
            // monster prefer a ranged attack when possible.

            // Determine how much ranged damage it can dish out per turn.
            var rangedDamage = 0.0;
            var rangedAttacks = 0.0;

            foreach (var move in Breed.Moves)
            {
                // TODO: Handle other ranged damage moves.
                if (!(move is BoltMove))
                {
                    continue;
                }

                rangedDamage += (move as BoltMove).attack.AverageDamage/move.Rate;
                rangedAttacks++;

                // TODO: Take elements into account?
                // TODO: Smart monsters should take hero resists into account.
            }

            if (rangedAttacks != 0)
            {
                // Determine how much melee damage it can dish out per turn.
                var meleeDamage = 0.0;
                var meleeAttacks = 0.0;

                foreach (var attack in Breed.Attacks)
                {
                    // Breeds don't have any raw ranged attacks, just ranged moves.
                    if (!(attack is RangedAttack))
                    {
                        meleeDamage += attack.AverageDamage;
                        meleeAttacks++;
                    }
                    // TODO: Smart monsters should take hero resists into account.
                }

                if (meleeAttacks > 0)
                {
                    meleeDamage /= meleeAttacks;
                }
                rangedDamage /= rangedAttacks;

                // The more damage a monster can do with ranged attacks, relative to its
                // melee attacks, the more cautious it is.
                var damageRatio = 100*rangedDamage/(rangedDamage + meleeDamage);
                var caution = damageRatio;

                // Being afraid makes the monster more cautious.
                caution += Monster.Fear;

                // Being close to death makes the monster more cautious.
                var nearDeath = 100*(1 - Monster.Health.Current/Monster.Health.Max);
                caution += nearDeath;

                // TODO: Breed-specific "aggression" modifier to caution.

                // Less likely to break away for a ranged attack if already in melee
                // distance.
                if (Position - Game.Hero.Position <= 1)
                {
                    Monster.wantsToMelee = caution < 60;
                }
                else
                {
                    Monster.wantsToMelee = caution < 30;
                }
            }

            // Now that we know what the monster *wants* to do, reconcile it with what
            // they're able to do.
            Debugger.Instance.logMonster(Monster, "Trying to find the Melee Path");
            Direction meleeDir;
            try
            {
                meleeDir = FindMeleePath();
            }
            catch (Exception ex)
            {
                Debugger.Instance.logMonster(Monster, "Error Finding Melee Path: " + ex.Message);
                throw;
            }

            Direction rangedDir = null;
            if (rangedAttacks > 0)
            {
                rangedDir = FindRangedPath();
            }

            Direction walkDir;
            if (Monster.wantsToMelee)
            {
                walkDir = meleeDir ?? rangedDir;
            }
            else
            {
                walkDir = rangedDir ?? meleeDir;
            }

            if (walkDir == null)
            {
                walkDir = Direction.None;
            }

            return new WalkAction(Meander(walkDir));
        }

        private bool IsValidRangedPosition(VectorBase position)
        {
            // Ignore tiles that are out of range.
            var toHero = position - Game.Hero.Position;
            if (toHero > maxRange)
            {
                return false;
            }

            // TODO: Being near max range reduces damage. Should try to be within
            // max damage range.

            // Don't go point-blank.
            if (toHero.kingLength() <= 2)
            {
                return false;
            }

            // Ignore occupied tiles.
            var actor = Game.CurrentStage.ActorAt(Position);
            if (actor != null && actor != Monster)
            {
                return false;
            }

            // Ignore tiles that don't have a line-of-sight to the hero.
            return _hasLosFrom(position);
        }

        /// Tries to find a path a desirable position for using a ranged [Move].
        /// 
        /// Returns the [Direction] to take along the path. Returns [Direction.NONE]
        /// if the monster's current position is a good ranged spot. Returns `null`
        /// if no good ranged position could be found.
        private Direction FindRangedPath()
        {
            foreach (var move in Breed.Moves)
            {
                if (move.Range > 0 && move.Range < maxRange)
                {
                    maxRange = move.Range;
                }
            }

            var flow = new Flow(Game.CurrentStage, Position, maxRange, CanOpenDoors);

            // First, see if the current tile or any of its neighbors are good. Once in
            // a tolerable position, the monster will hill-climb to get into a local
            // optimal position (which basically means as far from the hero as possible
            // while still in range).
            Direction best = null;
            var bestDistance = 0;

            if (IsValidRangedPosition(Position))
            {
                best = Direction.None;
                // TODO: Need to decide whether ranged attacks use kingLength or Cartesian
                // and then apply consistently.
                bestDistance = (Position - Game.Hero.Position).lengthSquared();
            }

            foreach (var direction in Direction.All)
            {
                var pos = Monster.Position + direction;
                if (!Monster.CanOccupy(pos))
                {
                    continue;
                }
                if (!IsValidRangedPosition(pos))
                {
                    continue;
                }

                var distance = (pos - Game.Hero.Position).lengthSquared();
                if (distance > bestDistance)
                {
                    best = direction;
                    bestDistance = distance;
                }
            }

            if (best != null)
            {
                return best;
            }

            // Otherwise, we'll need to actually pathfind to reach a good vantage point.
            var dir = flow.DirectionToNearestWhere(IsValidRangedPosition);
            if (dir != Direction.None)
            {
                Debugger.Instance.logMonster(Monster, "Ranged position " + dir);
                return dir;
            }

            // If we get here, couldn't find to a ranged position at all. We may be
            // cornered, or the hero may be surrounded.
            Debugger.Instance.logMonster(Monster, "No good ranged position");
            return null;
        }

        private Direction FindMeleePath()
        {
            // Try to pathfind towards the hero.
            var path = AStar.findPath(Game.CurrentStage, Position, Game.Hero.Position, Breed.Tracking, CanOpenDoors);

            if (path.length == 0)
            {
                return null;
            }

            if (!Monster.CanOccupy(Position + path.direction))
            {
                return null;
            }

            // Don't walk into another monster.
            var actor = Game.CurrentStage.ActorAt(Position + path.direction);
            if (actor != null && actor != Game.Hero)
            {
                return null;
            }
            return path.direction;
        }

        /// Returns `true` if there is an open LOS from [from] to the hero.
        private bool _hasLosFrom(VectorBase from)
        {
            var los = new Los(from, Game.Hero.Position);
            foreach (VectorBase step in los.Points)
            {
                if (step == Game.Hero.Position)
                {
                    return true;
                }
                if (!Game.CurrentStage[step].IsTransparent)
                {
                    return false;
                }
                var actor = Game.CurrentStage.ActorAt(step);
                if (actor != null && actor != Game.Hero)
                {
                    return false;
                }
            }

            throw new ApplicationException("unreachable");
        }
    }

    public class AfraidState : MonsterState
    {
        public override Action GetAction()
        {
            // If we're already in the darkness, rest.
            if (!Game.CurrentStage[Position].Visible)
            {
                return new RestAction();
            }

            // TODO: Should not walk past hero to get to escape!
            // Run to the nearest place the hero can't see.
            var flow = new Flow(Game.CurrentStage, Position, Breed.Tracking, Monster.CanOpenDoors);

            Func<VectorBase, bool> myPredicate = delegate { return !Game.CurrentStage[Position].Visible; };

            var dir = flow.DirectionToNearestWhere(myPredicate);

            if (dir != Direction.None)
            {
                Debugger.Instance.logMonster(Monster, "Fleeing " + dir + " to darkness");
                return new WalkAction(Meander(dir));
            }

            // If we couldn't find a hidden tile, at least try to get some distance.
            var heroDistance = (Position - Game.Hero.Position).kingLength();


            Func<VectorBase, bool> myFartherPredicate = delegate(VectorBase pos)
            {
                var here = Position + pos;
                if (!Monster.CanOccupy(here))
                {
                    return false;
                }
                if (Game.CurrentStage.ActorAt(here) != null)
                {
                    return false;
                }
                return (here - Game.Hero.Position).kingLength() > heroDistance;
            };

            var farther = Direction.All.Where(myFartherPredicate).ToList();

            if (farther.Count != 0)
            {
                var meanderDir = Rng.Instance.Item(farther);
                Debugger.Instance.logMonster(Monster, "Fleeing " + dir + " away from hero");
                return new WalkAction(Meander(dir));
            }

            // If we got here, we couldn't escape. Cornered!
            Debugger.Instance.logMonster(Monster, "Cornered!");

            return GetNextStateAction(new AwakeState());
        }
    }
}