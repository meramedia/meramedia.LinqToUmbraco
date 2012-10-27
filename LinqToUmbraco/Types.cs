using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.helpers;

namespace meramedia.Linq.Core
{
    internal class Types
    {
        private static Dictionary<string, Type> _knownTypes;
        private static readonly object Locker = new object();

        internal static Dictionary<string, Type> KnownTypes
        {
            get
            {
                if (_knownTypes == null)
                {
                    lock (Locker)
                    {
                        _knownTypes = new Dictionary<string, Type>();
                        var types = TypeFinder
                            .FindClassesOfType<DocTypeBase>()
                            .Where(t => t != typeof(DocTypeBase))
                            .ToDictionary(k => ((UmbracoInfoAttribute)k.GetCustomAttributes(typeof(UmbracoInfoAttribute), true)[0]).Alias);

                        foreach (var type in types)
                            _knownTypes.Add(Casing.SafeAlias(type.Key), type.Value);
                    }
                }

                return _knownTypes;
            }
        }
    }
}
