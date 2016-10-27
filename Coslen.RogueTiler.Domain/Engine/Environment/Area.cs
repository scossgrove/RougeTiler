using System;
using System.Collections.Generic;
using System.Diagnostics;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Classes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine.Environment
{
    public class Area
    {
        public Area(string name, int width, int height, double abundance, List<Level> levels)
        {
            Name = name;
            Width = width;
            Height = height;
            Abundance = abundance;
            Levels = levels;
        }

        public string Name { get; set; }

        /// Width of the stage for this area.
        public int Width { get; set; }

        /// Height of the stage for this area.
        public int Height { get; set; }

        /// The amount of food the level contains.
        /// 
        /// A higher number here increases the rate that the [Hero] finds food as they
        /// explore the level.
        public double Abundance { get; set; }

        public List<Level> Levels { get; set; }

        /// <summary>
        /// This will build a stage.
        /// 
        /// This involves:
        /// 1. Generating TileTypeFactory, Shadows & Explored matrixes.
        /// 2. Generating Items/Loot
        /// 3. Generating and placing Breeds.
        /// 
        /// This DOES NOT invole:
        /// 1. placing of the hero.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="hasExitUp"></param>
        /// <param name="hasExitDown"></param>
        /// <returns></returns>
        public Stage BuildStage(Game game, int depth, bool hasExitUp, bool hasExitDown)
        {
            var level = Levels[depth];

            var stage = new Stage(Width, Height);
            stage.HasExitUp = hasExitUp;
            stage.HasExitDown = hasExitDown;
            level.BuildStage(stage);


            // Allocate a Hero Position in order to do calculations below
            var heroPosition = stage.StairUpPosition;
            if (heroPosition == null)
            {
                heroPosition = stage.FindOpenTile();
            }
            var tmpHeroSave = new HeroSave("temp", new Warrior());
            var tempHero = new Hero(game, heroPosition, tmpHeroSave, null, tmpHeroSave.Name);
            stage.CurrentHero = tempHero;
            stage.AddActor(tempHero);
            stage.LastHeroPosition = heroPosition;


            // Place the items.
            var numItems = Rng.Instance.taper(level.NumItems.Value, 3);
            for (var i = 0; i < numItems; i++)
            {
                var itemDepth = PickDepth(depth);
                var drop = Levels[itemDepth].FloorDrop;

                Action<Item> myAddItem = delegate(Item item)
                {
                    item.Position = stage.FindOpenTile();
                    stage.Items.Add(item);
                };

                drop.SpawnDrop(new AddItem(myAddItem));
            }

            // Place the monsters.
            var numMonsters = Rng.Instance.taper(level.NumMonsters.Value, 3);
            var placesOfMonsters = new List<VectorBase>();
            for (var i = 0; i < numMonsters; i++)
            {
                var monsterDepth = PickDepth(depth);

                // Place strong monsters farther from the hero.
                var tries = 1;
                if (monsterDepth > depth)
                {
                    tries = 1 + (monsterDepth - depth)*2;
                }

                VectorBase pos = null;
                for (var attempts = 0; attempts < 5; attempts++)
                {
                    pos = stage.FindDistantOpenTile(tries);
                    if (placesOfMonsters.Contains(pos))
                    {
                        continue;
                    }

                    if (pos != null)
                    {
                        break;
                    }
                }

                if (pos == null)
                {
                    throw new ApplicationException("Invalid position found....");
                }

                var breed = Rng.Instance.Item(Levels[monsterDepth].Breeds);
                stage.SpawnMonster(game, breed, pos);
                placesOfMonsters.Add(pos);
            }

            // TODO: no quest system....
            //game.quest = level.quest.generate(stage);
            //game.quest.announce(game.log);

            // TODO: Temp. Wizard light it.
            /*
            for (var pos in stage.bounds) {
              for (var dir in Direction.all) {
                if (stage.bounds.contains(pos + dir) &&
                    stage[pos + dir].isTransparent) {
                  stage[pos].visible = true;
                  break;
                }
              }
            }
            */

            return stage;
        }

        public int PickDepth(int depth)
        {
            while (Rng.Instance.OneIn(4) && depth > 0)
            {
                depth--;
            }
            while (Rng.Instance.OneIn(6) && depth < Levels.Count - 1)
            {
                depth++;
            }

            return depth;
        }

        /// Selects a random [Breed] for the appropriate depth in this Area. Will
        /// occasionally select out-of-level breeds.
        public Breed PickBreed(int level)
        {
            if (Rng.Instance.OneIn(2))
            {
                while (Rng.Instance.OneIn(2) && level > 0)
                {
                    level--;
                }
            }
            else
            {
                while (Rng.Instance.OneIn(4) && level < Levels.Count - 1)
                {
                    level++;
                }
            }

            return Rng.Instance.Item(Levels[level].Breeds);
        }
    }
}