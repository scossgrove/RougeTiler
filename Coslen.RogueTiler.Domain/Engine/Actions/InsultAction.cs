using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    internal class InsultAction : Action
    {
        private readonly Actor target;

        public InsultAction(Actor target)
        {
            this.target = target;
        }

        public override ActionResult OnPerform()
        {
            var insults = new List<string> {"{1} insult[s] {2 his} mother!", "{1} jeer[s] at {2}!", "{1} mock[s] {2} mercilessly!", "{1} make[s] faces at {2}!"};
            var message = Rng.Instance.Item(insults);

            return Succeed(message, Actor, target);
        }
    }
}