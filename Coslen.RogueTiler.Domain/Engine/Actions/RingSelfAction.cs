namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Creates an expanding ring of damage centered on the [Actor].
    /// 
    /// This class mainly exists as an [Action] that [Item]s can use.
    public class RingSelfAction : Action
    {
        private readonly Attack _attack;

        public RingSelfAction(Attack attack)
        {
            _attack = attack;
        }

        public override ActionResult OnPerform()
        {
            return alternate(RayAction.ring(Actor.Position, _attack));
        }
    }
}