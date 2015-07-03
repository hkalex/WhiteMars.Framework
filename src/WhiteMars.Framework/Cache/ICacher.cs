using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The public interface for ICacher
    /// </summary>
    public interface ICacher
    {
        /// <summary>
        /// Set a value to cache without expire
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void Set<TValue>(string key, TValue value);

        /// <summary>
        /// Set a value to cache with timeout
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        void Set<TValue>(string key, TValue value, TimeSpan timeout);

        /// <summary>
        /// Set a value of cache with explicit timeout datetime
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        void Set<TValue>(string key, TValue value, DateTimeOffset expire);

        /// <summary>
        /// Get the value of cache. If the cache does not exist or expired, default(Tvalue) will be returned.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        TValue Get<TValue>(string key);
    }
}
