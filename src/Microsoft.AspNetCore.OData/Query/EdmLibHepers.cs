using Microsoft.AspNetCore.OData.Query.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Query
{
    public static class EdmLibQueryHelpers
    {
        public static bool IsDynamicTypeWrapper(Type type)
        {
            return (type != null && typeof(DynamicTypeWrapper).IsAssignableFrom(type));
        }
    }
}
