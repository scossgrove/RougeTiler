using System;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class WalkAction : Action
    {
        private readonly VectorBase offset;

        public WalkAction(VectorBase offset)
        {
            this.offset = offset;
        }

        public override ActionResult OnPerform()
        {
            // Rest if we aren't moving anywhere.
            if (Vector.zero == offset)
            {
                return alternate(new RestAction());
            }

            var pos = Actor.Position + offset;

            // See if there is an actor there.
            var target = Game.CurrentStage.ActorAt(pos);
            if (target != null && target != Actor)
            {
                return alternate(new AttackAction(target));
            }

            // See if it's a door.
            var tile = Game.CurrentStage[pos].Type;
            if (tile.OpensTo != null)
            {
                return alternate(new OpenDoorAction(pos));
            }

            if (tile.AlternateAction != null)
            {
                return alternate(tile.AlternateAction);
            }

            // See if we can walk there.
            if (!Actor.CanOccupy(pos))
            {
                return Fail("{1} hit[s] the {2}.", Actor.NounText, tile.DisplayName);
            }

            Actor.Position = pos;

            // See if the hero stepped on anything interesting.
            if (Actor is Hero)
            {
                foreach (var item in Game.CurrentStage.itemsAt(pos))
                {
                    hero.Disturb();

                    // Treasure is immediately, freely acquired.
                    if (item.isTreasure)
                    {
                        // Pick a random value near the price.
                        var min = (int) Math.Ceiling(item.price*0.5);
                        var max = (int) Math.Ceiling(item.price*1.5);
                        var value = Rng.Instance.Range(min, max);
                        hero.Gold += value;
                        Log("{1} pick[s] up {2} worth " + value + " gold.", hero, item);
                        Game.CurrentStage.removeItem(item);
                        item.SetActive(false);


                        gameResult.AddEvent(EventType.Gold, Actor, ElementFactory.Instance.None, Actor.Position, null, item);
                    }
                    else
                    {
                        Log("{1} [are|is] standing on {2}.", Actor, item);
                    }
                }
            }

            return Succeed();
        }

        public override string ToString()
        {
            return Actor + " walks " + offset;
        }
    }
}