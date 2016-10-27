using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Logging;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Entities
{
    [Serializable]
    public class Actor : Thing
    {
        /// Cold lowers speed.
        public Condition Cold = new ColdCondition();

        /// Makes it hard for the actor to see.
        public Condition Dazzle = new DazzleCondition();

        /// Haste raises speed.
        public Condition Haste = new HasteCondition();

        /// Poison inflicts damage each turn.
        public Condition Poison = new PoisonCondition();

        // Temporary resistance to elements.
        public Dictionary<Element, ResistCondition> Resistances = new Dictionary<Element, ResistCondition>();

        public Actor(Game game, int x, int y, int health, Noun noun, Pronoun pronoun) : base(new VectorBase(x, y), noun, pronoun)
        {
            Game = game;
            Health = new Stat(health);
            Energy = new Energy();

            foreach (Element element in ElementFactory.Instance.All)
            {
                Resistances[element] = new ResistCondition(element);
            }
            foreach (var condition in Conditions)
            {
                condition.Bind(this);
            }

            ChangePosition(null, new VectorBase(x, y));
        }

        public Game Game { get; set; }
        public Stat Health { get; set; }
        public Energy Energy { get; set; }

        // All [Condition]s for the actor.
        public List<Condition> Conditions
        {
            get
            {
                var result = new List<Condition> {Haste, Cold, Poison, Dazzle};

                result.AddRange(Resistances.Values.Select(x => (Condition) x));

                return result;
            }
        }

        public bool IsAlive => Health.Current > 0;

        /// Whether or not the actor can be seen by the [Hero].
        public bool IsVisible
        {
            get { return Game.CurrentStage[Position].Visible; }
        }

        public virtual bool NeedsInput
        {
            get { return false; }
        }

        public virtual bool CanOpenDoors
        {
            get { return true; }
        }

        /// Gets the actor's current speed, taking into any account any active
        /// [Condition]s.
        public int Speed
        {
            get
            {
                var speed = OnGetSpeed();
                speed += Haste.Intensity;
                speed -= Cold.Intensity;
                return speed;
            }
        }

        /// The actor's dodge ability. This is the percentage chance of a melee
        /// attack missing the actor.
        public int Dodge
        {
            get
            {
                var dodge = 15;

                // Hard to block an attack you can't see coming.
                if (Dazzle.IsActive)
                {
                    dodge -= 5;
                }

                return dodge;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        /// Logs [message] if the actor is visible to the hero.
        public void Log(string message, params Noun[] nouns)
        {
            if (!IsVisible)
            {
                return;
            }
            Game.Log.Message(message, nouns);
        }

        public override void ChangePosition(VectorBase from, VectorBase to)
        {
            if (Game != null && Game.CurrentStage != null)
            {
                Game.CurrentStage.moveActor(from, to);
            }
        }

        public virtual int OnGetSpeed()
        {
            return 0;
        }

        public Action GetAction()
        {
            var action = OnGetAction();
            if (action != null)
            {
                action.Bind(this, true);
            }
            return action;
        }

        public virtual Action OnGetAction()
        {
            return null;
        }

        /// Get an [Attack] for this [Actor] to attempt to hit [defender].
        public Attack GetAttack(Actor defender)
        {
            var attack = OnGetAttack(defender);

            // Hard to hit an actor you can't see.
            if (Dazzle.IsActive)
            {
                attack = attack.AddStrike(-5);
            }

            return attack;
        }

        /// Get an [Attack] for this [Actor] to attempt to hit [defender].
        public virtual Attack OnGetAttack(Actor defender)
        {
            return null;
        }

        /// This is called on the defender when some attacker is attempting to hit it.
        /// The defender can modify the attack or simply return the incoming one.
        public virtual Attack Defend(Attack attack)
        {
            // Apply temporary resistance.
            var resistance = Resistances[attack.Element];
            if (resistance != null && resistance.IsActive)
            {
                attack = attack.AddResistance(resistance.Intensity);
            }

            return attack;
        }

        /// Reduces the actor's health by [damage], and handles its death. Returns
        /// `true` if the actor died.
        public bool TakeDamage(Action action, int damage, Noun attackNoun, Actor attacker = null)
        {
            Health.Current -= damage;
            action.Log("{1} takes {2} damage.", this.NounText, damage.ToString());

            OnDamaged(action, attacker, damage);

            if (Health.Current > 0)
            {
                return false;
            }

            action.AddEvent(EventType.Die, element: ElementFactory.Instance.None, other: null, pos: null, dir: null, actor: this);

            action.Log("{1} kill[s] {2}.", attackNoun, this);
            if (attacker != null)
            {
                attacker.OnKilled(action, this);
            }

            OnDied(attackNoun);

            return true;
        }

        /// Called when this actor has successfully hit this [defender].
        public virtual void OnDamage(Action action, Actor defender, int damage)
        {
            // Do nothing.
        }

        /// Called when [attacker] has successfully hit this actor.
        /// 
        /// [attacker] may be `null` if the damage is not the direct result of an
        /// attack (for example, poison).
        public virtual void OnDamaged(Action action, Actor attacker, int damage)
        {
            // Do nothing.
        }

        /// Called when this Actor has been killed by [attackNoun].
        public virtual void OnDied(Noun attackNoun)
        {
            // Do nothing.
        }

        /// Called when this Actor has killed [defender].
        public virtual void OnKilled(Action action, Actor defender)
        {
            // Do nothing.
        }

        /// Called when this Actor has completed a turn.
        public virtual void OnFinishTurn(Action action)
        {
            // Do nothing.
        }

        public bool CanOccupy(VectorBase pos)
        {
            if (pos.x < 0)
            {
                return false;
            }
            if (pos.x >= Game.CurrentStage.Width)
            {
                return false;
            }
            if (pos.y < 0)
            {
                return false;
            }
            if (pos.y >= Game.CurrentStage.Height)
            {
                return false;
            }

            if (Game.CurrentStage[pos].IsPassable)
            {
                return true;
            }
            return Game.CurrentStage[pos].IsTraversable && CanOpenDoors;
        }

        public void FinishTurn(Action action)
        {
            Energy.Spend();

            if (Conditions != null)
            {
                foreach (var condition in Conditions)
                {
                    condition.Update(action);
                }
            }

            if (IsAlive)
            {
                OnFinishTurn(action);
            }
        }

        public void SetActive(bool isActive)
        {
            // TODO: GameObject.SetActive(isActive);
        }

        public void Destroy()
        {
            // TODO: UnityEngine.Object.Destroy(GameObject);
        }
    }
}