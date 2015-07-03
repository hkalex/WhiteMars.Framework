using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    internal class InMemoryCacherProvider : ICacherProvider
    {
        ICacher sharedCacher;
        ConcurrentDictionary<string, ICacher> cachers = new ConcurrentDictionary<string, ICacher>(StringComparer.OrdinalIgnoreCase);

        public InMemoryCacherProvider()
        {
            this.sharedCacher = new InMemoryCacher();
        }

        public ICacher GetCacher()
        {
            return this.sharedCacher;
        }

        public ICacher GetCacher(string tenantUrl)
        {
            if (cachers.ContainsKey(tenantUrl))
            {
                return cachers[tenantUrl];
            }
            else
            {
                var result = new InMemoryCacher();
                cachers[tenantUrl] = result;
                return result;
            }
        }
    }
}
