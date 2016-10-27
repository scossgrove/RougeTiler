using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Base class for an [Action] that applies (or extends/intensifies) a
    /// [Condition]. It handles cases where the condition is already in effect with
    /// possibly a different intensity.
    public abstract class ConditionAction : Action
    {
        /// The [Condition] on the actor that should be affected.
        public abstract Condition condition { get; }

        /// The intensity of the condition to apply.
        public virtual int getIntensity()
        {
            return 1;
        }

        /// The number of turns the condition should last.
        public virtual int getDuration()
        {
            return 0;
        }

        /// Override this to log the message when the condition is first applied.
        public virtual void logApply()
        {
        }

        /// Override this to log the message when the condition is already in effect
        /// and its duration is extended.
        public virtual void logExtend()
        {
        }

        /// Override this to log the message when the condition is already in effect
        /// at a weaker intensity and the intensity increases.
        public virtual void logIntensify()
        {
        }

        public override ActionResult OnPerform()
        {
            var intensity = getIntensity();
            var duration = getDuration();

            // TODO: Apply resistance to duration and bail if zero duration.
            // TODO: Don't lower intensity by resistance here (we want to handle that
            // each turn in case it changes), but do see if resistance will lower the
            // intensity to zero. If so, bail.

            if (!condition.IsActive)
            {
                condition.Activate(duration, intensity);
                logApply();
                return ActionResult.Success;
            }

            if (condition.Intensity >= intensity)
            {
                // Scale down the new duration by how much weaker the new intensity is.
                duration = (duration*intensity)/condition.Intensity;

                // Compounding doesn't add as much as the first one.
                duration /= 2;
                if (duration == 0)
                {
                    return Succeed();
                }

                condition.Extend(duration);
                logExtend();
                return ActionResult.Success;
            }

            // Scale down the existing duration by how much stronger the new intensity
            // is.
            var oldDuration = (condition.Duration*condition.Intensity)/intensity;

            condition.Activate(oldDuration + duration/2, intensity);
            logIntensify();
            return ActionResult.Success;
        }
    }

    public class HasteAction : ConditionAction
    {
        private readonly int _duration;
        private readonly int _speed;

        public HasteAction(int duration, int speed)
        {
            _duration = duration;
            _speed = speed;
        }

        public override Condition condition
        {
            get { return Actor.Haste; }
        }

        public override int getIntensity()
        {
            return _speed;
        }

        public override int getDuration()
        {
            return _duration;
        }

        public override void logApply()
        {
            Log("{1} start[s] moving faster.", Actor);
        }

        public override void logExtend()
        {
            Log("{1} [feel]s the haste lasting longer.", Actor);
        }

        public override void logIntensify()
        {
            Log("{1} move[s] even faster.", Actor);
        }
    }

    public class FreezeAction : ConditionAction
    {
        private readonly int _damage;

        public FreezeAction(int damage)
        {
            _damage = damage;
        }

        public override Condition condition
        {
            get { return Actor.Cold; }
        }

        // TODO: Should also break items in inventory.

        public override int getIntensity()
        {
            return 1 + _damage/40;
        }

        public override int getDuration()
        {
            return 3 + Rng.Instance.triangleInt(_damage*2, _damage/2);
        }

        public override void logApply()
        {
            Log("{1} [are|is] frozen!", Actor);
        }

        public override void logExtend()
        {
            Log("{1} feel[s] the cold linger!", Actor);
        }

        public override void logIntensify()
        {
            Log("{1} feel[s] the cold intensify!", Actor);
        }
    }

    public class PoisonAction : ConditionAction
    {
        private readonly int _damage;

        public PoisonAction(int damage)
        {
            _damage = damage;
        }

        public override Condition condition
        {
            get { return Actor.Poison; }
        }

        public override int getIntensity()
        {
            return 1 + _damage/20;
        }

        public override int getDuration()
        {
            return 1 + Rng.Instance.triangleInt(_damage*2, _damage/2);
        }

        public override void logApply()
        {
            Log("{1} [are|is] poisoned!", Actor);
        }

        public override void logExtend()
        {
            Log("{1} feel[s] the poison linger!", Actor);
        }

        public override void logIntensify()
        {
            Log("{1} feel[s] the poison intensify!", Actor);
        }
    }

    internal class DazzleAction : ConditionAction
    {
        private readonly int _damage;

        public DazzleAction(int damage)
        {
            _damage = damage;
        }

        public override Condition condition
        {
            get { return Actor.Dazzle; }
        }

        public override int getDuration()
        {
            return 3 + Rng.Instance.triangleInt(_damage*2, _damage/2);
        }

        public override void logApply()
        {
            Log("{1} [are|is] dazzled by the light!", Actor);
        }

        public override void logExtend()
        {
            Log("{1} [are|is] dazzled by the light!", Actor);
        }
    }

    public class ResistAction : ConditionAction
    {
        private readonly int _duration;
        private readonly Element _element;

        public ResistAction(int duration, Element element)
        {
            _duration = duration;
            _element = element;
        }

        public override Condition condition
        {
            get { return Actor.Resistances[_element]; }
        }

        public override int getDuration()
        {
            return _duration;
        }

        // TODO: Resistances of different intensity.
        public override void logApply()
        {
            Log("{1} [are|is] resistant to " + _element + ".", Actor);
        }

        public override void logExtend()
        {
            Log("{1} feel[s] the resistance extend.", Actor);
        }
    }
}