namespace Coslen.RogueTiler.Domain.Engine
{
    /// A condition that impairs vision.
    public class DazzleCondition : Condition
    {
        public new void OnDeactivate()
        {
            Actor.Log("{1} can see clearly again.", Actor);
        }
    }
}