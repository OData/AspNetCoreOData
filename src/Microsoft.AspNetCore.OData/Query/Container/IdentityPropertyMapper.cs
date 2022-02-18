//-----------------------------------------------------------------------------
// <copyright file="IdentityPropertyMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Container
{
    internal class IdentityPropertyMapper : IPropertyMapper
    {
        public string MapProperty(string propertyName)
        {
            return propertyName;
        }
    }
}
