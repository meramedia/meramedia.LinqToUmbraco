﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Provides the base framework for an umbraco item
    /// </summary>
    [DataContract]
    public class DocTypeBase : IDocTypeBase //This class should be abstract but it can't be done AND achieve the Children property like this
    {
        #region Internal Storage
        private int _id;
        private string _nodeName;
        private string _versionId;
        private int _templateId;
        private int _parentId;
        private User _writer;
        private User _creator;
        private string _path;
        private IEnumerable<DocTypeBase> _ancestors;
        private AssociationTree<DocTypeBase> _children;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeBase"/> class.
        /// </summary>
        public DocTypeBase()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        protected internal UmbracoDataProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has been modified since it was first loaded
        /// </summary>
        /// <value><c>true</c> if this instance has been modified; otherwise, <c>false</c>.</value>
        public bool IsDirty { get; protected internal set; }

        #region Fields
        /// <summary>
        /// Gets or sets the id of the umbraco item
        /// </summary>
        /// <value>The id.</value>
        [Field]
        [UmbracoInfo("id", DisplayName = "Id", Mandatory = true), DataMember(Name = "Id")]
        public virtual int Id
        {
            get
            {
                return _id;
            }
            protected internal set
            {
                if (_id != value)
                {
                    RaisePropertyChanging();
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name (title) of the umbraco item
        /// </summary>
        /// <value>The name.</value>
        [Field]
        [UmbracoInfo("nodeName", DisplayName = "NodeName", Mandatory = true), DataMember(Name = "NodeName")]
        public virtual string NodeName
        {
            get
            {
                return _nodeName;
            }
            set
            {
                if (_nodeName != value)
                {
                    RaisePropertyChanging();
                    _nodeName = value;
                    IsDirty = true;
                    RaisePropertyChanged("NodeName");
                }
            }
        }

        [Field]
        [Obsolete("Name property is obsolete, use NodeName instead")] //this is because most people expect NodeName not Name as the property
        public virtual string Name
        {
            get
            {
                return NodeName;
            }
            set
            {
                NodeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the template ID of the umbraco item
        /// </summary>
        /// <value>The name.</value>
        [Field]
        [UmbracoInfo("template", DisplayName = "Template", Mandatory = true), DataMember(Name = "TemplateId")]
        public virtual int TemplateId
        {
            get
            {
                return _templateId;
            }
            set
            {
                if (_templateId != value)
                {
                    RaisePropertyChanging();
                    _templateId = value;
                    IsDirty = true;
                    RaisePropertyChanged("Template");
                }
            }
        }

        /// <summary>
        /// Gets or sets the version of the umbraco item
        /// </summary>
        /// <value>The version.</value>
        [Field]
        [UmbracoInfo("version", DisplayName = "Version", Mandatory = true), DataMember(Name = "Version")]
        public virtual string Version
        {
            get
            {
                return _versionId;
            }
            protected internal set
            {
                if (_versionId != value)
                {
                    RaisePropertyChanging();
                    _versionId = value;
                    RaisePropertyChanged("Version");
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the parent node.
        /// </summary>
        /// <value>The parent node id.</value>
        [Field]
        [UmbracoInfo("parentID", DisplayName = "ParentId", Mandatory = true), DataMember(Name = "ParentId")]
        public virtual int ParentNodeId
        {
            get
            {
                return _parentId;
            }
            set
            {
                if (_parentId != value)
                {
                    RaisePropertyChanging();
                    _parentId = value;
                    RaisePropertyChanged("ParentId");
                }
            }
        }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>The create date.</value>
        [Field]
        [UmbracoInfo("createDate", DisplayName = "CreateDate"), DataMember(Name = "CreateDate")]
        public virtual DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        [Field]
        [UmbracoInfo("sortOrder", DisplayName = "SortOrder"), DataMember(Name = "SortOrder")]
        public virtual int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the last updated date of the current version
        /// </summary>
        /// <value>The update date.</value>
        [Field]
        [UmbracoInfo("updateDate", DisplayName = "UpdateDate"), DataMember(Name = "UpdateDate")]
        public virtual DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the level of this item in the content tree
        /// </summary>
        /// <value>The level.</value>
        [Field]
        [UmbracoInfo("level", DisplayName = "Level"), DataMember(Name = "Level")]
        public virtual int Level { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        [Field]
        [UmbracoInfo("path", DisplayName = "Path")]
        [DataMember(Name = "Path")]
        public virtual string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if (_path != value)
                {
                    RaisePropertyChanging();
                    _path = value;
                    RaisePropertyChanged("Path");
                }
            }
        }
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
        /// <returns>An <see cref="umbraco.Linq.Core.AssociationTree&lt;TDocTypeBase&gt;"/> of the children</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
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
                throw new ArgumentNullException("func");

            if (_ancestors == null)
                _ancestors = Provider.LoadAncestors(Id);

            return _ancestors.Where(a => a is TDocType).Cast<TDocType>().FirstOrDefault(func);
        }
        #endregion

        /// <summary>
        /// Gets or sets the creator ID.
        /// </summary>
        /// <value>The creator ID.</value>
        public int CreatorID { get; internal set; }

        /// <summary>
        /// Gets the umbraco user who created the item
        /// </summary>
        /// <value>The creator.</value>
        public virtual User Creator
        {
            get { return _creator ?? (_creator = new User(CreatorID)); }
        }

        public virtual string CreatorName { get; internal set; }

        /// <summary>
        /// ID of the user who last edited the item
        /// </summary>
        public int WriterID { get; internal set; }

        /// <summary>
        /// Gets the umbraco user who last edited the instance
        /// </summary>
        /// <value>The writer.</value>
        public virtual User Writer
        {
            get { return _writer ?? (_writer = new User(WriterID)); }
        }

        public virtual string WriterName { get; internal set; }

        /// <summary>
        /// Raises the property changing event.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected virtual void RaisePropertyChanging()
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(String.Empty));
            }
        }

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="name">The name of the changed property.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected virtual void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #region INotifyPropertyChanging Members

        /// <summary>
        /// Occurs when a property value is changing.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

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
