using System;
using System.Collections.Generic;
using System.Linq;

namespace meramedia.Linq.Core.Node
{
    /// <summary>
    /// Represents a collection of TDocTypeBase retrieved from the umbraco XML cache which are direct children of a node
    /// </summary>
    /// <typeparam name="TDocTypeBase">The type of the doc type base.</typeparam>
    public sealed class NodeAssociationTree<TDocTypeBase> : AssociationTree<TDocTypeBase> where TDocTypeBase : DocTypeBase, new()
    {
        private readonly object _lockObject = new object();

        internal NodeAssociationTree(IEnumerable<TDocTypeBase> nodes)
        {
            //_nodes = new List<TDocTypeBase>();
            _nodes = nodes.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeAssociationTree&lt;TDocTypeBase&gt;"/> class for a particular tree section
        /// </summary>
        /// <param name="parentNodeId">The parent node id to start from.</param>
        /// <param name="provider">The NodeDataProvider to link the tree with.</param>
        public NodeAssociationTree(int parentNodeId, UmbracoDataProvider provider)
        {
            Provider = provider;
            ParentNodeId = parentNodeId;            
        }

        /// <summary>
        /// Gets the enumerator for this Tree collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<TDocTypeBase> GetEnumerator()
        {
            if (_nodes == null) //first access, otherwise it'd be cached
            {
                LoadNodes();
            }
            if (_nodes != null) return _nodes.GetEnumerator();
            else throw new InvalidOperationException("No nodes!");
        }

        private void LoadNodes()
        {
            var provider = Provider as NodeDataProvider;

            if (provider != null)
            {
                provider.CheckDisposed();

                lock (_lockObject)
                {
                    var parents = provider
                        .Xml
                        .Where(x => x.Attribute("id") != null && (int) x.Attribute("id") == ParentNodeId);
                    var rawNodes = parents
                        .Single()
                        .Elements()
                        .Where(x => x.Attribute("isDoc") != null);
                    _nodes = provider.DynamicNodeCreation(rawNodes).Cast<TDocTypeBase>().ToList(); //drop is back to the type which was asked for 
                }
            }
        }

        /// <summary>
        /// Gets or sets the DataProvider associated with this Tree
        /// </summary>
        /// <value>The provider.</value>
        public override UmbracoDataProvider Provider { get; protected set; }

        /// <summary>
        /// Reloads the cache.
        /// </summary>
        public override void ReloadCache()
        {
            LoadNodes();
        }

    }
}
