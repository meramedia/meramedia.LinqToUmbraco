using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation;
using umbraco.cms.helpers;
using umbraco.BusinessLogic.Utils;

namespace meramedia.Linq.Core.Node
{
    /// <summary>
    /// Data Provider for LINQ to umbraco via umbraco nodes
    /// </summary>
    /// <remarks>
    /// <para>This class provides a data access model for the umbraco XML cache.
    /// It is responsible for the access to the XML and construction of nodes from it.</para>
    /// <para>The <see cref="meramedia.Linq.Core.Node.NodeDataProvider"/> is capable of reading the XML cache from either the path provided in the umbraco settings or from a specified location on the file system.</para>
    /// </remarks>
    public sealed class NodeDataProvider : UmbracoDataProvider
    {
        private static string XmlPath
        {
            get
            {
                return UmbracoContext.Current.Server.MapPath(UmbracoContext.Current.Server.ContentXmlPath);
            }
        }
     
        internal IEnumerable<XElement> Xml
        {
            get
            {
                var doc = UmbracoContext.Current.Server.ContentXml;
                if (doc == null)
                {
                    Debug.WriteLine("ALERT! Manually loading xml for linqtoumbraco");
                    return XDocument.Load(XmlPath).Descendants();
                }
                else
                {
                    Debug.WriteLine("Xml fetched for LinqToUmbraco");
                    return doc.Descendants();
                }                    
            }
        }

        public NodeDataProvider()
        {
            if (!File.Exists(XmlPath))
                throw new FileNotFoundException("The XML used by the provider must exist", XmlPath);        
        }

        /// <summary>
        /// Loads the tree with the relevant DocTypes from the XML
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <returns><see cref="meramedia.Linq.Core.Node.NodeTree&lt;TDocType&gt;"/> representation of the content tree</returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override Tree<TDocType> LoadTree<TDocType>()
        {
            CheckDisposed();

            var attr = ReflectionAssistance.GetUmbracoInfoAttribute(typeof(TDocType));

            if (!NodeCache.ContainsKey(attr))
                SetupNodeTree<TDocType>(attr);

            return NodeCache.GetTree<TDocType>(attr);
        }

        internal void SetupNodeTree<TDocType>(UmbracoInfoAttribute attr) where TDocType : DocTypeBase, new()
        {
            var tree = new NodeTree<TDocType>(this);
            NodeCache.AddToCache(attr, tree);
        }

        /// <summary>
        /// Loads the specified id.
        /// </summary>
        /// <typeparam name="TDocType">The type of the doc type.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <exception cref="DocTypeMismatchException">If the type of the parent does not match the provided type</exception>
        /// <exception cref="System.ArgumentException">No node found matching the provided ID for the parent</exception>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override TDocType Load<TDocType>(int id)
        {
            CheckDisposed();

            var parentXml = Xml.SingleOrDefault(d => d.Attribute("isDoc") != null && (int)d.Attribute("id") == id);

            if (!ReflectionAssistance.CompareByAlias(typeof(TDocType), parentXml))
                if (parentXml != null)
                    throw new DocTypeMismatchException(parentXml.Name.LocalName, ReflectionAssistance.GetUmbracoInfoAttribute(typeof(TDocType)).Alias);

            if (parentXml == null) //really shouldn't happen!
                throw new ArgumentException("Parent ID \"" + id + "\" cannot be found in the loaded XML. Ensure that the umbracoDataContext is being disposed of once it is no longer needed");

            var parent = new TDocType();
            LoadFromXml(parentXml, parent);

            return parent;
        }

        /// <summary>
        /// Loads the associated (children) nodes with the relivent DocTypes
        /// </summary>
        /// <param name="parentNodeId">The parent node id.</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override AssociationTree<DocTypeBase> LoadAssociation(int parentNodeId)
        {
            CheckDisposed();

            NodeAssociationTree<DocTypeBase> associationTree = new NodeAssociationTree<DocTypeBase>(parentNodeId, this);

            return associationTree;
        }


        /// <summary>
        /// Loads the associated nodes with the relivent DocTypes
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override AssociationTree<TDocType> LoadAssociation<TDocType>(IEnumerable<TDocType> nodes)
        {
            CheckDisposed();

            return new NodeAssociationTree<TDocType>(nodes);
        }

        /// <summary>
        /// Loads the ancestors for a node
        /// </summary>
        /// <param name="startNodeId">The start node id.</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override IEnumerable<DocTypeBase> LoadAncestors(int startNodeId)
        {
            CheckDisposed();

            var startElement = Xml.Single(x => x.Attribute("isDoc") != null && (int)x.Attribute("id") == startNodeId);
            var ancestorElements = startElement.Ancestors();

            IEnumerable<DocTypeBase> ancestors = DynamicNodeCreation(ancestorElements);

            return ancestors;
        }

        /// <summary>
        /// Creates a collection of nodes with the type specified from the XML
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <returns>Collecton of .NET types from the XML</returns>
        internal IEnumerable<DocTypeBase> DynamicNodeCreation(IEnumerable<XElement> elements)
        {
            foreach (XElement node in elements)
            {                
                Type t = Types.KnownTypes[Casing.SafeAlias(node.Name.LocalName)];
                DocTypeBase instaceOfT = (DocTypeBase)Activator.CreateInstance(t); //create an instance of the type and down-cast so we can use it
                LoadFromXml(node, instaceOfT);
                instaceOfT.Provider = this;

                yield return instaceOfT;
            }
        }



        /// <summary>
        /// Flushes the cache for this provider
        /// </summary>
        public override void Flush()
        {
            CheckDisposed();
            NodeCache.ClearTrees();
            MediaCache.Instance.Flush();
        }

        // clear cache for changed node. The next get will take care of the loading
        public override void NodeChanged(Content node)
        {
            NodeCache.ClearTreeForNode(node);            
        }

        /// <summary>
        /// Loads from XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">The XML.</param>
        /// <param name="node">The node.</param>
        public void LoadFromXml<T>(XElement xml, T node) where T : DocTypeBase
        {
            if (!ReflectionAssistance.CompareByAlias(node.GetType(), xml))
            {
                throw new DocTypeMismatchException(xml.Name.LocalName, ReflectionAssistance.GetUmbracoInfoAttribute(node.GetType()).Alias);
            }

            SetValuesFromXml(xml, node);

            var properties = node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttributes(typeof(PropertyAttribute), true).Any());
            foreach (var p in properties)
            {
                var attr = ReflectionAssistance.GetUmbracoInfoAttribute(p);

                XElement propertyXml = xml.Element(Casing.SafeAlias(attr.Alias));
                string data = null;
                //if the XML doesn't contain the property it means that the node hasn't been re-published with the property
                //so then we'll leave the data at null, otherwise let's grab it
                //Check if the propertyXml and propertyXml.FirstNode aren't null. If they are we don't need to set the value.
                if (propertyXml != null && propertyXml.FirstNode != null)
                {
                    //If the FirstNode is an XElement it means the property contains inner xml and we should return it. Otherwise just return the normal value.
                    if (propertyXml.FirstNode is XElement)
                    {
                        var reader = propertyXml.CreateReader();
                        reader.MoveToContent();
                        data = reader.ReadInnerXml();
                    }
                    else
                    {
                        data = propertyXml.Value;
                    }
                }

                if (p.PropertyType.IsValueType && p.PropertyType.GetGenericArguments().Length > 0 && typeof(Nullable<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
                {
                    if (string.IsNullOrEmpty(data))
                    {
                        //non-mandatory structs which have no value will be null
                        try
                        {
                            p.SetValue(node, null, null);
                        }
                        catch (FormatException ex)
                        {
                            throw new FormatException(
                                string.Format("Unable to cast '{0}' to the appropriate type ({1}) for node `{2}`. The alias of the property being parsed is {3}. Refer to inner exception for more details", data, p.PropertyType.FullName, node.Id, attr.Alias), ex);
                        }
                    }
                    else
                    {
                        //non-mandatory structs which do have a value have to be cast based on the type of their Nullable<T>, found from the first (well, only) GenericArgument
                        try
                        {
                            p.SetValue(node, Convert.ChangeType(data, p.PropertyType.GetGenericArguments()[0]), null);
                        }
                        catch (FormatException ex)
                        {
                            throw new FormatException(
                                string.Format("Unable to cast '{0}' to the appropriate type ({1}) for node `{2}`. The alias of the property being parsed is {3}. Refer to inner exception for more details", data, p.PropertyType.FullName, node.Id, attr.Alias), ex);
                        }
                    }
                }
                else
                {
                    // TODO: Address how Convert.ChangeType works in globalisation
                    try
                    {
                        p.SetValue(node, Convert.ChangeType(data, p.PropertyType), null);
                    }
                    catch (FormatException ex)
                    {
                        throw new FormatException(
                            string.Format("Unable to cast '{0}' to the appropriate type ({1}) for node `{2}`. The alias of the property being parsed is {3}. Refer to inner exception for more details", data, p.PropertyType.FullName, node.Id, attr.Alias), ex);
                    }
                }
            }
        }

        private static T SetValuesFromXml<T>(XElement xml) where T : DocTypeBase, new()
        {
            return new T
                {
                    Id = (int) xml.Attribute("id"),
                    ParentNodeId = (int) xml.Attribute("parentID"),
                    NodeName = (string) xml.Attribute("nodeName"),
                    Version = (string) xml.Attribute("version"),
                    CreateDate = (DateTime) xml.Attribute("createDate"),
                    SortOrder = (int) xml.Attribute("sortOrder"),
                    UpdateDate = (DateTime) xml.Attribute("updateDate"),
                    CreatorID = (int) xml.Attribute("creatorID"),
                    CreatorName = (string) xml.Attribute("creatorName"),
                    WriterID = (int) xml.Attribute("writerID"),
                    WriterName = (string) xml.Attribute("writerName"),
                    Level = (int) xml.Attribute("level"),
                    TemplateId = (int) xml.Attribute("template"),
                    Path = (string) xml.Attribute("path")
                };
        }
        private static void SetValuesFromXml<T>(XElement xml, T node) where T : DocTypeBase
        {
            node.Id = (int) xml.Attribute("id");
            node.ParentNodeId = (int) xml.Attribute("parentID");
            node.NodeName = (string) xml.Attribute("nodeName");
            node.Version = (string) xml.Attribute("version");
            node.CreateDate = (DateTime) xml.Attribute("createDate");
            node.SortOrder = (int) xml.Attribute("sortOrder");
            node.UpdateDate = (DateTime) xml.Attribute("updateDate");
            node.CreatorID = (int) xml.Attribute("creatorID");
            node.CreatorName = (string) xml.Attribute("creatorName");
            node.WriterID = (int) xml.Attribute("writerID");
            node.WriterName = (string) xml.Attribute("writerName");
            node.Level = (int) xml.Attribute("level");
            node.TemplateId = (int) xml.Attribute("template");
            node.Path = (string) xml.Attribute("path");

        }

        internal override DocTypeBase Find(int id)
        {
            return Find<DocTypeBase>(id);
        }

        internal override T Find<T>(int id)
        {
            if (typeof(T) != typeof(DocTypeBase))
            {
                var attr = ReflectionAssistance.GetUmbracoInfoAttribute(typeof (T));
                if (!NodeCache.ContainsKey(attr))
                    SetupNodeTree<T>(attr);
            }
            var node = NodeCache.GetNode<T>(id);
            if (node != null)
                return node;
            else            
            {
                Debug.WriteLine("Node not found in cache, trying xml..");
                var xmlNode = Xml.SingleOrDefault(d => d.Attribute("isDoc") != null && (int)d.Attribute("id") == id);
                return SetValuesFromXml<T>(xmlNode);
            }            
        }


        internal override IEnumerable<DocTypeBase> FindAll(int[] ids)
        {
            return FindAll<DocTypeBase>(ids);
        }

        internal override IEnumerable<T> FindAll<T>(int[] ids)
        {
            if (typeof(T) != typeof(DocTypeBase))
            {
                var attr = ReflectionAssistance.GetUmbracoInfoAttribute(typeof(T));
                if (!NodeCache.ContainsKey(attr))
                    SetupNodeTree<T>(attr);
            }


            List<T> nodes = NodeCache.GetNodes<T>(ids).ToList();
            if (nodes.Any())
                foreach (T n in nodes)
                    yield return n;
            else
            {
                foreach (int id in ids)
                {
                    Debug.WriteLine("Node not found in cache, trying xml..");
                    var xmlNode = Xml.SingleOrDefault(d => d.Attribute("isDoc") != null && (int)d.Attribute("id") == id);
                    yield return SetValuesFromXml<T>(xmlNode);
                }
            }  
        }
    }
}
