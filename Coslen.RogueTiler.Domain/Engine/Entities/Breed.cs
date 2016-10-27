using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Entities
{
    public class Breed : Quantifiable
    {
        /// The name of the breed. If the breed's name has irregular pluralization
        /// like "bunn[y|ies]", this will be the original unparsed string.
        private readonly string name;

        public Breed(
                string name, Pronoun pronoun, Glyph glyph, 
                List<Attack> attacks, List<Move> moves, 
                Drop drop, 
                int maxHealth, 
                int tracking, 
                int meander, 
                int speed, 
                List<string> flags, 
                string slug)
        {
            this.name = name;
            Pronoun = pronoun;

            Appearance = new Appearence {Glyph = glyph.Appearance, ForeGroundColor = glyph.Fore, Slug = slug, Type = AppearenceType.Monster};

            Attacks = attacks;
            Moves = moves;
            Drop = drop;
            MaxHealth = maxHealth;
            Tracking = tracking;
            Meander = meander;
            Speed = speed;
            Flags = flags;

            if (Flags.Contains(""))
            {
                throw new ArgumentException();
            }
        }

        public new Pronoun Pronoun { get; set; }

        public string Name
        {
            get { return Singular; }
        }

        /// Typed so the engine is coupled to how monsters appear.
        public Appearence Appearance { get; set; }

        public List<Attack> Attacks { get; set; }
        public List<Move> Moves { get; set; }

        public int MaxHealth { get; set; }

        /// How well the monster can navigate the stage to reach its target.
        /// Used to determine maximum pathfinding distance.
        public int Tracking { get; set; }

        /// How much randomness the monster has when walking towards its target.
        public int Meander { get; set; }

        /// The breed's speed, relative to normal. Ranges from `-6` (slowest) to `6`
        /// (fastest) where `0` is normal speed.
        public int Speed { get; set; }

        /// The [Item]s this monster may drop when killed.
        public Drop Drop { get; set; }

        public List<string> Flags { get; set; }

        public new string Singular
        {
            get { return Log.ParsePlural(name, false, true); }
        }

        public new string Plural
        {
            get { return Log.ParsePlural(name, true, true); }
        }


        /// How much experience a level one [Hero] gains for killing a [Monster] of
        /// this breed.
        public int ExperienceCents
        {
            get
            {
                // The more health it has, the longer it can hurt the hero.
                var exp = (double) MaxHealth;

                // Faster monsters are worth more.
                exp *= Energy.Gains[Energy.NormalSpeed + Speed];

                // Average the attacks (since they are selected randomly) and factor them
                // in.
                var attackTotal = 0.0;
                foreach (var attack in Attacks)
                {
                    // TODO: Take range into account?
                    attackTotal += attack.AverageDamage*Option.ExpElement[attack.Element];
                }

                attackTotal /= Attacks.Count;

                var moveTotal = 0.0;
                var moveRateTotal = 0.0;
                foreach (var move in Moves)
                {
                    // Scale by the move rate. The less frequently a move can be performed,
                    // the less it affects experience.
                    moveTotal += move.Experience/move.Rate;

                    // Magify the rate to roughly account for the fact that a move may not be
                    // applicable all the time.
                    moveRateTotal += 1/(move.Rate*2);
                }

                // A monster can only do one thing each turn, so even if the move rates
                // are better than than, limit it.
                moveRateTotal = Math.Min(1.0, moveRateTotal);

                // Time spent using moves is not time spent attacking.
                attackTotal *= (1.0 - moveRateTotal);

                // Add in moves and attacks.
                exp *= attackTotal + moveTotal;

                // Take into account flags.
                foreach (var flag in Flags)
                {
                    try
                    {
                        exp *= Option.ExpFlag[flag];
                    }
                    catch (Exception ex)
                    {
                    }
                }

                // Meandering monsters are worth less.
                var a = Option.ExpMeander - Meander;
                var meanderModifier = a/(double) Option.ExpMeander;

                exp *= meanderModifier;

                exp = (int) exp;

                return (int) exp;
            }
        }

        /// When a [Monster] of this Breed is generated, how many of the same type
        /// should be spawned together (roughly).
        public int NumberInGroup
        {
            get
            {
                if (Flags.Contains("horde"))
                {
                    return 12;
                }
                if (Flags.Contains("swarm"))
                {
                    return 8;
                }
                if (Flags.Contains("pack"))
                {
                    return 6;
                }
                if (Flags.Contains("group"))
                {
                    return 4;
                }
                if (Flags.Contains("few"))
                {
                    return 2;
                }
                return 1;
            }
        }

        public Monster Spawn(Game game, VectorBase pos, Monster parent = null)
        {
            var generation = 1;
            if (parent != null)
            {
                generation = parent.Generation + 1;
            }

            return new Monster(game, this, pos.x, pos.y, MaxHealth, generation);
        }
    }
}