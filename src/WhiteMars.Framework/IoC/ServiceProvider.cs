using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    internal class ServiceProvider : IServiceProvider
    {

        ServiceContainer defaultContainer = new ServiceContainer();
        ConcurrentDictionary<string, IServiceContainer> tenantContainers = new ConcurrentDictionary<string, IServiceContainer>(StringComparer.InvariantCultureIgnoreCase);


        public IServiceContainer GetContainer(string tenantUrl)
        {
            if (string.IsNullOrWhiteSpace(tenantUrl)) throw new ArgumentNullException("tenantUrl");

            if (tenantContainers.ContainsKey(tenantUrl))
            {
                return tenantContainers[tenantUrl];
            }
            else
            {
                var result = new ServiceContainer(tenantUrl);
                tenantContainers[tenantUrl] = result;
                return result;
            }
        }

        public IServiceContainer GetContainer()
        {
            return this.defaultContainer;
        }
    }
}
