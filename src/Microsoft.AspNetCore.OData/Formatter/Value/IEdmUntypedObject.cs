//-----------------------------------------------------------------------------
// <copyright file="IEdmUntypedObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an instance of an untyped structured object.
    /// Untyped means this structured Edm type is "Edm.Untyped" or no Edm type.
    /// </summary>
    public interface IEdmUntypedObject : IEdmStructuredObject
    {
    }
}
