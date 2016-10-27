using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Behaviors;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    /// The main player-controlled [Actor]. The player's avatar in the game world.
    public class Hero : Actor
    {

        public Hero(Game game, VectorBase pos, HeroSave save, Pronoun pronoun, string name) : base(game, pos.x, pos.y, Option.HeroHealthStart, new Noun("Hero"), pronoun)
        {
            HeroClass = save.HeroClass.Clone();
            Inventory = save.Inventory.clone();
            Equipment = save.Equipment.clone();
            BackPack = save.BackPack.clone();
            Crucible = save.Crucible.clone();

            ExperienceCents = save.ExperienceCents;
            Gold = save.Gold;

            // Hero state is cloned so that if they die in the dungeon, they lose
            // anything they found.
            RefreshLevel(false);

            HeroClass.Bind(this);

            // Give the hero energy so we can act before all of the monsters.
            Energy.CurrentEnergy = Energy.ActionCost;

            // Start with some initial ability to rest so we aren't weakest at the very
            // beginning.
            Food = save.Food;

            Health = save.Health;
            Charge = save.Charge;

            this.Name = name;
        }

        public new string NounText
        {
            get { return "you"; }
        }

        //public Pronoun Pronoun { get; }

        public HeroClass HeroClass { get; set; }

        public Inventory Inventory { get; set; } = new Inventory(Option.InventoryCapacity);

        public Equipment Equipment { get; set; }


        public Inventory BackPack { get; set; } = new Inventory(Option.HomeCapacity);
        public Inventory Crucible { get; set; } = new Inventory(Option.CrucibleCapacity);

        /// Experience is stored internally as hundredths of a point for higher (but
        /// not floating point) precision.
        public int ExperienceCents { get; set; }

        /// The hero's experience level.
        public int Level { get; set; }

        public double LevelPercentage
        {
            get
            {
                var levelPercent = (double)(100 * Experience ) / LevelUtilties.CalculateLevelCost(Level + 1).Value;
                levelPercent = levelPercent*10;
                levelPercent = Math.Floor(levelPercent);
                levelPercent = levelPercent/10;
                return levelPercent;
            }
        }

        public int Gold { get; set; }

        public Behavior Behavior { get; set; }

        /// How much "food" the hero has.
        /// 
        /// The hero gains food by exploring the level and can spend it while resting
        /// to regain health.
        public double Food
        {
            get { return _food; }
            set
            {
                _food = value;
            }
        }
        private double _food;

        /// The hero's current "charge".
        /// 
        /// This is interpreted and managed differently for each class: "fury" for
        /// warriors, "mana" for mages, etc.
        public Stat Charge { get; set; }

        /// How much noise the Hero's last action made.
        public int LastNoise { get; set; }

        // TODO: Hackish.
        public new Appearence Appearance
        {
            get { return new Appearence {Glyph = "@", IsHidden = false, IsInShadow = false, Type = AppearenceType.Hero}; }
        }

        // Calculated
        public override bool NeedsInput
        {
            get
            {
                if (Behavior != null && !Behavior.CanPerform(this))
                {
                    WaitForInput();
                }

                return Behavior == null;
            }
        }

        // Calculated
        public int Experience
        {
            get { return ExperienceCents/100; }
        }

        // Calculated
        public int Armor
        {
            get
            {
                var total = 0;

                foreach (var item in Equipment.slots.Values)
                {
                    if (item != null)
                    {
                        total += item.armor;
                    }
                }

                total += HeroClass.Armor;

                return total;
            }
        }

        /// <summary>
        /// This is the name for the hero.
        /// </summary>
        public string Name { get; set; }


        //public HeroSave ToHeroSave()
        //{
        //    var save = new HeroSave("test", HeroClass);

        //    save.HeroClass = HeroClass;
        //    save.Inventory = Inventory;
        //    save.Equipment = Equipment;
        //    save.ExperienceCents = ExperienceCents;

        //    return save;
        //}

        /// Increases the hero's food by an appropriate amount after having explored
        /// [numExplored] additional tiles.
        public void Explore(int numExplored)
        {
            Food += Health.Max * Game.CurrentStage.Abundance * numExplored / Game.CurrentStage.numExplorable;
        }

        public override int OnGetSpeed()
        {
            return Energy.NormalSpeed;
        }

        public override Action OnGetAction()
        {
            return Behavior.GetAction(this);
        }

        public override Attack OnGetAttack(Actor defender)
        {
            Attack attack = null;

            // See if a melee weapon is equipped.
            var weapon = Equipment.Weapon;
            if (weapon != null && !weapon.isRanged)
            {
                attack = weapon.attack;
            }
            else
            {
                attack = new Attack("punch[es]", Option.HeroPunchDamage);
            }

            // Let the class modify it.
            return HeroClass.ModifyAttack(attack, defender);
        }

        public override Attack Defend(Attack attack)
        {
            Disturb();
            attack = base.Defend(attack);
            return attack.AddArmor(Armor);
        }

        public override void OnDamaged(Action action, Actor attacker, int damage)
        {
            HeroClass.TookDamage(action, attacker, damage);
        }

        //public override void OnKilled(Action.Action action, Monster defender)
        public override void OnKilled(Action action, Actor defender)
        {
            var monster = (Monster) defender;
            ExperienceCents += monster.ExperienceCents/Level;
            RefreshLevel(true);
            HeroClass.KilledMonster(action, monster);
        }

        public override void OnDied(Noun attackNoun)
        {
            Game.Log.Message("you were slain by {1}.", attackNoun);
        }

        public override void OnFinishTurn(Action action)
        {
            // Make some noise.
            LastNoise = action.Noise;

            HeroClass.FinishedTurn(action);
        }

        public override void ChangePosition(VectorBase from, VectorBase to)
        {
            base.ChangePosition(from, to);
            if (Game != null && Game.CurrentStage != null)
            {
                Game.CurrentStage.dirtyVisibility();
                Game.CurrentStage.LastHeroPosition = to;
                //Game.quest.enterTile(game, Game.CurrentStage[to]);
            }
        }

        public void WaitForInput()
        {
            Behavior = null;
        }

        public void SetNextAction(Action action)
        {
            Behavior = new ActionBehavior(action);
        }

        /// Starts resting, if the hero has eaten and is able to regenerate.
        public bool Rest()
        {
            if (Poison.IsActive)
            {
                Game.Log.Error("You cannot rest while poison courses through your veins!");
                return false;
            }

            if (Food == 0)
            {
                Game.Log.Error("You must explore more before you can rest.");
                return false;
            }

            Behavior = new RestBehavior();
            return true;
        }

        public void Run(Direction direction)
        {
            Behavior = new RunBehavior(direction);
        }

        public void Disturb()
        {
            if (!(Behavior is ActionBehavior))
            {
                WaitForInput();
            }
        }

        public void RefreshLevel(bool log = false)
        {
            var level = LevelUtilties.CalculateLevel(ExperienceCents);

            // See if the we levelled up.
            while (Level < level)
            {
                Level++;
                Health.Max += Option.HeroHealthGain;
                Health.Current += Option.HeroHealthGain;

                if (log)
                {
                    Game.Log.Gain("{1} [have|has] reached level " + level + ".", this);
                }
            }
        }

        // This is intended to swap the weapons in there hand
        public void Swap()
        {
            throw new NotImplementedException();
        }

        // This is intended to toss/throw the item in the hand of the hero
        public void Toss()
        {
            throw new NotImplementedException();
        }
    }
}