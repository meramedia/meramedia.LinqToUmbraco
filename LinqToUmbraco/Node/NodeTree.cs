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
            if (Nodes == null)
            {
                Nodes = new List<TDocTypeBase>();

                var xmlNodes = _provider.Xml.Where(x => ReflectionAssistance.CompareByAlias(typeof(TDocTypeBase), x));

                lock (_lockObject)
                {
                    foreach (XElement node in xmlNodes)
                    {
                        TDocTypeBase dt = new TDocTypeBase();
                        _provider.LoadFromXml(node, dt);

                        dt.Provider = _provider;

                        Nodes.Add(dt);
                    }
                }
            }
            return Nodes.GetEnumerator();
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
