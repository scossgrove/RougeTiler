namespace Coslen.RogueTiler.Domain.Engine.Environment
{
    public abstract class QuestBuilder
    {
        // TODO: Kinds of quests:
        // - Get a number or set of items
        // - Explore the entire dungeon
        // - Find a certain item and use it on a certain monster
        // - Get dropped item from monster and use elsewhere.
        //
        // Restrictions that can modify the above:
        // - Complete quest within a turn limit
        // - Complete quest without killing any monsters
        // - Complete quest without using any items

        public virtual Quest Generate(Game game, Stage stage)
        {
            return null;
        }
    }
}