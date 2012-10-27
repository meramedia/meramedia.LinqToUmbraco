using System;
using System.Collections.Generic;
using System.Diagnostics;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace meramedia.Linq.Core
{
    /// <summary>
    /// Provides the methods required for a data access model within the LINQ to Umbraco project
    /// </summary>
    /// <remarks>
    /// This base class is used when defining how a DataProvider operates against a data source (such as the umbraco.config).
    /// 
    /// It provides abstractions for all the useful operations of the DataProvider
    /// </remarks>
    public abstract class UmbracoDataProvider : IDisposable, IActionHandler
    {
        protected UmbracoDataProvider()
        {
            Debug.WriteLine("Dataprovider instantiated");

            Document.AfterPublish += Document_AfterPublish;
            Document.AfterUnPublish += Document_AfterUnPublish;
            Document.AfterDelete += Document_AfterDelete;
            Document.AfterMoveToTrash += Document_AfterMoveToTrash;

            // Republish entire site
            umbraco.content.AfterRefreshContent += content_AfterRefreshContent;
            
            Media.AfterNew += Media_AfterNew;
            Media.AfterMoveToTrash += Media_AfterMoveToTrash;
            Media.AfterDelete += Media_AfterDelete;
        }

        private static void Media_AfterDelete(Media sender, DeleteEventArgs e)
        {
            MediaCache.Instance.Flush();
        }

        private static void Media_AfterMoveToTrash(Media sender, MoveToTrashEventArgs e)
        {
            MediaCache.Instance.Flush();
        }

        private static void Media_AfterNew(object sender, NewEventArgs e)
        {
            MediaCache.Instance.Flush();
        }

        private void content_AfterRefreshContent(Document sender, RefreshContentEventArgs e)
        {
            Debug.WriteLine("All Trees flushed! - RefreshContent");   
            Flush();
        }

        private void Document_AfterPublish(Document sender, PublishEventArgs e)
        {
            Debug.WriteLine("Nodetree flushed! - AfterPublish");            
            NodeChanged(sender);
        }

        private void Document_AfterUnPublish(Document sender, UnPublishEventArgs e)
        {
            Debug.WriteLine("Nodetree flushed! - AfterUnPublish");
            NodeChanged(sender);
        }

        private void Document_AfterDelete(Document sender, DeleteEventArgs e)
        {
            Debug.WriteLine("Nodetree flushed! - AfterDelete");
            NodeChanged(sender);
        }

        private void Document_AfterMoveToTrash(Document sender, MoveToTrashEventArgs e)
        {
            Debug.WriteLine("Nodetree flushed! - AfterMoveToTrash");
            NodeChanged(sender);
        }

        public abstract void Flush();
        public abstract void NodeChanged(Content node);

        /// <summary>
        /// Indicates the disposal status of the current provider
        /// </summary>
        protected bool Disposed;

        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        /// <value>The name of the provider.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Loads the tree with the relevant DocTypes
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <returns></returns>
        public abstract Tree<TDocType> LoadTree<TDocType>() where TDocType : DocTypeBase, new();

        /// <summary>
        /// Loads the associated nodes with the relevant DocTypes
        /// </summary>
        /// <param name="parentNodeId">The parent node id.</param>
        /// <returns></returns>
        public abstract AssociationTree<DocTypeBase> LoadAssociation(int parentNodeId);

        /// <summary>
        /// Loads the associated nodes with the relevant DocTypes
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public abstract AssociationTree<TDocType> LoadAssociation<TDocType>(IEnumerable<TDocType> nodes) where TDocType : DocTypeBase, new();

        /// <summary>
        /// Loads the specified id.
        /// </summary>
        /// <typeparam name="TDocType">The type of the doc type.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public abstract TDocType Load<TDocType>(int id) where TDocType : DocTypeBase, new();

        /// <summary>
        /// Loads the ancestors.
        /// </summary>
        /// <param name="startNodeId">The start node id.</param>
        /// <returns></returns>
        public abstract IEnumerable<DocTypeBase> LoadAncestors(int startNodeId);

        #region IDisposable Members

        /// <summary>
        /// Checks if the provider has been disposed
        /// </summary>
        protected internal void CheckDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException(null);
        }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        protected internal virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }

        #endregion


        // Flush cache on Sort
        // NOTE: There is a bug right now (2012-10-20) in umbraco so when sorting top level nodes the action is not triggered
        public bool Execute(Document sender, IAction action)
        {
            if (action == ActionSort.Instance)
            {
                Debug.WriteLine("Nodetree flushed! - Sort");
                NodeChanged(sender);
            }
                

            return true;
        }

        public string HandlerName()
        {
            return "meramedia.Linq.Core.UmbracoDataProvider.SortActionHandler";
        }

        public IAction[] ReturnActions()
        {
            return new IAction[] { ActionSort.Instance };
        }


        internal abstract DocTypeBase Find(int id);
        internal abstract T Find<T>(int id) where T : DocTypeBase, new();
        internal abstract IEnumerable<DocTypeBase> FindAll(int[] id);
        internal abstract IEnumerable<T> FindAll<T>(int[] id) where T : DocTypeBase, new();
    }
}
