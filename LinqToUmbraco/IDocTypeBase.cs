using System;
using System.ComponentModel;
namespace meramedia.Linq.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDocTypeBase
    {
        TDocType AncestorOrDefault<TDocType>() where TDocType : DocTypeBase;

        TDocType AncestorOrDefault<TDocType>(Func<TDocType, bool> func) where TDocType : DocTypeBase;

        AssociationTree<DocTypeBase> Children { get; }

        DateTime CreateDate { get;}

        int CreatorID { get; }

        string CreatorName { get; }

        int Id { get; }

        int Level { get; }

        string NodeName { get; }

        [Obsolete("Name property is obsolete, use NodeName instead")] //this is because most people expect NodeName not Name as the property
        string Name { get; }

        TParent Parent<TParent>() where TParent : DocTypeBase, new();

        int ParentNodeId { get;  }

        int SortOrder { get;  }

        int TemplateId { get;}

        DateTime UpdateDate { get;  }

        string Version { get; }

        int WriterID { get; }

        string WriterName { get; }

        string Path { get; }

        string Url { get;  }
    }
}
