using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.UIConnector;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.UI.CommonDialogs;
using Coslen.RogueTiler.Win.UI.InGame.Dialogs;
using Coslen.RogueTiler.Win.UI.InGame.Helpers;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Win.UI.InGame
{
    public class InGameScreen : ScreenBase
    {
        private readonly Debugger _debugger;
        private GameState _gameState;
        private readonly string _heroName;
        
        public InGameScreen(Storage storage, string heroName)
        {
            Storage = storage;
            this._heroName = heroName;
            _debugger = Debugger.Instance;

            GameState.Instance.Reset();
        }

        public Storage Storage { get; }
        public InGameLayout Layout { get; set; }

        public override void Setup()
        {
            // Adjust the current console
            SetUpConsoleSize();

            // Game State
            _gameState = GameState.Instance;

            LoadGame();
        }

        private void LoadGame()
        {
            // Load the current Hero State
            _gameState.HeroSave = Storage.GetHero(_heroName);

            if (_gameState.Game == null || _gameState.Game.CurrentStage == null)
            {
                _gameState.Game = new Game(_heroName);
            }

            // create the layout
            Layout = new InGameLayout(_gameState.Game);
        }

        /// <summary>
        ///     The return is to exit the Game
        /// </summary>
        /// <returns></returns>
        public override bool HandleInput()
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Black;

            var inputs = Inputs.Instance;

            var lastInput = GetPlayerInput();

            Console.ForegroundColor = originalColor;

            var exitGameLoop = false;

            Action action = null;

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            // Player Actions.
            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            if (lastInput == inputs.forfeit)
            {
                var dialog = new ForfeitDialog(_gameState.Game);
                dialog.Process();
                var result = dialog.DialogResult;
                if (result == ForfeitDialogResult.Yes)
                {
                    ScreenResult = new ScreenTransitionResult { FromScreen = ScreenType.InGame, ToScreen = ScreenType.MainMenu, Result = null };
                    Storage.Heroes.RemoveAll(x => x.Name == _gameState.Game.Hero.Name);
                    Storage.Save();
                    exitGameLoop = true;
                }
            } // Input.forfeit,
            else if (lastInput == inputs.quit)
            {
                var dialog = new ConfirmDialog("Are you sure you want to quit the game?");
                dialog.Process();
                var result = dialog.DialogResult;
                if (result == ConfirmDialogResult.Yes)
                {
                    ScreenResult = new ScreenTransitionResult {FromScreen = ScreenType.InGame, ToScreen = ScreenType.MainMenu, Result = null};
                    Storage.Update(_gameState);

                    Storage.Save();
                    exitGameLoop = true;
                }
            } // Input.quit,
            else if (lastInput == inputs.closeDoor)
            {
                // See how many adjacent open doors there are.
                var doors = new List<VectorBase>();
                foreach (var direction in Direction.All)
                {
                    var pos = _gameState.Game.Hero.Position + direction;
                    if (_gameState.Game.CurrentStage[pos].Type.ClosesTo != null)
                    {
                        doors.Add(pos);
                    }
                }

                if (doors.Count == 0)
                {
                    _gameState.Game.Log.Error("You are not next to an open door.");
                }
                else if (doors.Count == 1)
                {
                    _gameState.Game.Hero.SetNextAction(new CloseDoorAction(doors[0]));
                }
                else
                {
                    var closeDoorDialog = new CloseDoorDialog(_gameState.Game);
                    closeDoorDialog.Process();
                }
            } // Input.closeDoor,
            else if (lastInput == inputs.drop)
            {
                ShowItemDialog(ItemDialogUsage.Drop);
            } // Input.drop,
            else if (lastInput == inputs.use)
            {
                ShowItemDialog(ItemDialogUsage.Use);
            } // Input.use,
            else if (lastInput == inputs.pickUp)
            {
                var items = _gameState.Game.CurrentStage.itemsAt(_gameState.Game.Hero.Position);

                if (items.Count > 1)
                {
                    // Show item dialog if there are multiple things to pick up.
                    ShowItemDialog(ItemDialogUsage.PickUp);
                }
                else
                {
                    // Otherwise attempt to pick up any available item.
                    _gameState.Game.Hero.SetNextAction(new PickUpAction(items.Count - 1));
                }
            } // Input.pickUp,
            else if (lastInput == inputs.swap)
            {
                if (_gameState.Game.Hero.Inventory.lastUnequipped == -1)
                {
                    _gameState.Game.Log.Error("You aren't holding an unequipped item to swap.");
                }
                else
                {
                    action = new EquipAction(ItemLocations.Inventory, _gameState.Game.Hero.Inventory.lastUnequipped);
                }
            } // Input.swap,
            else if (lastInput == inputs.toss)
            {
                ShowItemDialog(ItemDialogUsage.Toss);
            } // Input.toss,
            else if (lastInput == inputs.selectCommand)
            {
                var commands = _gameState.Game.Hero.HeroClass.Commands.Where(cmd => cmd.CanUse(_gameState.Game));

                if (!commands.Any())
                {
                    _gameState.Game.Log.Error("You don't have any commands you can perform.");
                }
                else
                {
                    Command commandToProcess = null;
                    if (commands.Count() > 1)
                    {
                        var dialog = new SelectCommandDialog(_gameState.Game);
                        dialog.Process();
                        commandToProcess = dialog.DialogResult;
                    }
                    else
                    {
                        commandToProcess = commands.First();
                    }

                    if (commandToProcess is TargetCommand)
                    {
                        var targetCommand = commandToProcess as TargetCommand;

                        // If we still have a visible target, use it.
                        if (_gameState.Game.Target != null && _gameState.Game.Target.IsAlive && _gameState.Game.CurrentStage[_gameState.Game.Target.Position].Visible)
                        {
                            _gameState.Game.Hero.SetNextAction(targetCommand.GetTargetAction(_gameState.Game, _gameState.Game.Target.Position));
                        }
                        else
                        {
                            // No current target, so ask for one.
                            var targetDialog  = new TargetDialog(targetCommand.GetRange(_gameState.Game), (target) => _gameState.Game.Hero.SetNextAction(targetCommand.GetTargetAction(_gameState.Game, target)), _gameState.Game);
                            targetDialog.Process();
                        }
                    }
                    else if (commandToProcess is DirectionCommand)
                    {
                        var directionCommand = commandToProcess as DirectionCommand;
                        var directionDialog = new DirectionDialog(directionCommand, _gameState.Game);
                        directionDialog.Process();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            } // Input.selectCommand,

            else if (lastInput == inputs.stats)
            {
                ShowHeroStatisticsDialog();
            } // Input.HeroStatistics,
            else if (lastInput.Key == ConsoleKey.Z)
            {
                ShowStorageDialog();
            } // Storage
            else if (lastInput.Key == ConsoleKey.Oem3)
            {
                ShowConsoleDialog();
            } // Storage

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            // Running Options
            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            else if (lastInput == inputs.runNW) { _gameState.Game.Hero.Run(Direction.NorthWest); } // Input.runNW,
            else if (lastInput == inputs.runN) { _gameState.Game.Hero.Run(Direction.North); } // Input.runN,
            else if (lastInput == inputs.runNE) { _gameState.Game.Hero.Run(Direction.NorthEast); } // Input.runNE,
            else if (lastInput == inputs.runW) { _gameState.Game.Hero.Run(Direction.West); } // Input.runW,
            else if (lastInput == inputs.runE) { _gameState.Game.Hero.Run(Direction.East); } // Input.runE, SemiColon
            else if (lastInput == inputs.runSW) { _gameState.Game.Hero.Run(Direction.SouthWest); } // Input.runSW,
            else if (lastInput == inputs.runS) { _gameState.Game.Hero.Run(Direction.South); } // Input.runS, 
            else if (lastInput == inputs.runSE) { _gameState.Game.Hero.Run(Direction.SouthEast); } // Input.runSE, Slash (FWD)

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            // Firing Options
            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            else if (lastInput == inputs.fireNW) { FireTowards(Direction.NorthWest); } // Input.fireNW,
            else if (lastInput == inputs.fireN) { FireTowards(Direction.North); } // Input.fireN,
            else if (lastInput == inputs.fireNE) { FireTowards(Direction.NorthEast); } // Input.fireNE,
            else if (lastInput == inputs.fireW) { FireTowards(Direction.West); } // Input.fireW,
            else if (lastInput == inputs.fireE) { FireTowards(Direction.East); } // Input.fireE,
            else if (lastInput == inputs.fireSW) { FireTowards(Direction.SouthWest); } // Input.fireSW,
            else if (lastInput == inputs.fireS) { FireTowards(Direction.South); } // Input.fireS,
            else if (lastInput == inputs.fireSE) { FireTowards(Direction.SouthEast); } // Input.fireSE,

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            // directions.
            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            else if (lastInput == inputs.nw) { action = new WalkAction(Direction.NorthWest); } // Input.nw,
            else if (lastInput == inputs.n) { action = new WalkAction(Direction.North); } // Input.n,
            else if (lastInput == inputs.ne) { action = new WalkAction(Direction.NorthEast); } // Input.ne,
            else if (lastInput == inputs.w) { action = new WalkAction(Direction.West); } // Input.w,
            else if (lastInput == inputs.rest) { action = new RestAction(); } // Input.l,
            else if (lastInput == inputs.e) { action = new WalkAction(Direction.East); } // Input.e,
            else if (lastInput == inputs.sw) { action = new WalkAction(Direction.SouthWest); } // Input.sw,
            else if (lastInput == inputs.s) { action = new WalkAction(Direction.South); } // Input.s,
            else if (lastInput == inputs.se) { action = new WalkAction(Direction.SouthEast); } // Input.se, 

            //Check if we have a non-zero value for horizontal or vertical                                   
            if (action != null)
            {
                //UnityEngine.Debugger.Log("Adding action for Hero");
                GameState.Instance.Game.Hero.SetNextAction(action);
            }

            return exitGameLoop;
        }

        private void ShowConsoleDialog()
        {
            var targetDialog = new ConsoleDialog(_gameState.Game);
            targetDialog.Process();
        }

        public override void Draw()
        {
            LoadLayoutData();
         
            var buffer = new BufferContainer(0, 0, (short) ScreenHeight, (short) ScreenWidth);
            Layout.Draw(buffer);
        }

        private void LoadLayoutData()
        {
            // Update the Log Section
            var history = Layout.LayoutSections["History"] as TextListScreenSection;
            if (GameState.Instance.Game.Log.Messages.Any())
            {
                var totalMessages = GameState.Instance.Game.Log.Messages.Count;
                for (var messageIndex = 0; messageIndex < totalMessages; messageIndex++)
                {
                    history.Lines.Insert(0, GameState.Instance.Game.Log.Messages.First().Text);
                    GameState.Instance.Game.Log.Messages.Dequeue();
                }

                for (var messageIndex = history.Lines.Count - 1; messageIndex > 10; messageIndex--)
                {
                    history.Lines.RemoveAt(messageIndex);
                }
            }

            // Update the Main Game UI Segment
            var gameBoard = Layout.LayoutSections["Game Board"] as GameBoardScreenSection;
            gameBoard.Tiles = _gameState.Game.CurrentStage.Appearances;
            gameBoard.HeroPosition = _gameState.Game.CurrentStage.LastHeroPosition;

        }

        public void Dirty()
        {
            
        }

        public override void ScreenLoop()
        {
            var exitGameLoop = false;
            var gameLoopCounter = 0;

            do
            {
                var startOfLoop = DateTime.Now;
                _debugger.LogToDisk($"Game Loop Number: {gameLoopCounter}", LogLevel.Debug);

                var startOfDrawing = DateTime.Now;
                Draw();
                var endOfDrawing = DateTime.Now;

                // TODO: Add the effects into the game
                //if (Effects.length > 0) dirty();

                // Process any ongoing or pending actions.
                var startOfUpdate = DateTime.Now;
                var gameResult = _gameState.Game.Update();
                var endOfUpate = DateTime.Now;

                if (!_gameState.Game.Hero.IsAlive)
                {
                    ScreenResult = new ScreenTransitionResult { FromScreen = ScreenType.InGame, ToScreen = ScreenType.GameOver, Result = null };
                    Storage.Heroes.RemoveAll(x => x.Name == _gameState.Game.Hero.Name);
                    Storage.Save();
                    exitGameLoop = true;
                }

                if (gameResult.ChangeLevel)
                {
                    Storage.Update(_gameState);
                    _gameState.HeroSave.CurrentStage = gameResult.NewLevel;
                    _gameState.HeroSave.Position = new VectorBase(0,0); // this should not be possible

                    // Save the game
                    Storage.Save();

                    _gameState.Game = null;

                    LoadGame();

                    // Refresh the UI
                    Draw();
                }

                // TODO: game logic events
                //            if (game.hero.dazzle.isActive) dirty();

                //            for (final event in result.events) addEffects(_effects, event);

                //if (result.needsRefresh) dirty();

                //    _effects = _effects.where((effect) => effect.update(game)).toList();
                //    _positionCamera();

                var startOfInput = DateTime.Now;
                if (gameResult.IsPlayerTurn && gameResult.GetPlayerInput)
                {
                    exitGameLoop = HandleInput();
                }
                var endOfInput = DateTime.Now;

                var endOfLoop = DateTime.Now;

                // Debug the Loop Statistics
                var timeTakenForLoop = endOfLoop - startOfLoop;

                var timeTakenToDraw = endOfDrawing - startOfDrawing;
                var percentOfLoopToDraw = timeTakenToDraw.TotalMilliseconds/timeTakenForLoop.TotalMilliseconds;

                var timeTakenToInput = endOfInput - startOfInput;
                var percentOfLoopToInput = timeTakenToInput.TotalMilliseconds/timeTakenForLoop.TotalMilliseconds;

                var timeTakenToUpdate = endOfUpate - startOfUpdate;
                var percentOfLoopToUpdate = timeTakenToUpdate.TotalMilliseconds/timeTakenForLoop.TotalMilliseconds;


                _debugger.LogToDisk($"  ? Time for Loop: {timeTakenForLoop.TotalMilliseconds}", LogLevel.Info);
                _debugger.LogToDisk($"  ? Time for Draw: {timeTakenToDraw.TotalMilliseconds}, percent of loop: {ToPercentage(percentOfLoopToDraw)}", LogLevel.Info);
                _debugger.LogToDisk($"  ? Time for Update: {timeTakenToUpdate.TotalMilliseconds}, percent of loop: {ToPercentage(percentOfLoopToUpdate)}", LogLevel.Info);
                _debugger.LogToDisk($"  ? Time for Input: {timeTakenToInput.TotalMilliseconds}, percent of loop: {ToPercentage(percentOfLoopToInput)}", LogLevel.Info);


                //var timerMessage = new StringBuilder();
                //timerMessage.Append($"  ? {gameLoopCounter},");
                //timerMessage.Append($"{timeTakenForLoop.TotalMilliseconds},");
                //timerMessage.Append($"{timeTakenToDraw.TotalMilliseconds},{ToPercentage(percentOfLoopToDraw)},");
                //timerMessage.Append($"{timeTakenToUpdate.TotalMilliseconds},{ToPercentage(percentOfLoopToUpdate)},");
                //timerMessage.Append($"{timeTakenToInput.TotalMilliseconds},{ToPercentage(percentOfLoopToInput)}");
                //debugger.LogToDisk(timerMessage.ToString(), LogLevel.Info);

                gameLoopCounter++;
            } while (exitGameLoop == false);
        }

        private double ToPercentage(double source)
        {
            return Math.Floor(source*10000)/100;
        }
        
        private void ShowHeroStatisticsDialog()
        {
            var dialog = new HeroStatisicsDialog(_gameState.Game);
            dialog.Process();
        }

        private void ShowStorageDialog()
        {
            var dialog = new StorageDialog(_gameState.Game);
            dialog.Process();
        }

        private void ShowItemDialog(ItemDialogUsage usage)
        {
            var itemDialog = new ItemDialog(_gameState.Game);
            switch (usage)
            {
                case ItemDialogUsage.Drop:
                {
                    itemDialog.Drop();
                    break;
                }
                case ItemDialogUsage.PickUp:
                {
                    itemDialog.PickUp();
                    break;
                }
                case ItemDialogUsage.Toss:
                {
                    itemDialog.Toss();
                    break;
                }
                case ItemDialogUsage.Use:
                {
                    itemDialog.Use();
                    break;
                }
                default:
                {
                    throw new ApplicationException("Invalid/Unhandled ItemDialogUsage");
                }
            }

            itemDialog.Process();
            var result = itemDialog.DialogResult;
            if (result == null)
            {
                return;
            }

            if (result.RequiresTargetDialog)
            {
                // This will clear the screen of anything the dialog overwrote.
                Draw();
                var targetDialog = new TargetDialog(result.Item.type.tossAttack.Range, (target) => result.Command.SelectItem(_gameState.Game, result.Item, result.Location, result.ItemIndex, target), _gameState.Game);
                targetDialog.Process();
            }
            else
            {
                result.Command.SelectItem(_gameState.Game, result.Item, result.Location, result.ItemIndex, null);
            }
        }

        private void FireTowards(Direction dir)
        {
            // TODO: When there is more than one usable command, bring up the
            // SelectCommandDialog. Until then, just pick the first valid one.
            var command = _gameState.Game.Hero.HeroClass.Commands.FirstOrDefault(c => c.CanUse(_gameState.Game));

            if (command == null)
            {
                _gameState.Game.Log.Error("You don't have any commands you can perform.");
                return;
            }

            if (command is DirectionCommand)
            {
                var directionCommand = command as DirectionCommand;
                _gameState.Game.Hero.SetNextAction(directionCommand.GetDirectionAction(_gameState.Game, dir));
                return;
            }

            if (command is TargetCommand)
            {
                var pos = _gameState.Game.Hero.Position + dir;

                // Target the monster that is in the fired direction.
                var los = new Los(_gameState.Game.Hero.Position, pos);
                foreach (var step in los)
                {
                    // Stop if we hit a wall.
                    if (!_gameState.Game.CurrentStage[(VectorBase) step].IsTransparent)
                    {
                        break;
                    }

                    // See if there is an actor there.
                    var actor = _gameState.Game.CurrentStage.ActorAt((VectorBase) step);
                    if (actor != null)
                    {
                        _gameState.Game.Target = actor;
                        break;
                    }
                }

                var targetCommand = command as TargetCommand;
                _gameState.Game.Hero.SetNextAction(targetCommand.GetTargetAction(_gameState.Game, pos));
            }
        }

        private void FireAtTarget(TargetCommand command, Monster target)
        {
            _gameState.Game.Hero.SetNextAction(command.GetTargetAction(_gameState.Game, target.Position));
        }

        #region Stage Generation / Loading Functions

        private void LoadCurrentStageForHero(HeroSave currentHeroState)
        {
            if (!currentHeroState.Stages.Any())
            {
                // Need to build the level.
                BuildStageForHero(currentHeroState);
            }
            else
            {
                LoadStageForHero(currentHeroState);
            }
        }

        private void LoadStageForHero(HeroSave currentHeroState)
        {
            throw new NotImplementedException();

            //if (GameState.Instance.HeroSave == null)
            //{
            //    GameState.Instance.HeroSave = currentHeroState;
            //    GameState.Instance.GameLevel = currentHeroState.CurrentStage;
            //}

            //// Has the game be initialised?
            //if (GameState.Instance.Game == null || GameState.Instance.Game.CurrentStage == null)
            //{
            //    GameState.Instance.Game = new Game(currentHeroState.CurrentStage, currentHeroState);
            //    if (GameState.Instance.Game.CurrentStage.IsDirty)
            //    {
            //        Storage.Save();
            //    }
            //}
            //else
            //{
            //    GameState.Instance.Game.LoadLevel(currentHeroState.CurrentStage, currentHeroState);
            //}
        }

        private void BuildStageForHero(HeroSave currentHeroState)
        {
            throw new NotImplementedException();
            //if (GameState.Instance.HeroSave == null)
            //{
            //    GameState.Instance.HeroSave = currentHeroState;
            //    GameState.Instance.GameLevel = currentHeroState.CurrentStage;
            //}

            //// Has the game be initialised?
            //if (GameState.Instance.Game == null || GameState.Instance.Game.CurrentStage == null)
            //{
            //    GameState.Instance.Game = new Game(currentHeroState.CurrentStage, currentHeroState);
            //    if (GameState.Instance.Game.CurrentStage.IsDirty)
            //    {
            //        currentHeroState.CurrentStage = currentHeroState.CurrentStage;
            //        currentHeroState.Position = GameState.Instance.Game.Hero.Position;
            //        // Todo: find where this shoud have been set already
            //        if (GameState.Instance.Game.CurrentStage.LastHeroPosition == null)
            //        {
            //            GameState.Instance.Game.CurrentStage.LastHeroPosition = currentHeroState.Position;
            //        }

            //        Storage.Save();
            //    }
            //    GameState.Instance.Game.CurrentStage.SetTileExplored(false);
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //    //// reset the current stage
            //    //GameState.Instance.game.CurrentStage = null;
            //    //GameState.Instance.Game.LoadLevel(GameState.Instance.GameLevel);
            //}
        }

        #endregion

    }
}