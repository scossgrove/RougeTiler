using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public abstract class Action
    {
        private List<Action> actions;
        public Actor Actor { get; private set; }

        public Monster Monster
        {
            get { return Actor as Monster; }
        }

        public Game Game { get; private set; }
        public GameResult gameResult { get; set; }
        public bool ConsumesEnergy { get; set; }

        public Hero hero
        {
            get { return Actor as Hero; }
        }

        /// How much noise is produced by this action. Override to make certain
        /// actions quieter or louder.
        public int Noise
        {
            get { return Option.NoiseNormal; }
        }

        public void Bind(Actor actor, bool consumesEnergy)
        {
            if (actor == null)
            {
                throw new ArgumentNullException();
            }

            Actor = actor;
            Game = actor.Game;
            ConsumesEnergy = consumesEnergy;
        }

        public ActionResult Perform(List<Action> actions, GameResult gameResult)
        {
            // Action should be bound already.
            if (Actor == null)
            {
                throw new ArgumentNullException();
            }

            this.actions = actions;
            this.gameResult = gameResult;

            return OnPerform();
        }

        public abstract ActionResult OnPerform();

        /// Enqueue a secondary action that is a consequence of this one.
        public void AddAction(Action action, Actor actor = null)
        {
            action.Bind(actor ?? Actor, false);
            actions.Add(action);
        }

        public void AddEvent(EventType type, Actor actor, object other)
        {
            gameResult.AddEvent(type, actor, ElementFactory.Instance.None, null, null, other);
        }

        public void AddEvent(EventType type, Actor actor)
        {
            gameResult.AddEvent(type, actor, ElementFactory.Instance.None, null, null, null);
        }

        public void AddEvent(EventType type, Actor actor, Element element, VectorBase pos, Direction dir, object other)
        {
            gameResult.AddEvent(type, actor, element, pos, dir, other);
        }

        public void Error(params string[] nouns)
        {
            if (!Actor.IsVisible)
            {
                return;
            }
            var message = nouns.First();
            var nounList = nouns.ToList();
            nounList.RemoveAt(0);
            Game.Log.Message(message, nounList.Select(n => new Noun(n)).ToArray());
        }

        public void Log(string message, params Noun[] nouns)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            nounList.AddRange(nouns.Select(x=>x.NounText));
            Log(nounList.ToArray());
        }

        public void Log(string message, params string[] nouns)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            nounList.AddRange(nouns);
            Log(nounList.ToArray());
        }

        public void Log(params string[] nouns)
        {
            if (!Actor.IsVisible)
            {
                return;
            }
            var message = nouns.First();
            var nounList = nouns.ToList();
            nounList.RemoveAt(0);
            Game.Log.Message(message, nounList.Select( n => new Noun(n)).ToArray());
        }

        public ActionResult Succeed(string message, params Noun[] nouns)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            nounList.AddRange(nouns.Select(x => x.NounText));
            return Succeed(nounList.ToArray());
        }


        public ActionResult Succeed(string message, params string[] nouns)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            nounList.AddRange(nouns);
            return Succeed(nounList.ToArray());
        }

        public ActionResult Succeed(string message)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            return Succeed(nounList.ToArray());
        }

        public ActionResult Succeed(params string[] nouns)
        {
            if (nouns.Any())
            {
                Log(nouns);
            }
            return ActionResult.Success;
        }

        public ActionResult Fail(string message, params Noun[] nouns)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            nounList.AddRange(nouns.Select(x => x.NounText));
            return Fail(nounList.ToArray());
        }

        public ActionResult Fail(string message, params string[] nouns)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            nounList.AddRange(nouns);
            return Fail(nounList.ToArray());
        }

        public ActionResult Fail(string message)
        {
            List<string> nounList = new List<string>();
            nounList.Add(message);
            return Fail(nounList.ToArray());
        }

        public ActionResult Fail(params string[] nouns)
        {
            if (nouns.Any())
            {
                Error(nouns);
            }
            return ActionResult.Failure;
        }

        public ActionResult alternate(Action action)
        {
            action.Bind(Actor, ConsumesEnergy);
            return new ActionResult(action);
        }

        /// Returns [success] if [done] is `true`, otherwise returns [notDone].
        public ActionResult DoneIf(bool done)
        {
            return done ? ActionResult.Success : ActionResult.NotDone;
        }
    }

    public class ActionResult
    {
        public static ActionResult Success = new ActionResult(true, true);
        public static ActionResult Failure = new ActionResult(false, true);
        public static ActionResult NotDone = new ActionResult(true, false);

        /// An alternate [Action] that should be performed instead of the one that
        /// failed to perform and returned this. For example, when the [Hero] walks
        /// into a closed door, the [WalkAction] will fail (the door is closed) and
        /// return an alternate [OpenDoorAction] instead.
        public Action alternative;

        /// `true` if the [Action] does not need any further processing.
        public bool done;

        /// `true` if the [Action] was successful and energy should be consumed.
        public bool succeeded;

        public ActionResult(bool succeeded, bool done)
        {
            this.succeeded = succeeded;
            this.done = done;
            alternative = null;
        }

        public ActionResult(Action alternative)
        {
            this.alternative = alternative;
            succeeded = false;
            done = true;
        }
    }
}