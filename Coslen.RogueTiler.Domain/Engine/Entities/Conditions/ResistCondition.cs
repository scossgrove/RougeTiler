namespace Coslen.RogueTiler.Domain.Engine
{
    /// A condition that provides resistance to an element.
    public class ResistCondition : Condition
    {
        private readonly Element element;

        public ResistCondition(Element element)
        {
            this.element = element;
        }

        public new void OnDeactivate()
        {
            Actor.Log("{1} feel[s] susceptible to " + element + ".", Actor);
        }
    }
}