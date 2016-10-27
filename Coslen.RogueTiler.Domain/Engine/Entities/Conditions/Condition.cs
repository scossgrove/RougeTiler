using System;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// A temporary condition that modifies some property of an [Actor] while it
    /// is in effect.
    [Serializable]
    public abstract class Condition
    {
        /// The number of turns that the condition while remain in effect for.
        private int turnsRemaining;

        /// The [Actor] that this condition applies to.
        public Actor Actor { get; private set; }

        /// The "intensity" of this condition. The interpretation of this varies from
        /// condition to condition.
        public int Intensity { get; set; }

        /// Gets whether the condition is currently in effect.
        public bool IsActive
        {
            get { return turnsRemaining > 0; }
        }

        public int Duration
        {
            get { return turnsRemaining; }
        }

        /// Binds the condition to the actor that it applies to. Must be called and
        /// can only be called once.
        public void Bind(Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException();
            }
            Actor = actor;
        }

        /// Processes one turn of the condition.
        public void Update(Action action)
        {
            if (IsActive)
            {
                turnsRemaining--;
                if (IsActive)
                {
                    OnUpdate(action);
                }
                else
                {
                    OnDeactivate();
                    Intensity = 0;
                }
            }
        }

        /// Extends the condition by [duration].
        public void Extend(int duration)
        {
            turnsRemaining += duration;
        }

        /// Activates the condition for [duration] turns at [intensity].
        public void Activate(int duration, int intensity = 1)
        {
            turnsRemaining = duration;
            Intensity = intensity;
        }

        /// Cancels the condition immediately. Does not deactivate the condition.
        public void Cancel()
        {
            turnsRemaining = 0;
            Intensity = 0;
        }

        public void OnUpdate(Action action)
        {
        }

        public void OnDeactivate()
        {
        }
    }
}