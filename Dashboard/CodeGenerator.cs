using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;

namespace meramedia.Linq.Core.Dashboard
{
    internal class CodeGenerator
    {
        private readonly Dictionary<Guid, Type> _dataTypeMapping = new Dictionary<Guid, Type>();
        private List<DocumentType> _docTypes;
        public List<DocumentType> DocTypes
        {
            get { return _docTypes ?? (_docTypes = DocumentType.GetAllAsList()); }
        }

        internal string GenerateClasses()
        {
            var sb = new StringBuilder();

            foreach (var dt in DocTypes)
            {
                string className = GenerateTypeName(dt.Alias);

                var baseType = "DocTypeBase";
                if (dt.MasterContentType > 0)
                {
                    var parent = DocTypes.First(d => d.Id == dt.MasterContentType);
                    baseType = GenerateTypeName(parent.Alias);
                }

                sb.Append(string.Format(TemplateConstants.CLASS_TEMPLATE,
                    dt.Alias,
                    className,
                    String.Empty, // fixme
                    GenerateProperties(dt),
                    GenerateChildRelationships(dt),
                    String.Empty, // fixme
                    FormatForComment(dt.Description),
                    baseType
                    )
                );
            }

            return sb.ToString();
        }

        internal object GenerateChildRelationships(ContentType dt)
        {
            var sb = new StringBuilder();
            var children = dt.AllowedChildContentTypeIDs;
            foreach (var child in DocTypes.Where(d => children.Contains(d.Id)))
            {
                sb.Append(string.Format(TemplateConstants.CHILD_RELATIONS_TEMPLATE,
                    GenerateTypeName(child.Alias)
                    )
                );
            }
            return sb.ToString();
        }

        internal object GenerateProperties(ContentType dt)
        {
            var sb = new StringBuilder();

            foreach (var pt in
                dt.getVirtualTabs.Where(x => x.ContentType == dt.Id).SelectMany(x => x.GetPropertyTypes(dt.Id))
                .Concat(dt.PropertyTypes.Where(x => x.ContentTypeId == dt.Id && x.TabId == 0))
                )
            {
                sb.Append(string.Format(TemplateConstants.PROPERTIES_TEMPLATE,
                    GetDotNetType(pt),
                    GenerateTypeName(pt.Alias),
                    FormatForComment(pt.Description),
                    pt.Alias,
                    pt.Name,
                    pt.Mandatory.ToString().ToLower()
                    )
                );
            }

            return sb.ToString();
        }

        internal string GetDotNetType(PropertyType pt)
        {
            Guid id = pt.DataTypeDefinition.DataType.Id;
            if (!_dataTypeMapping.ContainsKey(id))
            {
                var defaultData = pt.DataTypeDefinition.DataType.Data as DefaultData;
                if (defaultData != null) //first lets see if it inherits from DefaultData, pretty much all do
                {
                    switch (defaultData.DatabaseType)
                    {
                        case DBTypes.Integer:
                            _dataTypeMapping.Add(id, typeof(int));
                            break;
                        case DBTypes.Date:
                            _dataTypeMapping.Add(id, typeof(DateTime));
                            break;
                        case DBTypes.Nvarchar:
                        case DBTypes.Ntext:
                            _dataTypeMapping.Add(id, typeof(string));
                            break;
                        default:
                            _dataTypeMapping.Add(id, typeof(object));
                            break;
                    }
                }
                else //hmm so it didn't, lets try something else
                {
                    var dbType = Application.SqlHelper.ExecuteScalar<string>(@"SELECT [t0].[dbType] FROM [cmsDataType] AS [t0] WHERE [t0].[controlId] = @p0", Application.SqlHelper.CreateParameter("@p0", id));

                    if (!string.IsNullOrEmpty(dbType)) //can I determine from the DB?
                    {
                        switch (dbType.ToUpper())
                        {
                            case "INTEGER":
                                _dataTypeMapping.Add(id, typeof(int));
                                break;
                            case "DATE":
                                _dataTypeMapping.Add(id, typeof(DateTime));
                                break;
                            case "NTEXT":
                            case "NVARCHAR":
                                _dataTypeMapping.Add(id, typeof(string));
                                break;
                            default:
                                _dataTypeMapping.Add(id, typeof(object));
                                break;
                        }
                    }
                    else
                    {
                        //ok, you've got a really freaky data type, so you get an Object back :P
                        _dataTypeMapping.Add(id, typeof(object));
                    }
                }
            }
            //if it's a valueType and it's not a mandatory field we'll make it nullable. And let's be lazy and us something like 'int?' rather than
            //the fully layed out version :P
            if (!pt.Mandatory && _dataTypeMapping[id].IsValueType)
                return _dataTypeMapping[id].Name + "?";

            //here we can use a standard type name
            return _dataTypeMapping[id].Name;
        }

        internal string GenerateDataContextCollections()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string className in DocTypes.Select(dt => GenerateTypeName(dt.Alias)))
                sb.Append(string.Format(TemplateConstants.TREE_TEMPLATE, className));

            return sb.ToString();
        }

        internal static string GenerateTypeName(string alias)
        {
            string s = Casing.SafeAlias(alias);
            return s[0].ToString().ToUpper() + s.Substring(1, s.Length - 1);
        }

        internal static string FormatForComment(string s)
        {
            return string.IsNullOrEmpty(s) ? s : s.Replace("\r\n", "\r\n///");
        }
    }
}
