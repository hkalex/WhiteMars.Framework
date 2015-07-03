using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The common interface for IoC service container
    /// </summary>
    public interface IServiceContainer : IDisposable
    {
        /// <summary>
        /// Resolve the unnamed instance
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Resolve<T>(params KeyValuePair<string, object>[] parameterOverrides);

        /// <summary>
        /// Resolve the instance with specific name
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="parameterOverrides">Parameter overrides.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Resolve<T>(string name, params KeyValuePair<string, object>[] parameterOverrides);

        /// <summary>
        /// Register a type to an interface with specific name
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="actualType"></param>
        /// <param name="name"></param>
        void RegisterType<TInterface>(string name, Type actualType);

        /// <summary>
        /// Register an instance to an interface with specific name
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        void RegisterInstance<TInterface>(string name, TInterface instance);
    }
}
