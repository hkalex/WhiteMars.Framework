using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The default Cacher
    /// </summary>
    internal class InMemoryCacher : ICacher
    {
        MemoryCache memoryCache = new MemoryCache("WhiteMars.Framework.Cache.Cacher");

        public void Set<TValue>(string key, TValue value)
        {
            this.memoryCache.Set(key, value, DateTimeOffset.MaxValue);
        }

        public void Set<TValue>(string key, TValue value, TimeSpan timeout)
        {
            this.memoryCache.Set(key, value, DateTimeOffset.Now.Add(timeout));
        }

        public void Set<TValue>(string key, TValue value, DateTimeOffset expire)
        {
            this.memoryCache.Set(key, value, expire);
        }

        public TValue Get<TValue>(string key)
        {
            if (this.memoryCache.Contains(key))
            {
                return (TValue)this.memoryCache.Get(key);
            }
            else
            {
                return default(TValue);
            }
        }
    }
}
