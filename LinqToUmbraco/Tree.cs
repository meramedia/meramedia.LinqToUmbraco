using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace meramedia.Linq.Core
{
    /// <summary>
    /// Represents a collection within DataProvider of a DocType
    /// </summary>
    /// <remarks>
    /// Similar to the implementation of <see cref="System.Data.Linq.Table&lt;TEntity&gt;"/>, 
    /// providing a single collection which represents all instances of the given type within the DataProvider.
    /// 
    /// Implementers of this type will need to provide a manner of retrieving the TDocType from the DataProvider
    /// </remarks>
    /// <typeparam name="TDocType">The type of the DocType.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class Tree<TDocType> : IContentTree, IEnumerable<TDocType>
        where TDocType : DocTypeBase, new()
    {
        private List<TDocType> _nodes;

        /// <summary>
        /// Gets the <see cref="umbracoDataProvider"/> Provider associated with this instance
        /// </summary>
        /// <value>The provider.</value>
        public abstract UmbracoDataProvider Provider { get; protected set; }

        /// <summary>
        /// Manually reload cache
        /// </summary>
        public abstract void ReloadCache();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public abstract IEnumerator<TDocType> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<TDocType> FindAll(int[] ids)
        {
            return ids.Select(id => _nodes.SingleOrDefault(x => x.Id == id));
        }

        public TDocType Find(int id)
        {
            return _nodes.SingleOrDefault(x => x.Id == id);
        }
    }
}
