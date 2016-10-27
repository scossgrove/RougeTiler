using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Events;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// Each call to [Game.update()] will return a [GameResult] object that tells
    /// the UI what happened during that update and what it needs to do.
    public class GameResult
    {
        // This is used to move between levels
        public bool ChangeLevel = false;
        public int NewLevel = 0;

        // This is being used for the game loop
        public bool GetPlayerInput = false;
        public bool IsPlayerTurn = false;

        /// Whether or not any game state has changed. If this is `false`, then no
        /// game processing has occurred (i.e. the game is stuck waiting for user
        /// input for the [Hero]).
        public bool MadeProgress = false;

        /// The "interesting" events that occurred in this update.
        public List<Event> Events { get; set; } = new List<Event>();

        /// Returns `true` if the game state has progressed to the point that a change
        /// should be shown to the user.
        public bool NeedsRefresh
        {
            get { return MadeProgress || Events.Count > 0; }
        }

        public void AddEvent(EventType type, Actor actor, Element element, VectorBase position, Direction direction, object other)
        {
            Events.Add(new Event(type, actor, element, position, direction, other));
        }
    }
}