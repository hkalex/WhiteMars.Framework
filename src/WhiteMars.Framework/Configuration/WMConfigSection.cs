using System;
using System.Configuration;

namespace WhiteMars.Framework.Configuration
{
    /// <summary>
    /// WhiteMars ConfigSection
    /// </summary>
    public class WMConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("ApplicationConfig")]
        public WMApplicationDefault ApplicationConfig
        { 
            get { return (WMApplicationDefault)this["ApplicationConfig"]; }
            set { this["ApplicationConfig"] = value; }
        }

        [ConfigurationProperty("AppSettings")]
        public WhiteMars.Framework.Configuration.WMAppSettings AppSettings
        {
            get { return (WhiteMars.Framework.Configuration.WMAppSettings)this["AppSettings"]; }
            set { this["AppSettings"] = value; }
        }
    }
}

