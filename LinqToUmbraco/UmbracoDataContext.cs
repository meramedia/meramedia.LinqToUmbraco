using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using meramedia.Linq.Core.Node;
using umbraco.presentation;

namespace meramedia.Linq.Core
{
    public abstract class UmbracoDataContext : IDisposable
    {
        private static readonly Lazy<UmbracoDataProvider> _dataProvider = new Lazy<UmbracoDataProvider>(() => new NodeDataProvider());
        public UmbracoDataProvider DataProvider
        {
            get { return _dataProvider.Value; }
        }


        /// <summary>
        /// Loads the tree of umbraco items.
        /// </summary>
        /// <typeparam name="TDocTypeBase">The type of the DocTypeBase.</typeparam>
        /// <returns>Collection of umbraco items</returns>
        /// <exception cref="System.ObjectDisposedException">If the DataContext has been disposed of</exception>        
        protected Tree<TDocTypeBase> LoadTree<TDocTypeBase>() where TDocTypeBase : DocTypeBase, new()
        {
            CheckDisposed();
            return DataProvider.LoadTree<TDocTypeBase>();
        }
       

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (DataProvider != null)
                {
                    DataProvider.Dispose(true);
                }

                _disposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        #endregion

        public DocTypeBase Find(int id)
        {
            return DataProvider.Find(id);
        }

        public T Find<T>(int id) where T: DocTypeBase, new()
        {
            return DataProvider.Find<T>(id);
        }
        public IEnumerable<T> FindAll<T>(int[] ids) where T: DocTypeBase, new()
        {
            return DataProvider.FindAll<T>(ids);
        }
        public IEnumerable<T> FindAll<T>(IEnumerable<int> ids) where T : DocTypeBase, new()
        {
            return DataProvider.FindAll<T>(ids.ToArray());
        }
        public IEnumerable<DocTypeBase> FindAll(int[] ids)
        {
            return DataProvider.FindAll(ids);
        }
        public IEnumerable<DocTypeBase> FindAll(IEnumerable<int> ids)
        {
            return DataProvider.FindAll(ids.ToArray());
        }

        public DocTypeBase CurrentPage
        {
            get
            {
                if (UmbracoContext.Current.PageId.HasValue)
                    return DataProvider.Find(UmbracoContext.Current.PageId.Value);
                else return null;
            }
        }

        public T GetCurrentPage<T>() where T: DocTypeBase, new()
        {
            if (UmbracoContext.Current.PageId.HasValue)
                return DataProvider.Find<T>(UmbracoContext.Current.PageId.Value);
            else return null;
        }
    
        public MediaCache Media
        {
            get
            {
                return MediaCache.Instance;
            }
        }

        public NodeCacheStatistics NodeCacheStatistics
        {
            get { return NodeCacheStatistics.Instance; }
        }

    }

    public sealed class NodeCacheStatistics
    {
        private static NodeCacheStatistics _instance;

        public static NodeCacheStatistics Instance
        {
            get { return _instance ?? (_instance = new NodeCacheStatistics()); }
        }

        public int NumItemsInMediaCache()
        {
            return MediaCache.Instance.NumItemsInCache();
        }

        public int NumTreesInNodeCache()
        {
            return NodeCache.NumTreesInCache();
        }

        public int NumNodesInNodeCache()
        {
            return NodeCache.NumNodesInCache();
        }
    }
}
