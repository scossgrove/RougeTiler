using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Classes;
using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;
using Newtonsoft.Json.Linq;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance
{
    public static class HeroSaveFactory
    {
        public static HeroSaveJson ToJson(HeroSave source)
        {
            // Has this hero been altered or played?
            if (!source.IsDirty)
            {
                // This path exists as there are no stages yet and therefore there is not 
                // Current Hero
                var heroSaveJson = new HeroSaveJson {
                    Name = source.Name,
                    // Lists that need to be preserved
                    Equipment = ConvertToJsonList(source.Equipment.Items),
                    Inventory = ConvertToJsonList(source.Inventory),
                    BackPack = ConvertToJsonList(source.BackPack),
                    Crucible = ConvertToJsonList(source.Crucible),
                    Stages = ConvertToJsonList(source.Stages),
                    // Actor Properties
                    Properties = source.Properties};

                heroSaveJson.Properties["Health"] = source.Health;
                heroSaveJson.Properties["Charge"] = source.Charge;
                heroSaveJson.Properties["Position"] = source.Position;
                heroSaveJson.Properties["ExperienceCents"] = source.ExperienceCents;
                heroSaveJson.Properties["Gold"] = source.Gold;
                heroSaveJson.Properties["Food"] = source.Food;

                var warrior = source.HeroClass as Warrior;
                if (warrior != null)
                {
                    SaveWarrior(warrior, heroSaveJson);
                }

                return heroSaveJson;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static HeroSave FromJson(HeroSaveJson source)
        {
            HeroClass heroClass = null;
            switch (source.HeroClass.Name)
            {
                case "Warrior":
                {
                    heroClass = LoadWarrior(source.HeroClass);
                    break;
                }
                default:
                {
                    throw new ApplicationException($"Unknown hero class '{source.HeroClass.Name}'.");
                }
            }

            var equipmentItemList = ConvertFromJsonList(source.Equipment);
            var equipment = new Equipment();
            for (var i = 0; i < equipmentItemList.Count; i++)
            {
                equipment[i] = equipmentItemList[i];
            }

            var inventoryItemList = ConvertFromJsonList(source.Inventory);
            var inventory = new Inventory(Option.InventoryCapacity);
            inventory.AddRange(inventoryItemList);

            var backPackItemList = ConvertFromJsonList(source.BackPack);
            var backPack = new Inventory(Option.HomeCapacity);
            backPack.AddRange(backPackItemList);

            var crucibleItemList = ConvertFromJsonList(source.Crucible);
            var crucible = new Inventory(Option.CrucibleCapacity);
            crucible.AddRange(crucibleItemList);

            var stages = ConvertFromJsonList(source.Stages);

            var heroSave = new HeroSave(
                source.Name, 
                heroClass, 
                inventory, 
                equipment,
                backPack,
                crucible,
                stages,
                source.Properties);

            return heroSave;
        }

        private static Warrior LoadWarrior(HeroClassJson source)
        {
            var fighting = source.Properties.Single(p => p.Name == "Fighting").Count;
            var combat = source.Properties.Single(p => p.Name == "Combat").Count;
            var toughness = source.Properties.Single(p => p.Name == "Toughness").Count;
            var masteries = new Dictionary<string, int>();
            foreach (var stat in source.Masteries)
            {
                masteries.Add(stat.Name, stat.Count);
            }

            return Warrior.Load(fighting, combat, toughness, masteries);
        }

        private static void SaveWarrior(Warrior warrior, HeroSaveJson data)
        {
            if (data.HeroClass == null)
            {
                data.HeroClass = new HeroClassJson();
                data.HeroClass.Properties = new List<TrainedStatJson>();
                data.HeroClass.Masteries = new List<TrainedStatJson>();
            }
            data.HeroClass.Name = warrior.Name;
            data.HeroClass.Properties.Add(ConvertToJson("Fighting", warrior.Fighting));
            data.HeroClass.Properties.Add(ConvertToJson("Combat", warrior.Combat));
            data.HeroClass.Properties.Add(ConvertToJson("Toughness", warrior.Toughness));

            foreach (var kvp in warrior.Masteries)
            {
                data.HeroClass.Masteries.Add(ConvertToJson(kvp.Key, kvp.Value));
            }
        }

        private static TrainedStatJson ConvertToJson(string name, TrainedStat source)
        {
            var result = new TrainedStatJson {Name = name, Count = source.Count};

            return result;
        }

        private static List<Item> ConvertFromJsonList(List<ItemDataJson> source)
        {
            var result = new List<Item>();
            if (source == null)
            {
                return result;
            }

            foreach (var item in source)
            {
                result.Add(ConvertFromItemDataJson(item));
            }

            return result;
        }

        private static List<ItemDataJson> ConvertToJsonList(List<Item> source)
        {
            var result = new List<ItemDataJson>();

            foreach (var item in source)
            {
                result.Add(ConvertToItemDataJson(item));
            }

            return result;
        }

        private static Item ConvertFromItemDataJson(ItemDataJson source)
        {
            if (source == null || source.ItemType == null)
            {
                return null;
            }

            var type = GameContent.Instance.items[source.ItemType];
            var prefix = ConvertFomItemAffixJson(source.Prefix);
            var suffix = ConvertFomItemAffixJson(source.Suffix);
            var position = source.Position;

            var result = new Item(type, prefix, suffix);
            result.Position = position;

            return result;
        }

        private static ItemDataJson ConvertToItemDataJson(Item source)
        {
            if (source == null)
            {
                return new ItemDataJson();
            }

            var result = new ItemDataJson {ItemType = source.type.ToString(), Prefix = ConvertToItemAffixJson(source.prefix), Suffix = ConvertToItemAffixJson(source.suffix), Position = source.Position};

            return result;
        }

        private static Affix ConvertFomItemAffixJson(ItemAffixJson source)
        {
            if (source == null || source.Name == null)
            {
                return null;
            }

            var name = source.Name;
            var attack = new Attack("", 0);

            if (source.AttackData.ContainsKey("element"))
            {
                attack = attack.Brand((Element) Enum.Parse(typeof (Element), source.AttackData["element"].ToString()));
            }

            if (source.AttackData.ContainsKey("damageBonus"))
            {
                attack = attack.AddDamage((double) source.AttackData["damageBonus"]);
            }

            if (source.AttackData.ContainsKey("strikeBonus"))
            {
                attack = attack.AddStrike((double) source.AttackData["strikeBonus"]);
            }

            if (source.AttackData.ContainsKey("damageScale"))
            {
                attack = attack.MultiplyDamage((double) source.AttackData["damageScale"]);
            }


            var affix = new Affix(name, attack);

            return affix;
        }

        private static ItemAffixJson ConvertToItemAffixJson(Affix source)
        {
            if (source == null)
            {
                return new ItemAffixJson();
            }

            var result = new ItemAffixJson {Name = source.name, AttackData = new Dictionary<string, object>()};

            if (source.attack.Element != ElementFactory.Instance.None)
            {
                result.AttackData.Add("element", source.attack.Element.ToString());
            }

            if (source.attack.DamageBonus != 0)
            {
                result.AttackData.Add("damageBonus", source.attack.DamageBonus);
            }

            if (source.attack.StrikeBonus != 0)
            {
                result.AttackData.Add("strikeBonus", source.attack.StrikeBonus);
            }

            if (source.attack.DamageScale != 1.0)
            {
                result.AttackData.Add("damageScale", source.attack.DamageScale);
            }

            return result;
        }


        private static List<Stage> ConvertFromJsonList(List<StageJson> source)
        {
            var result = new List<Stage>();

            foreach (var item in source)
            {
                result.Add(ConvertFromStageJson(item));
            }

            return result;
        }

        private static List<StageJson> ConvertToJsonList(List<Stage> source)
        {
            var result = new List<StageJson>();

            foreach (var item in source)
            {
                result.Add(ConvertToStageJson(item));
            }

            return result;
        }

        private static Stage ConvertFromStageJson(StageJson source)
        {
            var items = ConvertFromJsonList(source.Items);
            var explored = source.Explored;
            var shadows = ConvertFromJsonMatrix(source.Shadows);
            var tiles = ConvertFomJsonMatrix(source.Tiles);
            var actors = ConvertFromJsonList(source.Actors);
            var lastHeroPosition = source.LastHeroPosition;
            var stairDownPosition = source.StairDownPosition;
            var stairUpPosition = source.StairUpPosition;
            var abundance = source.Abundance;

            var result = new Stage(tiles, shadows, explored, items, actors, lastHeroPosition, abundance);
            result.StairDownPosition = stairDownPosition;
            result.StairUpPosition = stairUpPosition;
            return result;
        }

        private static StageJson ConvertToStageJson(Stage source)
        {
            var result = new StageJson();
            result.Items = ConvertToJsonList(source.Items);
            result.Explored = source.Explored;
            result.Shadows = ConvertToJsonMatrix(source.Shadows);
            result.Tiles = ConvertToJsonMatrix(source.Tiles);
            result.Actors = ConvertToJsonList(source.Actors);
            result.LastHeroPosition = source.LastHeroPosition;
            result.StairDownPosition = source.StairDownPosition;
            result.StairUpPosition = source.StairUpPosition;
            result.Abundance = source.Abundance;

            return result;
        }

        private static List<ActorJson> ConvertToJsonList(List<Actor> source)
        {
            var result = new List<ActorJson>();

            foreach (var item in source)
            {
                if (item is Hero)
                {
                    continue;
                }
                result.Add(ConvertToActorJson(item));
            }

            return result;
        }

        private static List<Actor> ConvertFromJsonList(List<ActorJson> source)
        {
            var result = new List<Actor>();

            foreach (var item in source)
            {
                if (item.ActorType == "Hero")
                {
                    continue;
                }
                result.Add(ConvertFromActorJson(item));
            }

            return result;
        }

        private static Actor ConvertFromActorJson(ActorJson source)
        {
            Actor result = null;

            if (source.ActorType == "Monster")
            {
                var breed = BreedFactory.Instance.Breeds[source.Properties["BreedName"].ToString()];
                var positionJObject = (JObject) source.Properties["Position"];
                var position = new VectorBase((int) positionJObject["x"], (int) positionJObject["y"]);

                var healthJObject = (JObject) source.Properties["Health"];
                var health = new Stat((int) healthJObject["Current"], (int) healthJObject["Max"]);

                var generation = int.Parse(source.Properties["Generation"].ToString());

                var monster = new Monster(null, breed, position.x, position.y, (int)health.Max, generation);

                monster.Health = health;

                result = monster;
            }
            else
            {
                throw new ApplicationException("Have not implemented peristance for actor type: " + source.ActorType);
            }

            return result;
        }

        private static ActorJson ConvertToActorJson(Actor source)
        {
            var result = new ActorJson();
            result.Properties = new Dictionary<string, object>();

            if (source is Monster)
            {
                var monster = source as Monster;

                result.ActorType = "Monster";
                result.Properties.Add("BreedName", monster.Breed.Name);
                result.Properties.Add("Position", source.Position);
                result.Properties.Add("Health", monster.Health);
                result.Properties.Add("Generation", monster.Generation);
                result.Properties.Add("Fear", monster.Fear);
                result.Properties.Add("MonsterState", monster.State.GetType().Name);
            }
            else if (source is Hero)
            {
                var hero = source as Hero;
                result.ActorType = "Hero";

                var ignoreProperties = new List<string> {"Equipment", "Inventory", "HeroClass", "Behavior", "Game", "Appearance"};

                foreach (var propInfo in hero.GetType().GetProperties())
                {
                    if (ignoreProperties.Contains(propInfo.Name) == false)
                    {
                        if (propInfo.CanWrite && propInfo.GetSetMethod( /*nonPublic*/ true).IsPublic)
                        {
                            // The setter exists and is public.
                            result.Properties.Add(propInfo.Name, propInfo.GetValue(hero, null));
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("Have not implemented peristance for actor type: " + source.GetType());
            }

            return result;
        }

        private static TileJson[,] ConvertToJsonMatrix(Tile[,] source)
        {
            var matrixWidth = source.GetUpperBound(0) + 1;
            var matrixHeight = source.GetUpperBound(1) + 1;

            var result = new TileJson[matrixWidth, matrixHeight];
            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    result[columnIndex, rowIndex] = ConvertToTileJson(source[columnIndex, rowIndex], new VectorBase(columnIndex, rowIndex));
                }
            }

            return result;
        }

        private static Tile[,] ConvertFomJsonMatrix(TileJson[,] source)
        {
            var matrixWidth = source.GetUpperBound(0) + 1;
            var matrixHeight = source.GetUpperBound(1) + 1;

            var result = new Tile[matrixWidth, matrixHeight];
            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    result[columnIndex, rowIndex] = ConvertFromTileJson(source[columnIndex, rowIndex], new VectorBase(columnIndex, rowIndex));
                }
            }

            return result;
        }

        private static Tile ConvertFromTileJson(TileJson source, VectorBase position)
        {
            var result = new Tile();

            result.Visible = source.Visible;
            result.Type = TileTypeFactory.Instance.GetById(source.TileType);
            result.IsExplored = source.IsExplored;
            result.Position = source.Position ?? position;
            return result;
        }

        private static TileJson ConvertToTileJson(Tile source, VectorBase position)
        {
            var result = new TileJson();

            result.Visible = source.Visible;
            result.TileType = source.Type.Name;
            result.IsExplored = source.IsExplored;
            result.Position = source.Position ?? position;

            return result;
        }

        private static ShadowJson[,] ConvertToJsonMatrix(Shadow[,] source)
        {
            var matrixWidth = source.GetUpperBound(0) + 1;
            var matrixHeight = source.GetUpperBound(1) + 1;

            var result = new ShadowJson[matrixWidth, matrixHeight];
            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    result[columnIndex, rowIndex] = ConvertToShadowJson(source[columnIndex, rowIndex]);
                }
            }

            return result;
        }

        private static Shadow[,] ConvertFromJsonMatrix(ShadowJson[,] source)
        {
            var matrixWidth = source.GetUpperBound(0) + 1;
            var matrixHeight = source.GetUpperBound(1) + 1;

            var result = new Shadow[matrixWidth, matrixHeight];
            for (var rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    result[columnIndex, rowIndex] = ConvertFromShadowJson(source[columnIndex, rowIndex]);
                }
            }

            return result;
        }

        private static Shadow ConvertFromShadowJson(ShadowJson source)
        {
            var result = new Shadow();

            result.SetActive(source.InShadow);
            result.SetActive(source.IsActive);

            return result;
        }

        private static ShadowJson ConvertToShadowJson(Shadow source)
        {
            var result = new ShadowJson();

            result.InShadow = source.IsInShadow;
            result.IsActive = source.IsActive;

            return result;
        }
    }
}