namespace Coslen.RogueTiler.Domain.Engine
{
    /// A condition that temporarily lowers the actor's speed.
    public class ColdCondition : Condition
    {
        public new void OnDeactivate()
        {
            Actor.Log("{1} warm[s] back up.", Actor);
        }
    }
}