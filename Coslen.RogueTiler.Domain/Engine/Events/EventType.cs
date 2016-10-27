namespace Coslen.RogueTiler.Domain.Engine
{
    /// A kind of [Event] that has occurred.
    public class EventType
    {
        /// One step of a bolt.
        public static EventType Bolt = new EventType("bolt");

        /// The leading edge of a cone.
        public static EventType Cone = new EventType("cone");

        /// A thrown item in flight.
        public static EventType Toss = new EventType("toss");

        /// An [Actor] was hit.
        public static EventType Hit = new EventType("hit");

        /// An [Actor] died.
        public static EventType Die = new EventType("die");

        /// An [Actor] was healed.
        public static EventType Heal = new EventType("heal");

        /// An [Actor] was frightened.
        public static EventType Fear = new EventType("fear");

        /// An [Actor] regained their courage.
        public static EventType Courage = new EventType("courage");

        /// Something in the level was detected.
        public static EventType Detect = new EventType("detect");

        /// An [Actor] teleported.
        public static EventType Teleport = new EventType("teleport");

        /// A new [Actor] was spawned by another.
        public static EventType Spawn = new EventType("spawn");

        /// A tile has been hit by sound.
        public static EventType Howl = new EventType("howl");

        /// A warrior's slash attack hits a tile.
        public static EventType Slash = new EventType("slash");

        /// A warrior's stab attack hits a tile.
        public static EventType Stab = new EventType("stab");

        /// The hero picks up gold worth [other].
        public static EventType Gold = new EventType("gold");

        public EventType(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}