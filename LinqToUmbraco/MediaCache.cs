using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.media;

namespace meramedia.Linq.Core
{
    //TODO: needs some kind of way of flushing old items out, otherwise memory consumption will grow forever
    public class MediaCache
    {
        private static Dictionary<int, Media> _cache;

        private const int MAX_NUM_ITEMS = 400;

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
                _cache.Add(cached.Id, cached);

                if (_cache.Count > MAX_NUM_ITEMS)
                    _cache.Remove(_cache.First().Key);
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
                    _cache.Add(m.Id, m);
                    cached.Add(m);

                    if (_cache.Count > MAX_NUM_ITEMS)
                        _cache.Remove(_cache.First().Key);
                }                    
            }
            return cached;
        }

        internal void Flush()
        {
            _cache.Clear();
        }

        internal int NumItemsInCache()
        {
            return _cache.Count;
        }
    }
}
