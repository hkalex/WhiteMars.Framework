using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WhiteMars.Framework.Configuration;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The application container for WhiteMars
    /// </summary>
    public static class WhiteMarsApplication
    {
        public static WMConfigSection ConfigSection { get; private set; }

        /// <summary>
        /// Gets the tenant info provider.
        /// </summary>
        /// <value>The tenant info provider.</value>
        public static ITenantMetaProvider TenantMetaProvider { get; private set; }

        /// <summary>
        /// Get the CacherProvider
        /// </summary>
        public static ICacherProvider CacherProvider { get; private set; }

        /// <summary>
        /// Get the ServiceProvider
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }


        static WhiteMarsApplication()
        {
            ConfigSection = System.Configuration.ConfigurationManager.GetSection("WhiteMars") as WMConfigSection;

            if (ConfigSection == null)
                throw new WhiteMarsException("WhiteMars config section is not defined.");


            ResolveStatisProperties(ConfigSection);
        }

        static void ResolveStatisProperties(WMConfigSection configSection)
        {
            if (configSection == null || configSection.ApplicationConfig == null)
                return;

            var staticProviderProperties = typeof(WhiteMarsApplication).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (var k in configSection.ApplicationConfig.Configs.Keys)
            {
                var prop = staticProviderProperties.FirstOrDefault(c => c.Name == k);
                if (prop != null)
                {
                    var typeString = configSection.ApplicationConfig.Configs[k];
                    if (string.IsNullOrWhiteSpace(typeString))
                        new WhiteMarsException(string.Format("WhiteMarsApplication.{0} is not well defined in the config file.", k));

                    var type = Type.GetType(typeString);
                    if (type == null)
                        throw new WhiteMarsException(string.Format("Type '{0}' is not found for WhiteMarsApplication.{1}.", typeString, k));

                    object instance;
                    try
                    {
                        instance = Activator.CreateInstance(type);
                    }
                    catch (Exception ex)
                    {
                        throw new WhiteMarsException(string.Format("Type '{0}' does not default constructor.", typeString), ex);
                    }

                    try
                    {
                        prop.SetValue(null, instance);
                    }
                    catch (Exception ex)
                    {
                        throw new WhiteMarsException(string.Format("Type '{0}' is not assignable to '{1}' for WhiteMarsApplication.{2}.", typeString, prop.GetType().Name, k), ex);
                    }
                }
            }

        }


    }
}
