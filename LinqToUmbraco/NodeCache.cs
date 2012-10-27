using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using meramedia.Linq.Core;
using meramedia.Linq.Core.Node;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

namespace meramedia.Linq.Core
{
    /// <summary>
    /// Class for handling the caching of the nodetrees
    /// </summary>    
    internal static class NodeCache
    {
        private static readonly Dictionary<UmbracoInfoAttribute, IContentTree> Trees = new Dictionary<UmbracoInfoAttribute, IContentTree>();
        private static readonly object CacheLock = new object();

        internal static bool ContainsKey(UmbracoInfoAttribute key)
        {
            return Trees.ContainsKey(key);
        }

        internal static void AddToCache(UmbracoInfoAttribute attr, IContentTree tree)
        {
            if (!Trees.ContainsKey(attr))
            {
                lock (CacheLock)
                {
                    Trees.Add(attr, tree);
                }
            }
        }

        internal static NodeTree<T> GetTree<T>(UmbracoInfoAttribute attr) where T : DocTypeBase, new()
        {
            return (NodeTree<T>) Trees[attr];            
        }

        internal static void ClearTrees()
        {
            lock (CacheLock)
            {
                Trees.Clear();
                Debug.WriteLine("All trees flushed!");
            }
        }

        internal static void ClearTreeForNode(Content changedNode)
        {
            lock (CacheLock)
            {
                var docType = changedNode.ContentType.Alias;
                var key = new UmbracoInfoAttribute(docType);
                ClearTree(key);
            }
        }

        internal static void ClearTree(UmbracoInfoAttribute key)
        {
            lock (CacheLock)
            {
                if (Trees.ContainsKey(key))
                    Trees.Remove(key);
                Debug.WriteLine(key.DisplayName + " tree flushed!");
            }  
        }

        internal static T GetNode<T>(int id) where T : DocTypeBase, new()
        {
            var tree = Trees.SingleOrDefault(x => x.Value is Tree<T>).Value;
            if (tree != null)
            {
                var node = ((Tree<T>)tree).SingleOrDefault(x => x.Id == id);
                if (node != null)
                    return node;
            }

            return null;
        }

        internal static IEnumerable<T> GetNodes<T>(int[] ids) where T : DocTypeBase, new()
        {
            var tree = Trees.SingleOrDefault(x => x.Value is Tree<T>).Value;
            if (tree != null)
            {
                foreach (int id in ids)
                {
                    var node = ((Tree<T>) tree).SingleOrDefault(x => x.Id == id);
                    if (node != null)
                        yield return node;
                }
            }         
        }

        internal static int NumTreesInCache()
        {
            return Trees.Count();
        }

        internal static int NumNodesInCache()
        {
            return Trees.Select(x => x.Value).Sum(i => i.Count);
        }
    }
}
