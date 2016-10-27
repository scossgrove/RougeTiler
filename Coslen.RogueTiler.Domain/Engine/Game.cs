using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Logging;
using Coslen.RogueTiler.Domain.Engine.StageBuilders;
using Coslen.RogueTiler.Domain.Utilities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// <summary>
    ///     Root class for the game engine. All game state is contained within this.
    /// </summary>
    [Serializable]
    public class Game
    {
        /// <summary>
        /// This is the set of actions that are to be performed for this update loop
        /// </summary>
        public List<Action> Actions { get; set; }

        /// <summary>
        /// This is the current target for the current actor (Monster/Hero)
        /// </summary>
        public Actor Target
        {
            get
            {
                // Make sure the target is still valid.
                if (_target != null)
                {
                    if (!_target.IsAlive || !_target.IsVisible)
                    {
                        _target = null;
                    }
                }

                return _target;
            }
            set
            {
                if (_target == value)
                {
                    return;
                }
                _target = value;
            }
        }

        private Actor _target;

        /// <summary>
        /// This is the logger for the system.
        /// </summary>
        public Log Log { get; set; }

        /// <summary>
        /// This is the current Stage data.
        /// 
        /// It contains the following information:
        /// 1. Map of Stage
        /// 2. Hero Information
        /// </summary>
        public Stage CurrentStage { get; private set; }

        /// <summary>
        /// This is the object that represents the hero.
        /// 
        /// It contains information regarding:
        /// 1. Health
        /// 2. Gold
        /// 3. Class
        /// 4. Masteries
        /// 5. basically the state of the hero.
        /// </summary>
        public Hero Hero { get; set; }

        /// <summary>
        /// This is the persistance mechanism for all game data.
        /// </summary>
        private Storage _storage;

        public Storage Storage
        {
            get
            {
                if (_storage == null)
                {
                    _storage = new Storage(GameContent.Instance);
                    _storage.Load();
                }
                return _storage;
            }
        }

        public Game(string heroName)
        {
            Log = new Log();
            Actions = new List<Action>();
            LoadGameData(heroName);
        }

        public void LoadGameData(string heroName)
        {
            var heroSave = Storage.GetHero(heroName);
            try
            {
                CurrentStage = StageFactory.LoadStage(this, heroName, heroSave.CurrentStage);
                CurrentStage.RefreshFieldOfView(CurrentStage.CurrentHero.Position);
            }
            catch (StageNotFoundExpection ex)
            {
                throw;
            }
        }

        /// <summary>
        ///     This is the internal game loop for processing the stage.
        /// </summary>
        /// <returns></returns>
        public GameResult Update()
        {
            var debugger = Debugger.Instance;
            var gameResult = new GameResult();

            while (true)
            {
                // Process any ongoing or pending actions.
                while (Actions.Any())
                {
                    var action = Actions.First();
                    debugger.LogToDisk($"  - Processing Action: {action} for actor[{action.Actor.NounText}]");

                    // Cascade through the alternates until we hit bottom out.
                    var result = action.Perform(Actions, gameResult);

                    while (result.alternative != null)
                    {
                        Actions.RemoveAt(0);
                        action = result.alternative;
                        Actions.Insert(0, action);

                        debugger.LogToDisk($"  - Alternate Action: {action}");
                        result = action.Perform(Actions, gameResult);
                    }

                    CurrentStage.refreshVisibility(Hero);

                    gameResult.MadeProgress = true;

                    if (result.done)
                    {
                        Actions.RemoveAt(0);

                        if (result.succeeded && action.ConsumesEnergy)
                        {
                            debugger.LogToDisk($"  - Actor Finish Turn: {action.Actor}");
                            action.Actor.FinishTurn(action);
                            CurrentStage.advanceActor();
                        }

                        // Refresh every time the hero takes a turn.
                        if (action.Actor == Hero)
                        {
                            return gameResult;
                        }
                    }

                    if (gameResult.Events.Count > 0)
                    {
                        return gameResult;
                    }
                }

                // If we get here, all pending actions are done, so advance to the next
                // tick until an actor moves.
                while (!Actions.Any())
                {
                    var actor = CurrentStage.currentActor;
                    debugger.LogToDisk($"  * Processing Current Actor : {actor} / Location [{actor.Position.x},{actor.Position.y}] ");

                    if (actor is Hero)
                    {
                        gameResult.IsPlayerTurn = true;
                    }

                    // If we are still waiting for input for the actor, just return (again).
                    if (actor.Energy.CanTakeTurn() && actor.NeedsInput)
                    {
                        debugger.LogToDisk("  * Current Actor : Needs Input");
                        gameResult.GetPlayerInput = true;
                        return gameResult;
                    }

                    if (actor.Energy.CanTakeTurn() || actor.Energy.Gain(actor.Speed))
                    {
                        // If the actor can move now, but needs input from the user, just
                        // return so we can wait for it.
                        if (actor.NeedsInput)
                        {
                            debugger.LogToDisk("  * Current Actor : Needs Input");
                            return gameResult;
                        }

                        var newAction = actor.GetAction();
                        debugger.LogToDisk($"  * Current Actor : Adding Action {newAction}");
                        Actions.Add(newAction);
                    }
                    else
                    {
                        debugger.LogToDisk($"  * Current Actor : skipping turn / Energy : {actor.Energy.CurrentEnergy} of {Energy.ActionCost}");
                        // This actor doesn't have enough energy yet, so move on to the next.
                        CurrentStage.advanceActor();
                    }

                    // Each time we wrap around, process "idle" things that are ongoing and
                    // speed independent.
                    if (actor == Hero)
                    {
                        TrySpawnMonster();
                    }
                }
            }
        }

        /// Over time, new monsters will appear in unexplored areas of the dungeon.
        /// This is to encourage players to not waste time: the more they linger, the
        /// more dangerous the remaining areas become.
        public void TrySpawnMonster()
        {
            if (!Rng.Instance.OneIn(Option.SpawnMonsterChance))
            {
                return;
            }

            // Try to place a new monster in unexplored areas.
            VectorBase pos = Rng.Instance.vectorInRect(CurrentStage.Bounds());

            var tile = CurrentStage[pos];
            if (tile.Visible || tile.IsExplored || !tile.IsPassable)
            {
                return;
            }
            if (CurrentStage.ActorAt(pos) != null)
            {
                return;
            }
            var breedForMonster = MonsterFactory.GenerateBreedOfMonster(CurrentStage.StageNumber);
            CurrentStage.SpawnMonster(this, breedForMonster, pos);
        }
    }
}