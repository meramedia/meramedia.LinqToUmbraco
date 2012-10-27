using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace meramedia.Linq.Core
{
    /// <summary>
    /// Provides the base framework for an umbraco item
    /// </summary>
    [DataContract]
    public class DocTypeBase : IDocTypeBase //This class should be abstract but it can't be done AND achieve the Children property like this
    {

        private int _parentId;
        private User _writer;
        private User _creator;
        private IEnumerable<DocTypeBase> _ancestors;
        private AssociationTree<DocTypeBase> _children;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeBase"/> class.
        /// </summary>
        public DocTypeBase()
        { }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        protected internal UmbracoDataProvider Provider { get; set; }


        #region Fields

        [Field, UmbracoInfo("id", DisplayName = "Id", Mandatory = true), DataMember(Name = "Id")]
        public virtual int Id { get; internal set; }

        [Field, UmbracoInfo("nodeName", DisplayName = "NodeName", Mandatory = true), DataMember(Name = "NodeName")]
        public virtual string NodeName { get; internal set; }

        [Field]
        [Obsolete("Name property is obsolete, use NodeName instead")] //this is because most people expect NodeName not Name as the property
        public virtual string Name
        {
            get
            {
                return NodeName;
            }
        }

        [Field, UmbracoInfo("template", DisplayName = "Template", Mandatory = true), DataMember(Name = "TemplateId")]
        public virtual int TemplateId { get; internal set; }

        [Field, UmbracoInfo("version", DisplayName = "Version", Mandatory = true), DataMember(Name = "Version")]
        public virtual string Version { get; internal set; }

        [Field]
        [UmbracoInfo("parentID", DisplayName = "ParentId", Mandatory = true), DataMember(Name = "ParentId")]
        public virtual int ParentNodeId
        {
            get
            {
                return _parentId;
            }
            internal set { _parentId = value; }
        }

        [Field]
        [UmbracoInfo("createDate", DisplayName = "CreateDate"), DataMember(Name = "CreateDate")]
        public virtual DateTime CreateDate { get; set; }

        [Field]
        [UmbracoInfo("sortOrder", DisplayName = "SortOrder"), DataMember(Name = "SortOrder")]
        public virtual int SortOrder { get; set; }

        [Field]
        [UmbracoInfo("updateDate", DisplayName = "UpdateDate"), DataMember(Name = "UpdateDate")]
        public virtual DateTime UpdateDate { get; set; }

        [Field]
        [UmbracoInfo("level", DisplayName = "Level"), DataMember(Name = "Level")]
        public virtual int Level { get; set; }

        [Field, UmbracoInfo("path", DisplayName = "Path"), DataMember(Name = "Path")]
        public virtual string Path { get; internal set; }

        #endregion

        #region Parents and Children
        /// <summary>
        /// Gets the children of this DocType instance.
        /// </summary>
        /// <value>The children of this DocType instance.</value>
        public AssociationTree<DocTypeBase> Children
        {
            get { return _children ?? (_children = Provider.LoadAssociation(Id)); }
        }

        /// <summary>
        /// Gets the children which are of the type TDocTypeBase.
        /// </summary>
        /// <typeparam name="TDocTypeBase">The DocType of the children desired.</typeparam>
        /// <returns>An <see cref="AssociationTree{TDocTypeBase}"/> of the children</returns>        
        protected AssociationTree<TDocTypeBase> ChildrenOfType<TDocTypeBase>() where TDocTypeBase : DocTypeBase, new()
        {
            return Provider.LoadAssociation(Children.Where(d => d is TDocTypeBase).Cast<TDocTypeBase>());
        }

        /// <summary>
        /// Parent this instance.
        /// </summary>
        /// <typeparam name="TParent">The type of the parent.</typeparam>
        /// <returns>Null when at the root level, else the parent instance</returns>
        /// <exception cref="DocTypeMismatchException">If the type of the parent does not match the provided type</exception>
        public virtual TParent Parent<TParent>() where TParent : DocTypeBase, new()
        {
            return _parentId == -1 ? null : Provider.Load<TParent>(_parentId);
        }

        /// <summary>
        /// Retrieves the first matching ancestor of the current type
        /// </summary>
        /// <remarks>
        /// Provides similar functionality to the XPath method
        /// </remarks>
        /// <returns>First ancestor matching type. Null if no match found</returns>        
        public TDocType AncestorOrDefault<TDocType>() where TDocType : DocTypeBase
        {
            return AncestorOrDefault<TDocType>(t => true); //just a simple little true statement ;)
        }

        /// <summary>
        /// Retrieves the first matching ancestor of the current type and additional boolean function
        /// </summary>
        /// <typeparam name="TDocType">The type of the doc type.</typeparam>
        /// <param name="func">Additional boolean operation to filter on</param>
        /// <returns>First ancestor matching type and function. Null if no match found</returns>
        /// <exception cref="System.ArgumentNullException">Func parameter required</exception>
        public TDocType AncestorOrDefault<TDocType>(Func<TDocType, bool> func) where TDocType : DocTypeBase
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (_ancestors == null)
            {
                _ancestors = Provider.LoadAncestors(Id);
            }

            return _ancestors.Where(a => a is TDocType).Cast<TDocType>().FirstOrDefault(func);
        }
        #endregion

        public int CreatorID { get; internal set; }

        public virtual User Creator
        {
            get { return _creator ?? (_creator = new User(CreatorID)); }
        }

        public virtual string CreatorName { get; internal set; }

        public int WriterID { get; internal set; }

        public virtual User Writer
        {
            get { return _writer ?? (_writer = new User(WriterID)); }
        }

        public virtual string WriterName { get; internal set; }

        protected void ValidateProperty(string regex, string value)
        {
            Regex r = new Regex(regex);
            if (!r.IsMatch(value))
            {
                throw new InvalidCastException("Value does not match validation expression from Umbraco");
            }
        }
    }
}
