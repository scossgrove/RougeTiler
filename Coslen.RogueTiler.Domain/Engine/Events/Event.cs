using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Events
{
    /// Describes a single "interesting" thing that occurred during a call to
    /// [Game.update()]. In general, events correspond to things that a UI is likely
    /// to want to display visually in some form.
    public class Event
    {
        public Event(EventType type, Actor actor, Element element, VectorBase pos, Direction dir, object other)
        {
            Type = type;
            Actor = actor;
            Element = element;
            Pos = pos;
            Dir = dir;
            Other = other;
        }

        public Actor Actor { get; set; }
        public Direction Dir { get; set; }
        public Element Element { get; set; }
        public object Other { get; set; }
        public VectorBase Pos { get; set; }
        public EventType Type { get; set; }
    }
}