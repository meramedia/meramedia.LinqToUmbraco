using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.Core.Dashboard
{
    internal class TemplateConstants
    {

        internal const string EXPORT_FOLDER = "/exported-doctypes/";

        internal const string POCO_TEMPLATE = @"using System;
            using System.Linq;
            using umbraco.Linq.Core;
            using System.Collections.Generic;

            namespace {0} {{
	            public partial class {1}DataContext : UmbracoDataContext {{
		            #region Partials
		            partial void OnCreated();
		            #endregion
		
		            public {1}DataContext() : base()
		            {{
			            OnCreated();
		            }}

		            {2}
	            }}

	            {3}
            }}";

        internal readonly static string TREE_TEMPLATE = @"
		public Tree<{0}> {0}s
		{{
			get
			{{
				return this.LoadTree<{0}>();
			}}
		}}";


        //0 - Alias
        //1 - class name
        //2 - interface or string.Empty
        //3 - properties
        //4 - child relationships
        //5 - interface explicit implementation
        //6 - description
        internal readonly static string CLASS_TEMPLATE = @"
	        /// <summary>
	        /// {6}
	        /// </summary>
	        [UmbracoInfo(""{0}"")]
	        [System.Runtime.Serialization.DataContractAttribute()]
	        [DocType()]
	        public partial class {1} : {7} {2} {{
		        public {1}() {{
		        }}
		        {3}
		        {4}
		        {5}
        }}";

        internal readonly static string PROPERTIES_TEMPLATE = @"
		private {0} _{1};
		/// <summary>
		/// {2}
		/// </summary>
		[UmbracoInfo(""{3}"", DisplayName = ""{4}"", Mandatory = {5})]
		[Property()]
		[System.Runtime.Serialization.DataMemberAttribute()]
		public virtual {0} {1}
		{{
			get
			{{
				return this._{1};
			}}
			set
			{{
				if ((this._{1} != value))
				{{
					this.RaisePropertyChanging();
					this._{1} = value;
                    this.IsDirty = true;
					this.RaisePropertyChanged(""{1}"");
				}}
			}}
		}}";

        internal readonly static string CHILD_RELATIONS_TEMPLATE = @"
		private AssociationTree<{0}> _{0}s;
		public AssociationTree<{0}> {0}s
		{{
			get
			{{
				if ((this._{0}s == null))
				{{
					this._{0}s = this.ChildrenOfType<{0}>();
				}}
				return this._{0}s;
			}}
			set
			{{
				this._{0}s = value;
			}}
		}}";

        internal readonly static string CHILD_RELATIONS_ABSTRACTION_TEMPLATE = @"
		IEnumerable<I{0}> I{1}.{0}s 
		{{
			get
			{{
				return this.{0}s.OfType<I{0}>();
			}}
		}}";
    }
}
