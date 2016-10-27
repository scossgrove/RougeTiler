namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// A modifier that can be applied to an [Item] to change its capabilities.
    /// For example, in a "Dagger of Wounding", the "of Wounding" part is an affix.
    public class Affix
    {
        public Attack attack;
        public string name;

        public Affix(string name, Attack attack)
        {
            this.name = name;
            this.attack = attack;
        }
    }
}