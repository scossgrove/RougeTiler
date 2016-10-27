using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// A condition that inflicts damage every turn.
    public class PoisonCondition : Condition
    {
        public new void OnUpdate(Action action)
        {
            // TODO: Apply resistances. If resistance lowers intensity to zero, end
            // condition and log message.

            if (!Actor.TakeDamage(action, Intensity, new Noun("the poison")))
            {
                Actor.Log("{1} [are|is] hurt by poison!", Actor);
            }
        }

        public new void OnDeactivate()
        {
            Actor.Log("{1} [are|is] no longer poisoned.", Actor);
        }
    }
}