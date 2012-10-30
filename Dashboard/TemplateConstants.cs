using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace meramedia.Linq.Core.Dashboard
{
    internal class TemplateConstants
    {
        internal const string EXPORT_FOLDER = "/exported-doctypes/";

        internal const string POCO_TEMPLATE =
@"using System;
using System.Linq;
using System.Collections.Generic;
using meramedia.Linq.Core;

namespace {0} 
{{
	public partial class DataContext : UmbracoDataContext 
    {{
        private static readonly Lazy<DataContext> _instance = new Lazy<DataContext>(() => new DataContext());
        public static DataContext Instance
        {{
            get
            {{
                return _instance.Value;
            }}
            
        }}

		partial void OnCreated();
		protected DataContext() : base()
		{{
			OnCreated();
		}}

		{1}
	}}

	{2}
}}";

        internal readonly static string TREE_TEMPLATE = 
@"
        public Tree<{0}> {1}
        {{
	        get
	        {{
		        return LoadTree<{0}>();
	        }}
        }}";


        //0 - Alias
        //1 - class name
        //2 - interface or string.Empty
        //3 - properties
        //4 - child relationships
        //5 - interface explicit implementation
        //6 - description
        internal readonly static string CLASS_TEMPLATE = 
@"
        /// <summary>
        /// {4}
        /// </summary>
        [UmbracoInfo(""{0}"")]
        [System.Runtime.Serialization.DataContractAttribute()]
        [DocType()]
        public partial class {1} : {5} 
        {{
	        public {1}() {{
	        }}
	        {2}
	        {3}

        }}";

        internal readonly static string PROPERTIES_TEMPLATE = 
@"        
        /// <summary>
        /// {2}
        /// </summary>
        [UmbracoInfo(""{3}"", DisplayName = ""{4}"", Mandatory = {5})]
        [Property()]
        [System.Runtime.Serialization.DataMemberAttribute()]
        public virtual {0} {1}
        {{ get; set;}}";

        internal readonly static string CHILD_RELATIONS_TEMPLATE = 
@"
        private AssociationTree<{0}> _{0}s;
        public AssociationTree<{0}> {0}s
        {{
	        get
	        {{
		        if ((_{0}s == null))
		        {{
			        _{0}s = ChildrenOfType<{0}>();
		        }}
		        return _{0}s;
	        }}
	        set
	        {{
		        _{0}s = value;
	        }}
        }}";

    }
}
