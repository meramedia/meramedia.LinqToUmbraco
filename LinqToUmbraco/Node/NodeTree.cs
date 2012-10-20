using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using meramedia.Linq.Core;

namespace meramedia.Linq.Core.Node
{
    /// <summary>
    /// Represents a collection of TDocTypeBase retrieved from the umbraco XML cache
    /// </summary>
    /// <typeparam name="TDocTypeBase">The type of the doc type base.</typeparam>
    public sealed class NodeTree<TDocTypeBase> : Tree<TDocTypeBase> where TDocTypeBase : DocTypeBase, new()
    {
        private readonly object _lockObject = new object();

        private NodeDataProvider _provider;

        private List<TDocTypeBase> _nodes;

        internal NodeTree(NodeDataProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public override UmbracoDataProvider Provider
        {
            get
            {
                return _provider;
            }
            protected set
            {
                var nodeProvider = value as NodeDataProvider;
                if (nodeProvider == null)
                {
                    throw new ArgumentException("Value must be of type NodeDataProvider");
                }
                _provider = nodeProvider;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<TDocTypeBase> GetEnumerator()
        {
            if (_nodes == null)
            {
                _nodes = new List<TDocTypeBase>();
                var rawNodes = _provider.Xml.Descendants().Where(x => ReflectionAssistance.CompareByAlias(typeof(TDocTypeBase), x));

                lock (_lockObject)
                {
                    foreach (XElement n in rawNodes)
                    {
                        var dt = new TDocTypeBase();
                        _provider.LoadFromXml(n, dt);

                        dt.IsDirty = false;
                        dt.Provider = _provider;

                        _nodes.Add(dt);
                    }
                }
            }
            return _nodes.GetEnumerator();
        }

        /// <summary>
        /// Reloads the cache for the particular NodeTree
        /// </summary>
        public override void ReloadCache()
        {            
            _provider.CheckDisposed();

            var attr = ReflectionAssistance.GetUmbracoInfoAttribute(typeof(TDocTypeBase));
            NodeCache.ClearTree(attr);
            _provider.SetupNodeTree<TDocTypeBase>(attr);
        }
    }
}
