//-----------------------------------------------------------------------------
// <copyright file="TypedEdmUntypedObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// Represents an <see cref="IEdmUntypedObject"/> backed by a CLR object without Edm type.
/// </summary>
internal class TypedEdmUntypedObject : TypedEdmStructuredObject, IEdmUntypedObject
{
    private ODataSerializerContext _context;
    public TypedEdmUntypedObject(ODataSerializerContext context, object instance)
        : base(instance, EdmUntypedStructuredTypeReference.NullableTypeReference, context?.Model)
    {
        _context = context;
    }

    public IDictionary<string, object> GetProperties()
    {
        IUntypedResourceMapper mapper = _context.UntypedMapper;
        return mapper.Map(Instance, _context);
    }
}
