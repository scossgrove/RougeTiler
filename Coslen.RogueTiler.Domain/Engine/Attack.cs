using System;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Logging;
using Coslen.RogueTiler.Domain.Utilities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine
{
    public class Attack : ICloneable
    {
        public Attack(string verb, double baseDamage, Element element = null, Noun noun = null)
        {
            Verb = verb;
            BaseDamage = baseDamage;
            if (element == null)
            {
                element = ElementFactory.Instance.None;
            }
            Element = element;
            Noun = noun;

            DamageBonus = 0;
            DamageScale = 1.0f;
            Armor = 0.0f;
            StrikeBonus = 0.0f;
            Resistance = 0;
        }

        /// The thing performing the attack. If `null`, then the attacker will be
        /// used.
        public Noun Noun { get; set; }

        /// A verb string describing the attack: "hits", "fries", etc.
        public string Verb { get; set; }

        /// The bonus applied to the defender's base dodge ability. A higher bonus
        /// makes it more likely the attack will make contact.
        public double StrikeBonus { get; set; }

        /// The average damage. The actual damage will be a `Rng.triangleInt` centered
        /// on this with a range of 1/2 of its value.
        public double BaseDamage { get; set; }

        /// Additional damage added to [baseDamage] after the multiplier has been
        /// applied.
        public double DamageBonus { get; set; }

        /// The multiplier for [baseDamage].
        public double DamageScale { get; set; }

        /// The average damage inflicted by the attack.
        public double AverageDamage
        {
            get { return BaseDamage*DamageScale + DamageBonus; }
        }

        /// The element for the attack.
        public Element Element { get; set; }

        /// The defender's armor.
        public double Armor { get; set; }

        /// The defender's level of resistance to the attack's element.
        /// 
        /// Zero means no resistance. Everything above that reduces damage by
        /// 1/(resistance + 1), so that one resists is half damage, two is third, etc.
        /// Secondary effects from the element are nullified if the defender has any
        /// resistance.
        public int Resistance { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// Creates an [Attack] intended to be passed to [combine].
        public void Modifier(Element element, int? strikeBonus, int? damageBonus, double? damageScale)
        {
            Verb = "";
            //this.Noun = null;
            BaseDamage = 0;
            Element = element ?? ElementFactory.Instance.None;
            StrikeBonus = strikeBonus ?? 0;
            DamageBonus = damageBonus ?? 0;
            DamageScale = damageScale ?? 1.0f;
        }

        /// Returns a new attack identical to this one but with [offset] added.
        public Attack AddDamage(double offset)
        {
            var newAttack = (Attack) Clone();
            newAttack.DamageBonus += offset;
            return newAttack;
        }

        /// Returns a new attack identical to this one but with [element].
        public Attack Brand(Element element)
        {
            var newAttack = (Attack) Clone();
            newAttack.Element = element;
            return newAttack;
        }

        /// Returns a new attack identical to this one but with [bonus] added to the
        /// strike modifier.
        public Attack AddStrike(double bonus)
        {
            var newAttack = (Attack) Clone();
            newAttack.StrikeBonus += bonus;
            return newAttack;
        }

        /// Returns a new attack identical to this one but with damage scaled by
        /// [factor].
        public Attack MultiplyDamage(double factor)
        {
            var newAttack = (Attack) Clone();
            newAttack.DamageScale *= factor;
            return newAttack;
        }

        /// Returns a new attack with [armor] added to it.
        public Attack AddArmor(double armor)
        {
            var newAttack = (Attack) Clone();
            newAttack.Armor += armor;
            return newAttack;
        }

        /// Returns a new attack with [resist] added to it.
        public Attack AddResistance(int resist)
        {
            var newAttack = (Attack) Clone();
            newAttack.Resistance += resist;
            return newAttack;
        }

        /// Creates a new attack that combines this one with [modifier].
        public Attack Combine(Attack modifier)
        {
            var result = (Attack) Clone();
            result.StrikeBonus += modifier.StrikeBonus;
            result.DamageBonus += modifier.DamageBonus;
            result.DamageScale *= modifier.DamageScale;

            if (modifier.Element != ElementFactory.Instance.None)
            {
                result.Element = modifier.Element;
            }

            return result;
        }

        /// Performs a melee [attack] from [attacker] to [defender] in the course of
        /// [action].
        /// 
        /// Returns `true` if the attack connected.
        public bool Perform(Action action, Actor attacker, Actor defender, bool canMiss = true)
        {
            var attack = defender.Defend(this);
            return attack.PerformAttack(action, attacker, defender, canMiss);
        }

        public bool PerformAttack(Action action, Actor attacker, Actor defender, bool? canMiss)
        {
            if (canMiss == null)
            {
                canMiss = true;
            }

            var attackNoun = Noun ?? attacker;

            // See if the attack hits.
            if (canMiss.Value)
            {
                var dodge = defender.Dodge + StrikeBonus;
                var strike = Rng.Instance.Inclusive(1, 100);

                // There's always at least a 5% chance of missing and a 5% chance of
                // hitting, regardless of all modifiers.
                strike = strike.Clamp(5, 95);

                if (strike < dodge)
                {
                    action.Log("{1} miss[es] {2}.", attackNoun, defender);
                    return false;
                }
            }

            // Roll for damage.
            var damage = _rollDamage();

            if (damage == 0)
            {
                // Armor cancelled out all damage.
                action.Log("{1} do[es] no damage to {2}.", attackNoun, defender);
                return true;
            }

            attacker.OnDamage(action, defender, damage);
            if (defender.TakeDamage(action, damage, attackNoun, attacker))
            {
                return true;
            }

            if (Resistance == 0)
            {
                _elementalSideEffect(defender, action, damage);
            }

            // TODO: Pass in and use element.
            action.AddEvent(EventType.Hit, defender, damage.ToString());
            action.Log("{1} " + Verb + " {2}.", attackNoun, defender);
            return true;
        }

        private void _elementalSideEffect(Actor defender, Action action, int damage)
        {
            // Apply any element-specific effects.
            //switch (Element.Name)
            //{
            //    case ElementFactory.Instance.none:
            //        // No effect.
            //        break;

            //    case ElementFactory.Instance.air:
            //        // TODO: Teleport.
            //        break;

            //    case ElementFactory.Instance.earth:
            //        // TODO: Cuts?
            //        break;

            //    case ElementFactory.Instance.fire:
            //        action.AddAction(new BurnAction(damage), defender);
            //        break;

            //    case ElementFactory.Instance.water:
            //        // TODO: Push back.
            //        break;

            //    case ElementFactory.Instance.acid:
            //        // TODO: Destroy items.
            //        break;

            //    case ElementFactory.Instance.cold:
            //        action.AddAction(new FreezeAction(damage), defender);
            //        break;

            //    case ElementFactory.Instance.lightning:
            //        // TODO: Break glass. Recharge some items?
            //        break;

            //    case ElementFactory.Instance.poison:
            //        action.AddAction(new PoisonAction(damage), defender);
            //        break;

            //    case ElementFactory.Instance.dark:
            //        // TODO: Blind.
            //        break;

            //    case ElementFactory.Instance.light:
            //        action.AddAction(new DazzleAction(damage), defender);
            //        break;

            //    case ElementFactory.Instance.spirit:
            //        // TODO: Drain experience.
            //        break;
            //}
        }

        private int _rollDamage()
        {
            // Calculate in cents so that we don't do as much rounding until after
            // armor is taken into account.
            var damageCents = (int) (AverageDamage*100);
            var rolled = Rng.Instance.triangleInt(damageCents, damageCents/2);
            rolled *= (int) ArmorUtilities.GetArmorMultiplier(Armor);
            return (int) Math.Round(rolled/100f);
        }

        public override string ToString()
        {
            var result = ((int) BaseDamage).ToString();

            if (DamageBonus > 0)
            {
                result += "+" + DamageBonus;
            }
            else if (DamageBonus < 0)
            {
                result += "" + DamageBonus;
            }

            if (DamageScale != 1.0)
            {
                result += "x" + DamageScale;
            }

            if (Element != ElementFactory.Instance.None)
            {
                result += " " + Element;
            }

            return result;
        }

        private void _copyTo(Attack other)
        {
            other.StrikeBonus = StrikeBonus;
            other.DamageBonus = DamageBonus;
            other.DamageScale = DamageScale;
            other.Armor = Armor;
            other.Resistance = Resistance;
        }
    }

    public static class ArmorUtilities
    {
        /// Armor reduces damage by an inverse curve such that increasing armor has
        /// less and less effect. Damage is reduced to the following:
        /// 
        /// armor damage
        /// ------------
        /// 0     100%
        /// 40    50%
        /// 80    33%
        /// 120   25%
        /// 160   20%
        /// ...   etc.
        public static double GetArmorMultiplier(double armor)
        {
            // Damage is never increased.
            return 1.0/(1.0 + Math.Max(0, armor)/40.0);
        }
    }

    public class RangedAttack : Attack, ICloneable
    {
        public RangedAttack(Noun noun, string verb, double baseDamage, Element element, int range) : base(verb, baseDamage, element, noun)
        {
            this.Range = range;
        }

        /// The maximum range of the attack.
        public int Range { get; set; }

        public new object Clone()
        {
            return MemberwiseClone();
        }
    }
}