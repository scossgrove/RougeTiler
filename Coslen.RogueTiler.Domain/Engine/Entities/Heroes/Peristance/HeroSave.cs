using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Items;
using Newtonsoft.Json.Linq;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance
{
    /// When the player is playing the game inside a dungeon, he is using a [Hero].
    /// When outside of the dungeon on the menu screens, though, only a subset of
    /// the hero's data persists (for example, there is no position when not in a
    /// dungeon). This class stores that state.
    public class HeroSave
    {
        public HeroSave(string name, HeroClass heroClass)
        {
            Name = name;
            HeroClass = heroClass;
        }

        public HeroSave(
                string name, 
                HeroClass heroClass, 
                Inventory inventory, 
                Equipment equipment,
                Inventory backPack,
                Inventory crucible,
                List<Stage> stages, 
                Dictionary<string, object> properties) 
            : this(name, heroClass)
        {
            Inventory = inventory;
            Equipment = equipment;
            BackPack = backPack;
            Crucible = crucible;
            Stages = stages;
            Properties = properties;
            IsDirty = false;
        }

        // This marks the hero game as having been played.
        public bool IsDirty { get; set; }

        public Equipment Equipment = new Equipment();

        public Inventory Inventory = new Inventory(Option.InventoryCapacity);
        
        public Inventory BackPack { get; set; } = new Inventory(Option.HomeCapacity);
        public Inventory Crucible { get; set; } = new Inventory(Option.CrucibleCapacity);

        // These are the stage that have been visited by the hero
        public List<Stage> Stages { get; set; } = new List<Stage>();

        public HeroClass HeroClass { get; set; }

        public int Level
        {
            get { return LevelUtilties.CalculateLevel(ExperienceCents); }
        }

        private int ConvertToInt(object source)
        {
            return int.Parse(source.ToString());
        }

        private double ConvertToDouble(object source)
        {
            return double.Parse(source.ToString());
        }

        private VectorBase ConvertToVectorBase(JObject source)
        {
            return new VectorBase((int) source["x"], (int) source["y"]);
        }

        private Stat ConvertToStat(JObject source)
        {
            return new Stat((double)source["Current"], (double)source["Max"]);
        }

        #region "Persisted Properties"

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public string Name
        {
            get
            {
                if (!Properties.ContainsKey("Name"))
                {
                    Name = string.Empty;
                }
                return Properties["Name"].ToString();
            }
            set { Properties["Name"] = value; }
        }

        public int Energy
        {
            get
            {
                if (!Properties.ContainsKey("Energy"))
                {
                    Energy = 0;
                }
                return (int) Properties["Energy"];
            }
            set { Properties["Energy"] = value; }
        }

        public int ExperienceCents
        {
            get
            {
                if (!Properties.ContainsKey("ExperienceCents"))
                {
                    ExperienceCents = 0;
                }
                return ConvertToInt(Properties["ExperienceCents"]);
            }
            set { Properties["ExperienceCents"] = value; }
        }

        public int Gold
        {
            get
            {
                if (!Properties.ContainsKey("Gold"))
                {
                    Gold = Option.HeroGoldStart;
                }
                return ConvertToInt(Properties["Gold"]);
            }
            set { Properties["Gold"] = value; }
        }

        public int LastNoise
        {
            get
            {
                if (!Properties.ContainsKey("LastNoise"))
                {
                    LastNoise = 0;
                }
                return ConvertToInt(Properties["LastNoise"]);
            }
            set { Properties["LastNoise"] = value; }
        }

        public double Food
        {
            get
            {
                if (!Properties.ContainsKey("Food"))
                {
                    Food = 0;
                }
                return ConvertToDouble(Properties["Food"]);
            }
            set { Properties["Food"] = value; }
        }

        public Stat Charge
        {
            get
            {
                if (!Properties.ContainsKey("Charge"))
                {
                    Charge = new Stat(0);
                }
                if (Properties["Charge"] is Stat)
                {
                    return (Stat)Properties["Charge"];
                }
                return ConvertToStat((JObject)Properties["Charge"]);
            }
            set { Properties["Charge"] = value; }
        }

        public Stat Health
        {
            get
            {
                if (!Properties.ContainsKey("Health"))
                {
                    Health = new Stat(0);
                }

                if (Properties["Health"] is Stat)
                {
                    return (Stat)Properties["Health"];
                }
                return ConvertToStat((JObject)Properties["Health"]);
            }
            set { Properties["Health"] = value; }
        }

        public VectorBase Position
        {
            get
            {
                if (!Properties.ContainsKey("Position"))
                {
                    Position = new VectorBase(0, 0);
                }
                if (Properties["Position"] is VectorBase)
                {
                    return (VectorBase) Properties["Position"];
                }
                return ConvertToVectorBase((JObject) Properties["Position"]);
            }
            set { Properties["Position"] = value; }
        }

        public int CurrentStage
        {
            get
            {
                if (!Properties.ContainsKey("CurrentStage"))
                {
                    CurrentStage = 0;
                }
                return ConvertToInt(Properties["CurrentStage"]);
            }
            set
            {
                Properties["CurrentStage"] = value;
            }
        }

        #endregion
    }
}