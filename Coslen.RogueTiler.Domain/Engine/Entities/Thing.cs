using System;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Entities
{
    [Serializable]
    public abstract class Thing : Noun
    {
        private VectorBase position;

        protected Thing(VectorBase position, Noun noun, Pronoun pronoun) : base(noun == null ? string.Empty : noun.NounText)
        {
            Position = position;
            Pronoun = pronoun;
        }

        public VectorBase Position
        {
            get { return position; }
            set
            {
                if (value != position)
                {
                    ChangePosition(position, value);
                    position = value;
                }
            }
        }

        public virtual Appearence Appearance { get; set; }

        public new virtual Pronoun Pronoun { get; private set; }

        /// Called when the actor's position is about to change from [from] to [to].
        public virtual void ChangePosition(VectorBase from, VectorBase to)
        {
        }
    }
}