using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Behaviors
{
    /// Automatic running.
    public class RunBehavior : Behavior
    {
        private Direction direction;
        private bool firstStep = true;

        /// Whether the hero is running with open tiles to their left.
        private bool? openLeft;

        /// Whether the hero is running with open tiles to their right.
        private bool? openRight;

        public RunBehavior(Direction direction)
        {
            this.direction = direction;
        }

        public override bool CanPerform(Hero hero)
        {
            if (hero == null)
            {
                throw new ArgumentNullException(nameof(hero));
            }
            if (firstStep)
            {
                // On first step, always try to go in direction player pressed.
            }
            else if (openLeft == null)
            {
                // On the second step, figure out if we're in a corridor and which way
                // it's going. If the hero is running straight (NSEW), allow up to a 90°
                // turn. This covers cases like:
                //
                //     ####
                //     .@.#
                //     ##.#
                //
                // If the player presses right here, we want to take a first step, then
                // turn and run south. If the hero is running diagonally, we only allow
                // a 45° turn. That way it doesn't get confused by cases like:
                //
                //      #.#
                //     ##.##
                //     .@...
                //     #####
                //
                // If the player presses NE here, we want to run north and not get
                // confused by the east passage.
                var dirs = new List<Direction> {direction.rotateLeft45(), direction, direction.rotateRight45()};

                if (Direction.Cardinal.Contains(direction))
                {
                    dirs.Add(direction.rotateLeft90());
                    dirs.Add(direction.rotateRight90());
                }

                var openDirs = dirs.Where(dir => _isOpen(hero, dir)).ToList();

                if (openDirs.Count == 0)
                {
                    return false;
                }

                if (openDirs.Count == 1)
                {
                    // Entering a corridor.
                    openLeft = false;
                    openRight = false;

                    // The direction may change if the first step entered a corridor from
                    // around a corner.
                    direction = openDirs.First();
                }
                else
                {
                    // Entering an open area.
                    openLeft = _isOpen(hero, direction.rotateLeft90());
                    openRight = _isOpen(hero, direction.rotateRight90());
                }
            }
            else if (!openLeft.Value && !openRight.Value)
            {
                if (!_runInCorridor(hero))
                {
                    return false;
                }
            }
            else
            {
                if (!_runInOpen(hero))
                {
                    return false;
                }
            }

            return _shouldKeepRunning(hero);
        }

        public override Action GetAction(Hero hero)
        {
            firstStep = false;
            return new WalkAction(direction);
        }

        /// Advance one step while in a corridor.
        /// 
        /// The hero will follow curves and turns as long as there is only one
        /// direction they can go. (This is more or less true, though right-angle
        /// turns need special handling.)
        private bool _runInCorridor(Hero hero)
        {
            // Keep running as long as there's only one direction to go. Allow up to a
            // 90° turn while running.
            var openDirs = new List<Direction> {direction.rotateLeft90(), direction.rotateLeft45(), direction, direction.rotateRight45(), direction.rotateRight90()}.Where(dir => _isOpen(hero, dir)).ToList();

            if (openDirs.Count == 1)
            {
                direction = openDirs.First();
                return true;
            }

            // Corner case, literally. If we're approaching a right-angle turn, keep
            // going. We'd normally stop here because there are two ways you can go,
            // straight into the corner of the turn (1) or diagonal to take a shortcut
            // around it (2):
            //
            //     ####
            //     #12.
            //     #@##
            //     #^#
            //
            // We detect this case by seeing if there are two (and only two) open
            // directions: ahead and 45° *and* if one step past that is blocked.
            if (openDirs.Count != 2)
            {
                return false;
            }
            if (!openDirs.Contains(direction))
            {
                return false;
            }
            if (!openDirs.Contains(direction.rotateLeft45()) && !openDirs.Contains(direction.rotateRight45()))
            {
                return false;
            }

            var twoStepsAhead = hero.Game.CurrentStage[hero.Position + direction*2].IsTraversable;
            if (twoStepsAhead)
            {
                return false;
            }

            // If we got here, we're in a corner. Keep going straight.
            return true;
        }

        private bool _runInOpen(Hero hero)
        {
            // Whether or not the hero's left and right sides are open cannot change.
            // In other words, if he is running along a wall on his left (closed on
            // left, open on right), he will stop if he enters an open room (open on
            // both).
            var nextLeft = _isOpen(hero, direction.rotateLeft45());
            var nextRight = _isOpen(hero, direction.rotateRight45());
            return openLeft == nextLeft && openRight == nextRight;
        }

        /// Returns `true` if the hero can run one step in the current direction.
        /// 
        /// Returns `false` if they should stop because they'd hit a wall or actor.
        private bool _shouldKeepRunning(Hero hero)
        {
            var stage = hero.Game.CurrentStage;
            var pos = hero.Position + direction;
            if (!stage[pos].IsPassable)
            {
                return false;
            }

            // Don't run into someone.
            if (stage.ActorAt(pos) != null)
            {
                return false;
            }

            // Don't run next to someone.
            if (stage.ActorAt(pos + direction.rotateLeft90()) != null)
            {
                return false;
            }
            if (stage.ActorAt(pos + direction.rotateLeft45()) != null)
            {
                return false;
            }
            if (stage.ActorAt(pos + direction) != null)
            {
                return false;
            }
            if (stage.ActorAt(pos + direction.rotateRight45()) != null)
            {
                return false;
            }
            if (stage.ActorAt(pos + direction.rotateRight90()) != null)
            {
                return false;
            }

            return true;
        }

        private bool _isOpen(Hero hero, Direction dir)
        {
            if (hero == null)
            {
                throw new ArgumentNullException(nameof(hero));
            }
            return hero.Game.CurrentStage[hero.Position + dir].IsTraversable;
        }
    }
}