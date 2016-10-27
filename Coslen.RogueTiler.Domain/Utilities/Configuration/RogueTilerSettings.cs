using System;
using System.Configuration;

namespace Coslen.RogueTiler.Domain.Utilities.Configuration
{
    public class RogueTilerSettings : ConfigurationSection
    {
        private static RogueTilerSettings settings = ConfigurationManager.GetSection("RogueTilerSettings") as RogueTilerSettings;

        public static RogueTilerSettings Settings
        {
            get
            {
                return settings;
            }
        }
        

        [ConfigurationProperty("debugLevel", IsRequired = true, DefaultValue = "Info")]
        public string DebugLevel
        {
            get
            {
                return (string)this["debugLevel"];
            }
            set
            {
                this["debugLevel"] = value;
            }
        }
    }
}
