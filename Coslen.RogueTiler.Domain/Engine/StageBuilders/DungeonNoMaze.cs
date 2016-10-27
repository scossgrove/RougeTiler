using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public abstract class DungeonNoMaze : DungeonBuilder
    {

        public override void AfterGeneration()
        {
            // Randomly switch some tiles around.
            Erode(10000, floor: TileTypeFactory.Instance.Floor, wall: TileTypeFactory.Instance.Wall);
        }

        public override void AddMaze()
        {
            // Connect all of the points together.
            var randomIndex = Rng.Instance.Range(0, Rooms.Count);
            var connected = new List<VectorBase>() { Rooms[randomIndex].GetCenter() };

            var toConnect = new List<VectorBase>();
            toConnect = Rooms.Select(r => r.GetCenter()).ToList();
            toConnect.RemoveAll(x => connected.Contains(x));

            while (toConnect.Any())
            {

                randomIndex = Rng.Instance.Range(0, toConnect.Count);

                var to = toConnect[randomIndex];
                toConnect.RemoveAt(randomIndex);

                connected.Add(to);
                CarvePath(connected[connected.Count - 2], to, TileTypeFactory.Instance.Floor);
            }
        }
    }
}