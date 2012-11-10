using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.media;

namespace meramedia.Linq.Core
{
    public class MediaCache
    {
        private static Dictionary<int, Media> _cache;

        private const int MaxNumItems = 400;

        private static readonly object CacheLock = new object();

        private static MediaCache _instance;
        public static MediaCache Instance
        {
            get { return _instance ?? (_instance = new MediaCache()); }
        }

        private MediaCache()
        {
            _cache = new Dictionary<int, Media>();
        }

        public Media Find(int id)
        {
            var cached = _cache.SingleOrDefault(x => x.Key == id).Value;
            if (cached == null)
            {
                cached = new Media(id);
                lock (CacheLock)
                {
                    _cache.Add(cached.Id, cached);

                    if (_cache.Count > MaxNumItems)
                        _cache.Remove(_cache.First().Key);
                }
            }
            return cached;

        }

        public IEnumerable<Media> FindAll(int[] ids)
        {
            List<Media> cached = _cache.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList();

            if (cached.Count() != ids.Length) // didn't get all from cache
            {
                foreach (var m in ids.Where(x => !cached.Select(m => m.Id).Contains(x)).Select(id => new Media(id)))
                {
                    lock (CacheLock)
                    {
                        _cache.Add(m.Id, m);                        

                        if (_cache.Count > MaxNumItems)
                            _cache.Remove(_cache.First().Key);
                    }
                    cached.Add(m);

                }                    
            }
            return cached;
        }

        internal void Flush()
        {
            lock (CacheLock)
            {
                _cache.Clear();    
            }
            
            Debug.WriteLine("All mediaitems flushed!");
        }

        internal void Flush(int id)
        {
            if (_cache.ContainsKey(id))
            {
                lock (CacheLock)
                {
                    _cache.Remove(id);    
                }                
                Debug.WriteLine("Media id: " + id + "flushed!");
            }
        }

        internal int NumItemsInCache()
        {
            return _cache.Count;
        }
    }
}
