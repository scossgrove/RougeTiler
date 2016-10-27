using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Spawns a new [Monster] of a given [Breed].
    internal class SpawnAction : Action
    {
        private readonly Breed _breed;
        private readonly VectorBase _pos;

        public SpawnAction(VectorBase pos, Breed breed)
        {
            _pos = pos;
            _breed = breed;
        }

        public override ActionResult OnPerform()
        {
            // There's a chance the move will do nothing (except burn charge) based on
            // the monster's generation. This is to keep breeders from filling the
            // dungeon.
            if (!Rng.Instance.OneIn(Monster.Generation))
            {
                return ActionResult.Success;
            }

            // Increase the generation on the spawner too so that its rate decreases
            // over time.
            Monster.Generation++;

            var spawned = _breed.Spawn(Game, _pos, Monster);
            Game.CurrentStage.AddActor(spawned);

            AddEvent(EventType.Spawn, spawned, ElementFactory.Instance.None, null, null, null);

            // TODO: Message?
            return ActionResult.Success;
        }
    }
}