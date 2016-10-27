using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Domain.Utilities.Configuration;

namespace Coslen.RogueTiler.Domain.Engine
{
    public class Stage
    {
        #region "Properties and Fields"
        /// Tracks global pathfinding distances to the hero, ignoring other actors.
        private Flow _heroPaths;

        //public string[,] DebugMatrix;

        public bool HasExitDown;
        public VectorBase StairDownPosition { get; set; }
        public bool HasExitUp;
        public VectorBase StairUpPosition { get; set; }

        /// <summary>
        /// This determines if a given position is in a shadow or not
        /// </summary>
        public Shadow[,] Shadows;

        /// <summary>
        /// This is the set of tiles that make the map of the stage.
        /// </summary>
        public Tile[,] Tiles;

        /// <summary>
        /// This defines is a given position has been explored or not.
        /// </summary>
        public bool[,] Explored { get; set; }
        
        /// <summary>
        /// This is the number(level) for the stage. It generally represents the
        /// depth of the stage(i.e. how far down or up the stage is).
        /// </summary>
        public int StageNumber { get; set; }

        /// <summary>
        /// This is the hero for the stage.
        /// </summary>
        public Hero CurrentHero { get; set; }

        /// <summary>
        /// This is an indication of the amount of food on this stage
        /// </summary>
        public double Abundance
        {
            get { return _abundance; }
            set
            {
                _abundance = value;
            }
        }

        #endregion

        public Stage(int width, int height)
        {
            Tiles = new Tile[width, height];
            Shadows = new Shadow[width, height];
            Explored = new bool[width, height];
            _actorsByTile = new Actor[width, height];
         
            for (var columnIndex = 0; columnIndex < width; columnIndex++)
            {
                for (var rowIndex = 0; rowIndex < height; rowIndex++)
                {
                    Tiles[columnIndex, rowIndex] = new Tile();
                    Shadows[columnIndex, rowIndex] = new Shadow();
                }
            }
        }

        public Stage(Tile[,] tiles, Shadow[,] shadows, bool[,] explored, List<Item> items, List<Actor> actors, VectorBase lastHeroPosition, double abundance)
        {
            Tiles = tiles;
            Shadows = shadows;
            Explored = explored;

            var width = Tiles.GetUpperBound(0) + 1;
            var height = Tiles.GetUpperBound(1) + 1;
            _actorsByTile = new Actor[width, height];

            foreach (var actor in actors)
            {
                AddActor(actor);
            }

            Items = items;

            LastHeroPosition = lastHeroPosition;

            Abundance = abundance;
        }

        public Tile this[int iCol, int iRow] // Access this matrix as a 2D array
        {
            get { return Tiles[iCol, iRow]; }
            set { Tiles[iCol, iRow] = value; }
        }

        public Tile this[VectorBase target] // Access this matrix as a 2D array
        {
            get { return Tiles[target.x, target.y]; }
            set { Tiles[target.x, target.y] = value; }
        }

        public int Width
        {
            get { return Tiles.GetUpperBound(0) + 1; }
        }

        public int Height
        {
            get { return Tiles.GetUpperBound(1) + 1; }
        }

        /// The total number of tiles the [Hero] can explore in the stage.
        public int numExplorable
        {
            get { return _numExplorable; }
            set
            {
                _numExplorable = value;
            }
        }

        public int numberAlreadyExplored
        {
            get { return GetNumberOfExploredTiles(); }
        }

        public double percentageAlreadyExplored
        {
            get
            {
                var unformatedValue = (double)numberAlreadyExplored / numExplorable;
                unformatedValue = unformatedValue*1000;
                unformatedValue = Math.Floor(unformatedValue);
                unformatedValue = unformatedValue/10;
                return unformatedValue;
            }
        }

        public Rect Bounds()
        {
            // Dont know if this is correct
            var result = new Rect(0, 0, Width, Height);
            return result;
        }

        /// Lazily calculates the paths from every reachable tile to the [Hero]. We
        /// use this to place better and stronger things farther from the Hero. Sound
        /// propagation is also based on this.
        public void RefreshDistances()
        {
            // Don't recalculate if still valid.
            if (CurrentHero == null)
            {
                return;
            }

            if (_heroPaths != null && CurrentHero != null && CurrentHero.Position == _heroPaths.Start)
            {
                return;
            }

            _heroPaths = new Flow(this, CurrentHero.Position, true, true);
        }


        // TODO: This is hackish and may fail to terminate.
        /// Selects a random passable tile that does not have an [Actor] on it.
        public VectorBase FindOpenTile()
        {
            while (true)
            {
                var pos = Rng.Instance.vectorInRect(Bounds());

                if (!IsOpen(pos))
                {
                    continue;
                }

                return pos;
            }
        }

        private bool IsOpen(VectorBase position)
        {
            if (this[position].Type == null)
            {
                return false;
            }

            if (!this[position].IsPassable)
            {
                return false;
            }

            if (ActorAt(position) != null)
            {
                return false;
            }

            return true;
        }

        /// Randomly selects an open tile in the stage. Makes [tries] attempts and
        /// chooses the one most distance from some point. Assumes that [scent2] has
        /// been filled with the distance information for the target point.
        /// 
        /// This is used during level creation to place stronger [Monster]s and
        /// better treasure farther from the [Hero]'s starting location.
        public VectorBase FindDistantOpenTile(int tries = 0)
        {
            RefreshDistances();

            var bestDistance = -1;
            VectorBase best = null;

            for (var i = 0; i < tries; i++)
            {
                var pos = FindOpenTile();
                if (pos == null)
                {
                }
                var distance = _heroPaths.GetDistance(pos);
                if (distance == null)
                {
                }
                if (distance > bestDistance)
                {
                    best = pos;
                    bestDistance = distance.Value;
                }
            }

            if (best == null)
            {
            }
            return best;
        }

        public VectorBase FindDistantOpenTileNear(VectorBase position)
        {
            RefreshDistances();

            if (IsOpen(position))
            {
                return position;
            }

            Circle circle = new Circle(position, 1);

            int circleEdgeCount = 0;
            //while (circle.edge.MoveNext())
            foreach(var point in circle.Points)
            {
                if (IsOpen(point))
                {
                    return point;
                }
            }

            return null;
        }

        /// Called after the level generator has finished laying out the stage.
        public void FinishBuild()
        {
            CalculateExplorableTiles();

            // postponing this til hero is around
            //RefreshFieldOfView(CurrentHero.Position);
        }

        public void SetTileExplored(bool isExplored)
        {
            var matrixWidth = Tiles.GetUpperBound(0);
            var matrixHeight = Tiles.GetUpperBound(1);

            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    Tiles[columnIndex, rowIndex].IsExplored = false;
                    Tiles[columnIndex, rowIndex].SetVisible(false);
                }
            }
        }

        private int GetNumberOfExploredTiles()
        {
            int numberExplored = 0;
            for (var rowIndex = 0; rowIndex < Height; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < Width; columnIndex++)
                {
                    if (Explored[columnIndex, rowIndex])
                    {
                        numberExplored++;
                    }
                }
            }

            return numberExplored;
        }

        public void RefreshFieldOfView(VectorBase position)
        {
            var initialNumberAlreadyExplored = GetNumberOfExploredTiles();
            var lit = new bool[Width, Height];
            var radius = Option.HeroSightRange;
            ShadowCaster.ComputeFieldOfViewWithShadowCasting(position.x, position.y, radius,
                (x1, y1) =>
                {
                    if (this.Bounds().Contains(new VectorBase(x1, y1)))
                    {
                        return !Tiles[x1, y1].IsTransparent;
                    }
                    else
                    {
                        return false;
                    }
                }, // delegate to handle transperancy
                (x2, y2) =>
                {
                    // Need to check against the bounds of the stage
                    if (this.Bounds().Contains(new VectorBase(x2, y2)))
                    {
                        lit[x2, y2] = true;
                        Explored[x2, y2] = true;

                        Tiles[x2, y2].SetVisible(true);
                        Tiles[x2, y2].IsExplored = true;
                    }
                } // delegate to handle visibility
                );
            var changedNumberAlreadyExplored = GetNumberOfExploredTiles();

            // Let the hero know things have been explored.
            CurrentHero.Explore(changedNumberAlreadyExplored - initialNumberAlreadyExplored);

            SetVisibility(lit);
        }

        private void SetVisibility(bool[,] lit)
        {
            for (var rowIndex = 0; rowIndex < Height; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < Width; columnIndex++)
                {
                    if (Shadows[columnIndex, rowIndex] == null)
                    {
                        Shadows[columnIndex, rowIndex] = new Shadow();
                    }

                    var currentShadow = Shadows[columnIndex, rowIndex];
                    // Set shadows
                    if (currentShadow != null)
                    {
                        currentShadow.SetActive(lit[columnIndex, rowIndex]);
                        currentShadow.SetShadow(lit[columnIndex, rowIndex]);
                    }

                    // Set Items
                    var items = itemsAt(new VectorBase(columnIndex, rowIndex));
                    foreach (var item in items)
                    {
                        item.SetActive(lit[columnIndex, rowIndex]);
                    }

                    // Set Actors
                    var actor = ActorAt(new VectorBase(columnIndex, rowIndex));
                    if (actor != null)
                    {
                        actor.SetActive(lit[columnIndex, rowIndex]);
                    }
                }
            }
        }

        #region Monster Functions

        public void SpawnMonster(Game game, Breed breed, VectorBase pos)
        {
            if (ActorAt(pos) != null)
            {
                throw new ApplicationException("Cannot add two actor to the same location");
            }

            var monsters = new List<Monster>();

            var count = Rng.Instance.triangleInt(breed.NumberInGroup, breed.NumberInGroup / 2);
            
            // Place the first monster.
            monsters.Add(AddMonster(game,breed, pos));

            // If the monster appears in groups, place the rest of the groups.
            for (var i = 1; i < count; i++)
            {
                // Find every open tile that's neighboring a monster in the group.
                var open = new List<VectorBase>();
                foreach (var monster in monsters)
                {
                    foreach (var dir in Direction.All)
                    {
                        var neighbor = monster.Position + dir;
                        if (this[neighbor].IsPassable && (ActorAt(neighbor) == null))
                        {
                            open.Add(neighbor);
                        }
                    }
                }

                if (open.Count == 0)
                {
                    // We filled the entire reachable area with monsters, so give up.
                    break;
                }

                open = open.Distinct().ToList();

                monsters.Add(AddMonster(game, breed, Rng.Instance.Item(open)));
            }
        }

        private Monster AddMonster(Game game, Breed breed, VectorBase position)
        {
            if (ActorAt(position) != null)
            {
                throw new ApplicationException("Cannot add two actors at the same spot");
            }
            var monster = breed.Spawn(game, position);
            AddActor(monster);
            return monster;
        }

        #endregion

        #region Hero Functions

        /// Gets the number of tiles to walk from [pos] to the [Hero]'s current
        /// position taking into account which tiles are traversable.
        public int? getHeroDistanceTo(VectorBase pos)
        {
            RefreshDistances();
            return _heroPaths.GetDistance(pos);
        }

        public VectorBase LastHeroPosition;

        #endregion

        #region Visibility Functions

        private bool visibilityDirty = true;
        //private Fov fov;

        public void dirtyVisibility()
        {
            visibilityDirty = true;
        }

        public void refreshVisibility(Hero hero)
        {
            if (visibilityDirty)
            {
                visibilityDirty = false;
                RefreshFieldOfView(hero.Position);
            }
        }

        #endregion

        #region "Items Functions"

        public List<Item> Items = new List<Item>();

        // TODO: Move into Item collection?
        // TODO: What if there are multiple items at pos?
        public Item ItemAt(VectorBase pos)
        {
            foreach (var item in Items)
            {
                if (item.Position == pos)
                {
                    return item;
                }
            }

            return null;
        }

        /// Gets the [Item]s at [pos].
        public List<Item> itemsAt(VectorBase pos)
        {
            return Items.Where(item => item.Position.x == pos.x && item.Position.y == pos.y).ToList();
        }

        /// Removes [item] from the stage. Does nothing if the item is not on the
        /// ground.
        public void removeItem(Item item)
        {
            if (Items.Any(x => x == item))
            {
                Items.Remove(item);
            }
            else
            {
                throw new ApplicationException("Item not found on stage");
            }
            //for (var i = 0; i < Items.Count; i++)
            //{
            //    if (items[i] == item)
            //    {
            //        items.removeAt(i);
            //        return;
            //    }
            //}
        }

        #endregion

        #region "Actor Functions"

        /// A spatial partition to let us quickly locate an actor by tile.
        /// 
        /// This is a performance bottleneck since pathfinding needs to ensure it
        /// doesn't step on other actors.
        private readonly Actor[,] _actorsByTile;

        public List<Actor> Actors = new List<Actor>();
        private int _currentActorIndex;
        private int _numExplorable;
        private double _abundance;

        public Actor currentActor
        {
            get { return Actors[_currentActorIndex]; }
        }

        public Actor ActorAt(VectorBase pos)
        {
            return _actorsByTile[pos.x, pos.y];
        }

        public void AddActor(Actor actor)
        {
            Actors.Add(actor);
            _actorsByTile[actor.Position.x, actor.Position.y] = actor;
        }

        /// Called when an [Actor]'s position has changed so the stage can track it.
        public void moveActor(VectorBase from, VectorBase to)
        {
            if (from != null && to != null)
            {
                var actor = _actorsByTile[from.x, from.y];
                _actorsByTile[from.x, from.y] = null;
                _actorsByTile[to.x, to.y] = actor;
            }
        }

        public void removeActor(Actor actor)
        {
        //    if (_actorsByTile[actor.Position.x, actor.Position.y] != actor)
        //    {
        //        throw new ApplicationException();
        //    }

            var index = Actors.IndexOf(actor);
            if (_currentActorIndex > index)
            {
                _currentActorIndex--;
            }

            Actors.RemoveAt(index);
            _actorsByTile[actor.Position.x, actor.Position.y] = null;

            // This removes the item from the Unity UI
            actor.Destroy();
        }

        public void advanceActor()
        {
            _currentActorIndex = (_currentActorIndex + 1)%Actors.Count;
        }

        #endregion

        #region Persistance Functions

        public static string GetStageFileName(string areaName, int areaLevel)
        {
            var persistanceFolder = GamePathUtilities.GetStreamingAssetsPath() + "/SavedGames";
            var fileName = string.Format("{0}_{1}_{2}.dat", areaName.Replace(" ", "-"), "stage", areaLevel.ToString("00"));
            var fullFileName = string.Format("{0}/{1}", persistanceFolder, fileName);

            return fullFileName;
        }

        public static bool SaveExists(string areaName, int areaLevel)
        {
            var fileName = GetStageFileName(areaName, areaLevel);
            return File.Exists(fileName);
        }

        public bool IsDirty { get; set; } = false;

        #endregion

        #region Appearance Functions

        public Appearence[,] Appearances
        {
            get
            {
                var rawTileAppearances = BuildAppearance();
                var finalAppearanceMatrix = AddRulers(rawTileAppearances);
                return finalAppearanceMatrix;
            }
        }

        

        private Appearence[,] AddRulers(Appearence[,] rawTileAppearances)
        {
            var ruledAppearances = new Appearence[Width + 3, Height + 2];

            for (var rowIndex = 0; rowIndex < Height + 2; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < Width + 3; columnIndex++)
                {
                    ruledAppearances[columnIndex, rowIndex] = new Appearence {Glyph = " ", IsExplored = true, IsHidden = false, IsInShadow = false};
                }
            }

            for (var rowIndex = 0; rowIndex < Height; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < Width; columnIndex++)
                {
                    if (rowIndex == 0)
                    {
                        if (columnIndex%10 == 0)
                        {
                            ruledAppearances[columnIndex + 3, rowIndex] = new Appearence {Glyph = Math.Floor((double) columnIndex/10).ToString(), IsExplored = true, IsHidden = false, IsInShadow = false};
                        }

                        ruledAppearances[columnIndex + 3, rowIndex + 1] = new Appearence {Glyph = (columnIndex%10).ToString(), IsExplored = true, IsHidden = false, IsInShadow = false};

                        ruledAppearances[columnIndex + 3, rowIndex + 2] = new Appearence {Glyph = " ", IsExplored = true, IsHidden = false, IsInShadow = false};
                    }

                    if (columnIndex == 0)
                    {
                        if (rowIndex%10 == 0)
                        {
                            ruledAppearances[columnIndex, rowIndex + 2] = new Appearence {Glyph = Math.Floor((double) rowIndex/10).ToString(), IsExplored = true, IsHidden = false, IsInShadow = false};
                        }

                        ruledAppearances[columnIndex + 1, rowIndex + 2] = new Appearence {Glyph = (rowIndex%10).ToString(), IsExplored = true, IsHidden = false, IsInShadow = false};

                        //ruledAppearances[columnIndex + 2, rowIndex + 3] = new Appearence()
                        //{
                        //    Glyph = " ",
                        //    IsExplored = true,
                        //    IsHidden = false,
                        //    IsInShadow = false
                        //};
                    }

                    ruledAppearances[columnIndex + 3, rowIndex + 2] = rawTileAppearances[columnIndex, rowIndex].Clone();
                }
            }
            return ruledAppearances;
        }

        private void CheckPositionsForMatrix(Appearence[,] source, string point, int xOffset = 0, int yOffset = 0)
        {
            var config = RogueTilerSettings.Settings;
            //bool allowChecks = config.AllowMatrixPositionChecking;
            bool allowChecks = false;
            if (!allowChecks)
            {
                return;
            }

            var debugger = Debugger.Instance;
            var message = string.Empty;

            message = $"Checking Appearing Positions - {point}";
            debugger.Info(message);

            var matrixWidth = source.GetUpperBound(0) + 1;
            var matrixHeight = source.GetUpperBound(1) + 1;

            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    var appearance = GetTileAppearence(columnIndex, rowIndex);

                    if (columnIndex != appearance.Position.x + xOffset || rowIndex != appearance.Position.y + yOffset)
                    {
                        message = $"[{columnIndex.ToString().PadLeft(2, '0')},{rowIndex.ToString().PadLeft(2, '0')}]" + $" [{appearance.Position.x.ToString().PadLeft(2, '0')},{appearance.Position.y.ToString().PadLeft(2, '0')}] Positions Dont Match";
                        debugger.Info(message);
                    }
                }
            }
        }

        private Appearence[,] BuildAppearance()
        {
            var debugger = Debugger.Instance;
            var message = string.Empty;

            var appearanceMatrix = new Appearence[Width, Height];

            for (var rowIndex = 0; rowIndex < Height; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < Width; columnIndex++)
                {
                    var appearance = GetTileAppearence(columnIndex, rowIndex);
                    appearanceMatrix[columnIndex, rowIndex] = appearance.Clone();
                }
            }

            CheckPositionsForMatrix(appearanceMatrix, "After Initial Creation");

            foreach (var item in Items)
            {
                // Only show items if the actor is not on top of the item
                var actor = _actorsByTile[item.Position.x, item.Position.y];
                if (actor == null)
                {
                    if (item is Item)
                    {
                        var appearance = item.Appearance;
                        if (appearance == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        appearanceMatrix[item.Position.x, item.Position.y] = appearance;
                    }
                }
            }
            CheckPositionsForMatrix(appearanceMatrix, "After Item Inclusion");

            for (var rowIndex = 0; rowIndex < Height; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < Width; columnIndex++)
                {
                    var appearance = appearanceMatrix[columnIndex, rowIndex];
                    var tile = Tiles[columnIndex, rowIndex];
                    var shadow = Shadows[columnIndex, rowIndex];

                    appearance.IsExplored = tile.IsExplored;

                    // Basically if they are standing on a tile that
                    // has yet to be explored then they
                    // must be hidden
                    if (!tile.IsExplored)
                    {
                        appearance.IsHidden = true;
                    }

                    appearance.IsInShadow = shadow.IsInShadow;

                    var actor = _actorsByTile[columnIndex, rowIndex];
                    if (actor != null)
                    {
                        if (actor is Monster)
                        {
                            //message =
                            //   $"[{columnIndex.ToString().PadLeft(2, '0')},{rowIndex.ToString().PadLeft(2, '0')}]" +
                            //   $" s = [{shadow.IsInShadow.ToString()[0]}]" +
                            //   $" t(e) [{tile.IsExplored.ToString()[0]}]" +
                            //   $" a(e/h/s) [{appearance.IsExplored.ToString()[0]}/{appearance.IsHidden.ToString()[0]}/{appearance.IsInShadow.ToString()[0]}]" +
                            //   $" [{appearance.Position.x.ToString().PadLeft(2, '0')},{appearance.Position.y.ToString().PadLeft(2, '0')}]";

                            //debugger.Info(message);
                        }
                    }
                }
            }

            CheckPositionsForMatrix(appearanceMatrix, "After Hidding / Shadow Logic");

            return appearanceMatrix;
        }

        public Appearence GetTileAppearence(int columnIndex, int rowIndex)
        {
            Appearence tile = null;

            // Is the tile visible
            var currentTile = Tiles[columnIndex, rowIndex];

            // Is there an actor at that position?
            bool isHandled = false;
            var actor = _actorsByTile[columnIndex, rowIndex];
            if (actor != null)
            {
                if (actor is Hero)
                {
                    var hero = actor as Hero;
                    tile = hero.Appearance;
                    isHandled = true;
                }
                else if (actor is Monster)
                {
                    var distanceToHero = (GameState.Instance.Game.Hero.Position - actor.Position).kingLength();
                    if (distanceToHero <= Option.HeroSightRange || Option.ShowAll)
                    {
                        var monster = actor as Monster;
                        tile = monster.Appearance;
                        isHandled = true;
                    }
                }
                else
                {
                    throw new ApplicationException("have not cast the appearance of the actor");
                }
            }

            if (isHandled == false)
            {
                if (currentTile.Type == null)
                {
                    tile = new Appearence {Glyph = "."};
                }
                else if (currentTile.Type.IsWall)
                {
                    tile = new Appearence {Glyph = currentTile.Type.DebugCharacter, ForeGroundColor = currentTile.Type.Appearance.Fore };
                }
                else if (currentTile.Type.Name == "floor")
                {
                    tile = new Appearence {Glyph = "."};
                }
                else if (currentTile.Type.Name == "grass")
                {
                    tile = new Appearence {Glyph = ".", ForeGroundColor = currentTile.Type.Appearance.Fore };
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(currentTile.Type.DebugCharacter))
                    {
                        tile = new Appearence {Glyph = currentTile.Type.DebugCharacter, ForeGroundColor = currentTile.Type.Appearance.Fore };
                    }
                    else
                    {
                        tile = new Appearence {Glyph = currentTile.Type.Name[0].ToString(), ForeGroundColor = currentTile.Type.Appearance.Fore };
                    }
                }
            }

            if (tile == null)
            {
                System.Diagnostics.Debugger.Break();
            }

            tile.Position = new VectorBase(columnIndex, rowIndex);

            return tile;
        }

        #endregion

        public void CalculateExplorableTiles()
        {
            // Count the explorable tiles. We assume the level is fully reachable, so
            // any traversable tile or tile next to a traversable one is explorable.
            numExplorable = 0;

            var boundingRect = Bounds();
            boundingRect.Inflate(-1);
            foreach (var pos in boundingRect.PointsInRect())
            {
                var tile = this[pos];
                if (tile.IsTraversable)
                {
                    numExplorable++;
                }
                else
                {
                    // See if it's next to an traversable one.
                    foreach (var dir in Direction.All)
                    {
                        var newPosition = pos + dir;
                        if (newPosition.x < 0 || newPosition.x >= Width || newPosition.y < 0 || newPosition.y >= Height)
                        {
                            continue;
                        }

                        if (this[newPosition].IsTraversable)
                        {
                            numExplorable++;
                            break;
                        }
                    }
                }
            }
        }

        public bool IsInStage(VectorBase pos)
        {
            return Bounds().Contains(pos);
        }
    }
}