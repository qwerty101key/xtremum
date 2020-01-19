using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace ContentManagement
{
    public class DefaultCacher : ICacher
    {
        public List<string> Files { get; private set; }
        public TimeSpan Duration { get; private set; }

        public DefaultCacher(TimeSpan timeSpan, List<string> files)
        {
            Files = files?.ToList() ?? new List<string>();
            Duration = timeSpan;
        }

        public object GetValue(string key)
        {
            return MemoryCache.Default.GetCacheItem(key)?.Value;            
        }

        public void SetValue(string key, object value)
        {
            CacheItemPolicy policy = new CacheItemPolicy()
            {
                /* Запись удаляется из кеша, если ей не пользовались некоторое время (3 минуты) */
                SlidingExpiration = Duration
            };

            /* Запись удаляется из кеша, если исходный файл на диске был изменён */

            policy.ChangeMonitors.Add(new HostFileChangeMonitor(Files));

            MemoryCache.Default.Set(
                new CacheItem(key, value),
                policy);
        }
    }
}