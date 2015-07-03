using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The common interface for ICacher provider
    /// </summary>
    public interface ICacherProvider
    {
        /// <summary>
        /// Get the non-tenant specific ICacher
        /// </summary>
        /// <returns></returns>
        ICacher GetCacher();

        /// <summary>
        /// Get the tenant specific ICacher
        /// </summary>
        /// <param name="tenantUrl"></param>
        /// <returns></returns>
        ICacher GetCacher(string tenantUrl);
    }
}
