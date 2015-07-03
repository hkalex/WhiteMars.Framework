using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The common interface for IoC service provider
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// Get the shared container
        /// </summary>
        /// <returns>The container.</returns>
        IServiceContainer GetContainer();

        /// <summary>
        /// Get default container for a tenant.
        /// </summary>
        /// <param name="tenantUrl"></param>
        /// <returns></returns>
        IServiceContainer GetContainer(string tenantUrl);
    }
}
