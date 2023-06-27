using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ODataQueryBuilder.Query.Container
{
    internal class NamedProperty<T> : PropertyContainer
    {
        public string Name { get; set; }

        public T Value { get; set; }

        public bool AutoSelected { get; set; }

        public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
            bool includeAutoSelected)
        {
            Contract.Assert(dictionary != null);

            if (Name != null && (includeAutoSelected || !AutoSelected))
            {
                string mappedName = propertyMapper.MapProperty(Name);
                if (mappedName != null)
                {
                    if (String.IsNullOrEmpty(mappedName))
                    {
                        throw Error.InvalidOperation(SRResources.InvalidPropertyMapping, Name);
                    }

                    dictionary.Add(mappedName, GetValue());
                }
            }
        }

        public virtual object GetValue()
        {
            return Value;
        }
    }
}
