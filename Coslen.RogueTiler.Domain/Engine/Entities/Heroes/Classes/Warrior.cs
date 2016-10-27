using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Classes
{
    /// A warrior is focused on combat. Players choosing them don't want to spend
    /// a bunch of time fiddling with commands so almost all warrior abilities are
    /// passive and increase in level automatically simply by doing something
    /// related to the ability.
    public class Warrior : HeroClass
    {
        public Warrior()
        {
            Commands = new List<Command> { new ArcheryCommand(), new LanceCommand(), new SlashCommand(), new StabCommand() };
        }

        /// Increases damage when armed. Trained by killing monsters while armed.
        public TrainedStat Combat = new TrainedStat(100, 240);

        /// Increases damage when unarmed. Trained by killing monsters while unarmed.
        public TrainedStat Fighting = new TrainedStat(80, 60);

        // Each mastery increases damage when wielding a weapon of a given category.
        public Dictionary<string, TrainedStat> Masteries = new Dictionary<string, TrainedStat>();

        // Increases armor. Trained by taking damage.
        public TrainedStat Toughness = new TrainedStat(400, 200);
        public new string Name => "Warrior";

        public new int Armor => Toughness.Level;

        public static TrainedStat NewMasteryStat()
        {
            return new TrainedStat(100, 180);
        }

        public static Warrior Load(int fighting, int combat, int toughness, Dictionary<string, int> masteries)
        {
            var loadedWarror = new Warrior();

            loadedWarror.Fighting.Increment(fighting);
            loadedWarror.Combat.Increment(combat);
            loadedWarror.Toughness.Increment(toughness);

            foreach (var keyPair in  masteries)
            {
                var stat = NewMasteryStat();
                stat.Increment(keyPair.Value);
                loadedWarror.Masteries.Add(keyPair.Key, stat);
            }

            return loadedWarror;
        }

        public override HeroClass Clone()
        {
            var masteryCounts = new Dictionary<string, int>();

            foreach (var keyPair in Masteries)
            {
                masteryCounts.Add(keyPair.Key, keyPair.Value.Count);
            }

            return Load(Fighting.Count, Combat.Count, Toughness.Count, masteryCounts);
        }

        public override Attack ModifyAttack(Attack attack, Actor defender)
        {
            var weapon = Hero.Equipment.Weapon;
            if (weapon != null)
            {
                // TODO: Should combat apply to ranged attacks?
                attack = attack.AddDamage(Combat.Level);

                if (Masteries.ContainsKey(weapon.type.category))
                {
                    var mastery = Masteries[weapon.type.category];
                    if (mastery != null)
                    {
                        attack = attack.MultiplyDamage(1.0 + mastery.Level*0.1);
                    }
                }

                return attack;
            }
            return attack.AddDamage(Fighting.Level);
        }

        public override void TookDamage(Action action, Actor attacker, int damage)
        {
            // Getting hit increases fury.
            Hero.Charge.Current = Math.Min(100, Hero.Charge.Current + 200 * damage / Hero.Health.Max);

            // Indirect damage doesn't increase toughness.
            if (attacker == null)
            {
                return;
            }

            // Reduce damage by armor (again). This is so that toughness increases
            // much more slowly as the hero wears more armor.
            damage = (int) Math.Floor(damage*ArmorUtilities.GetArmorMultiplier(Hero.Armor - Toughness.Level)*10);

            if (Toughness.Increment(damage))
            {
                action.Game.Log.Gain("{1} [have|has] reached toughness level " + Toughness.Level + ".", Hero);
            }
        }

        public override void KilledMonster(Action action, Monster monster)
        {
            var weapon = Hero.Equipment.Weapon;
            TrainedStat stat;
            string name;

            if (weapon != null)
            {
                stat = Combat;
                name = "combat";

                if (!Masteries.ContainsKey(weapon.type.category))
                {
                    Masteries.Add(weapon.type.category, NewMasteryStat());
                }
                var mastery = Masteries[weapon.type.category];
                if (mastery.Increment(monster.Breed.MaxHealth))
                {
                    action.Game.Log.Gain("{1} [have|has] reached ${weapon.type.category} mastery level ${mastery.level}.", Hero);
                }
            }
            else
            {
                stat = Fighting;
                name = "fighting";
            }

            // Base it on the health of the monster to discourage the player from just
            // killing piles of weak monsters.
            if (stat.Increment(monster.Breed.MaxHealth))
            {
                action.Game.Log.Gain($"{Hero.NounText} [have|has] reached {name} level {stat.Level}.");
            }
        }

        public override void FinishedTurn(Action action)
        {
            // Fury decays over time.
            Hero.Charge.Current = Math.Floor(Hero.Charge.Current * 0.9);
        }
    }
}