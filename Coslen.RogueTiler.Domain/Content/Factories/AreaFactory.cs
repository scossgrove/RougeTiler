using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Schema;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Quests;
using Coslen.RogueTiler.Domain.Engine.StageBuilders;
using DropFactory = Coslen.RogueTiler.Domain.Content.Factories.DropFactory;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public class AreaFactory
    {
        private static AreaFactory instance;
        public static AreaFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AreaFactory();
                }
                return instance;
            }
        }

        private AreaFactory()
        {
            initialize();
        }

        public Dictionary<string, Area> Areas { get; set; } = new Dictionary<string, Area>();

        public class AreaLevelMap
        {
            public int StageNumber { get; set; }
            public string AreaName { get; set; }
            public int LevelNumber { get; set; }

            public AreaLevelMap(string areaName, int levelNumber, int stageNumber)
            {
                AreaName = areaName;
                LevelNumber = levelNumber;
                StageNumber = stageNumber;
            }
        }

        public List<AreaLevelMap> AreaLevelMaps { get; set; } = new List<AreaLevelMap>();

        private void initialize()
        {
            var stageNumber = 0;
            var currentLevelNumber = 0;
            var currentAreaName = "Friendly Forest";
            area(currentAreaName, 80, 34, 3.0);
            level(new Forrest(), monsters: 14, items: 6, breeds: new List<string>() {
              "butterfly",
              "field mouse",
              "vole",
              "robin",
              "garter snake",
              "frog",
              "slug",
            }, drop: new List<Rarity>() {
              DropFactory.RarityDrop(1, "Rock"),
              DropFactory.RarityDrop(1, "Flower"),
              DropFactory.RarityDrop(1, "treasure", 1),
              DropFactory.RarityDrop(2, "Stick"),
              DropFactory.RarityDrop(3, "magic", 1)
            }, quest: kill("fuzzy bunny", 1));
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new Forrest(), monsters: 18, items: 8, breeds: new List<string>(){
              "white mouse",
              "bee",
              "giant earthworm",
              "garden spider",
              "tree snake",
              "wasp",
              "forest sprite"
            }, drop: new List<Rarity>() {
              DropFactory.RarityDrop(1, "Rock"),
              DropFactory.RarityDrop(2, "Flower"),
              DropFactory.RarityDrop(3, "Stick"),
              DropFactory.RarityDrop(1, "treasure", 1),
              DropFactory.RarityDrop(2, "magic", 2),
              DropFactory.RarityDrop(2, "equipment", 2)
            }, quest: kill("fox", 1));
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            // ---------------------------------------------------------------------------------------------
            currentAreaName = "Training Grounds";
            currentLevelNumber = 0;
            area(currentAreaName, 79, 33, 7.0);
            level(new TrainingGrounds(), monsters: 40, items: 6, breeds: new List<string>(){
              "mangy cur",
              "giant slug",
              "brown bat",
              "stray cat",
              "giant cockroach",
              "simpering knave",
              "decrepit mage",
              "lazy eye"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(3, "Rock"),
              DropFactory.RarityDrop(3, "magic", 2),
              DropFactory.RarityDrop(1, "treasure", 2),
              DropFactory.RarityDrop(1, "equipment", 2)
            }, quest: kill("wild dog", 3));
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new TrainingGrounds(), monsters: 46, items: 7, breeds: new List<string>(){
              "brown spider",
              "crow",
              "wild dog",
              "sewer rat",
              "drunken priest"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(3, "Rock"),
              DropFactory.RarityDrop(2, "magic", 3),
              DropFactory.RarityDrop(1, "treasure", 3),
              DropFactory.RarityDrop(1, "equipment", 3)
            }, quest: kill("giant spider"));
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new TrainingGrounds(), monsters: 52, items: 8, breeds: new List<string>(){
              "giant spider",
              "unlucky ranger",
              "raven",
              "tree snake",
              "giant earthworm"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(3, "Rock"),
              DropFactory.RarityDrop(2, "magic", 4),
              DropFactory.RarityDrop(2, "treasure", 4),
              DropFactory.RarityDrop(1, "equipment", 4)
            }, quest: kill("giant cave worm"));
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            // ---------------------------------------------------------------------------------------------
            currentAreaName = "Goblin Stronghold";
            currentLevelNumber = 0;
            area(currentAreaName, 85, 39, 12.0,
                quest: tileType("the stairs", TileTypeFactory.Instance.StairsUp));
            level(new GoblinStronghold(), monsters: 48, items: 12, breeds: new List<string>(){
              "scurrilous imp",
              "vexing imp",
              "goblin peon",
              "house sprite",
              "wild dog",
              "lizard guard",
              "blood worm",
              "giant cave worm"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(10, "Rock"),
              DropFactory.RarityDrop(1, "item", 4)
            });
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new GoblinStronghold(), monsters: 50, items: 13, breeds: new List<string>(){
              "green slime",
              "juvenile salamander",
              "imp incanter",
              "kobold",
              "goblin fighter",
              "lizard protector",
              "giant bat"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(10, "Rock"),
              DropFactory.RarityDrop(1, "item", 5)
            });
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new GoblinStronghold(), monsters: 52, items: 14,
                quest: kill("Feng"), breeds: new List<string>(){
              "kobold shaman",
              "mongrel",
              "giant centipede",
              "frosty slime",
              "kobold trickster",
              "imp warlock",
              "goblin archer",
              "armored lizard",

            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(10, "Rock"),
              DropFactory.RarityDrop(1, "item", 6)
            });
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new GoblinStronghold(), monsters: 54, items: 15, breeds: new List<string>(){
              "kobold priest",
              "goblin warrior",
              "smoking slime",
              "cave snake",
              "floating eye",
              "plague rat",
              "salamander",
              "scaled guardian"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(10, "Rock"),
              DropFactory.RarityDrop(1, "item", 7)
            });
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));

            level(new GoblinStronghold(), monsters: 56, items: 16,
                quest: kill("Erlkonig, the Goblin Prince"), breeds: new List<string>(){
              "goblin ranger",
              "goblin mage",
              "mischievous sprite",
              "cave bat",
              "fire worm",
              "sparkling slime",
              "saurian"
            }, drop: new List<Rarity>(){
              DropFactory.RarityDrop(10, "Rock"),
              DropFactory.RarityDrop(1, "item", 7)
            });
            AreaLevelMaps.Add(new AreaLevelMap(currentAreaName, currentLevelNumber++, stageNumber++));
        }

        private Area _currentArea;
        private QuestBuilder _currentQuest;

        private void area(String name, int width, int height, double abundance, QuestBuilder quest = null)
        {
            _currentQuest = quest;
            _currentArea = new Area(name, width, height, abundance, new List<Level>());
            Areas.Add(name, _currentArea);
        }

        private void level(StageBuilder builder,
                int monsters = 0, int items = 0, List<string> breeds = null, List<Rarity> drop = null, QuestBuilder quest = null
             )
        {
            if (quest == null) quest = _currentQuest;

            var breedList = new List<Breed>();
            foreach (var name in breeds)
            {
                var breed = BreedFactory.Instance.Breeds[name];
                if (breed == null) throw new ApplicationException($"Could not find breed '{name}'");
                breedList.Add(breed);
            }

            _currentArea.Levels.Add(
                new Level(
                    (stage) => builder.Generate(stage),
                    monsters,
                    items,
                    DropFactory.dropOneOf(drop),
                    breedList,
                    quest)
               );
        }

        QuestBuilder kill(String breed, int count = 1) => new MonsterQuestBuilder(BreedFactory.Instance.Breeds[breed], count);

        QuestBuilder tileType(String description, TileType type) =>
            new TileQuestBuilder(description, type);

        QuestBuilder floorItem(String type) =>
            new FloorItemQuestBuilder(ItemTypeFactory.Instance.ItemTypes[type]);

        public Stage CreateStage(Game game, int stageNumber)
        {
            // TODO: I should raise and event using event hubs or something here
            if (!AreaLevelMaps.Any(a => a.StageNumber == stageNumber))
            {
                throw new StageNotFoundExpection();
            }

            var areaLevelMap = AreaLevelMaps.Single(a => a.StageNumber == stageNumber);
            var area = Areas[areaLevelMap.AreaName];
            var createStairsDown = true;
            var createStairsUp = stageNumber != 0;
            var stage = area.BuildStage(game, areaLevelMap.LevelNumber, createStairsUp, createStairsDown);

            stage.StageNumber = stageNumber;

            // This is the amount of food the stage conatins
            stage.Abundance = area.Abundance;

            return stage;
        }


        public Breed GenerateBreedOfMonsterForStage(int stageNumber)
        {
            var areaLevelMap = AreaLevelMaps.Single(a => a.StageNumber == stageNumber);
            var area = Areas[areaLevelMap.AreaName];
            return area.PickBreed(areaLevelMap.LevelNumber);
        }
    }
}
