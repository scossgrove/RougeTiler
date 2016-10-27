using System.Collections.Generic;
using System.IO;
using System.Linq;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;
using Newtonsoft.Json;

namespace Coslen.RogueTiler.Domain.Utilities
{
    /// The entry point for all persisted save data.
    public class Storage
    {
        public Storage(GameContent content)
        {
            Content = content;
            //Load();
        }

        private GameContent Content { get; set; }
        public List<HeroSave> Heroes { get; set; } = new List<HeroSave>();

        public void Load()
        {
            var filename = GetStorageFilename();
            if (!File.Exists(filename))
            {
                return;
            }

            var json = File.ReadAllText(filename);

            var heroDataJson = JsonConvert.DeserializeObject<List<HeroSaveJson>>(json);

            foreach (var hero in heroDataJson)
            {
                var heroSave = HeroSaveFactory.FromJson(hero);

                Heroes.Add(heroSave);
            }
        }

        public void Save()
        {
            var heroData = new List<HeroSaveJson>();
            foreach (var hero in Heroes)
            {
                var heroSaveJson = HeroSaveFactory.ToJson(hero);

                heroData.Add(heroSaveJson);
            }

            var heroDataJson = JsonConvert.SerializeObject(heroData, Formatting.Indented);

            var filename = GetStorageFilename();
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            File.WriteAllText(filename, heroDataJson);
        }

        private static string GetStorageFilename()
        {
            var projectRoot = GamePathUtilities.GetProjectRoot();
            var savedGameLocation = projectRoot + @"\App_Data";
            var filename = savedGameLocation + @"\heroes.json";
            return filename;
        }

        public HeroSave GetHero(string heroName)
        {
            return Heroes.Single(h => h.Name == heroName);
        }

        public void Update(GameState gameState)
        {
            // Construct the Hero State
            gameState.UpdateHeroSave();
        }
    }
}