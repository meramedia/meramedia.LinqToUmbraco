
namespace meramedia.Linq.Core
{
    /// <summary>
    /// Base of an umbraco content tree
    /// </summary>
    public interface IContentTree
    {
        /// <summary>
        /// Gets the <see cref="UmbracoDataProvider"/> Provider associated with this instance
        /// </summary>
        /// <value>The provider.</value>
        UmbracoDataProvider Provider { get; }

        /// <summary>
        /// Reloads the cache for the particular tree
        /// </summary>
        void ReloadCache();

        int Count { get;  }
    }
}
