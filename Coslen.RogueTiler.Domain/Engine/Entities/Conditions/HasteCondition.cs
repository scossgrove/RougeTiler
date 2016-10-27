namespace Coslen.RogueTiler.Domain.Engine
{
    /// A condition that temporarily boosts the actor's speed.
    public class HasteCondition : Condition
    {
        public new void OnDeactivate()
        {
            Actor.Log("{1} slow[s] back down.", Actor);
        }
    }
}