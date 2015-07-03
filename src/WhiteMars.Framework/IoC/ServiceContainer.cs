using System;
using Microsoft.Practices.Unity.StaticFactory;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The default implementation of ServiceContainer
    /// </summary>
    internal class ServiceContainer : IServiceContainer
    {
        IUnityContainer container = new UnityContainer();

        internal ServiceContainer()
        {
            container.LoadConfiguration();
        }

        internal ServiceContainer(string tenantUrl)
            : base()
        {
            var tenantConfigLocation = WhiteMarsApplication.ConfigSection.AppSettings["TenantConfigLocation"];

            var tenantConfigFilePath = System.IO.Path.Combine(tenantConfigLocation, tenantUrl + ".config");

            if (System.IO.File.Exists(tenantConfigFilePath))
            {
                var map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = tenantConfigFilePath;
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None); 
                var section = (UnityConfigurationSection)config.GetSection("unity");

                // create child container
                var childContainer = this.container.CreateChildContainer();
                childContainer.LoadConfiguration(section);


                this.container = childContainer;
            }
        }


        #region IServiceContainer implementation

        public T Resolve<T>(params System.Collections.Generic.KeyValuePair<string, object>[] parameterOverrides)
        {
            return this.container.Resolve<T>();
        }

        public T Resolve<T>(string name, params System.Collections.Generic.KeyValuePair<string, object>[] parameterOverrides)
        {
            return this.container.Resolve<T>(name);
        }

        public void RegisterType<TInterface>(string name, Type actualType)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance<TInterface>(string name, TInterface instance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            using (this.container)
            {
                // trigger auto disposal
            }
        }

        #endregion
        
    }
}
