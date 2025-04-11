//-----------------------------------------------------------------------------
// <copyright file="TestPropertyMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Container;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Container
{
    internal class TestPropertyMapper : IPropertyMapper
    {
        public string MapProperty(string propertyName)
        {
            return propertyName;
        }
    }
}
