using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine.Environment
{
    /// Abstract class for a stage generator. An instance of this encapsulates
    /// some dungeon generation algorithm. These are implemented in content.
    /// Describes one level in a [Area]. When the [Hero] enters a [Level] for an
    /// area, this determines how that specific level is generated.
    public class Level
    {
        public QuestBuilder quest { get; set; }

        public Level(Action<Stage> buildStage, int? numMonsters, int? numItems, Drop floorDrop, List<Breed> breeds, QuestBuilder quest)
        {
            BuildStage = buildStage;
            NumMonsters = numMonsters;
            NumItems = numItems;
            FloorDrop = floorDrop;
            Breeds = breeds;
            this.quest = quest;
        }

        public Action<Stage> BuildStage { get; set; }

        public Drop FloorDrop { get; set; }

        public int? NumMonsters { get; set; }
        public int? NumItems { get; set; }

        /// The [Breed]s that appear in this [Level].
        public List<Breed> Breeds { get; set; }
    }
}