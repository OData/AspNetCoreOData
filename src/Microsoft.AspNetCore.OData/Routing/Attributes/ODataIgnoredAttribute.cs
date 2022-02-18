//-----------------------------------------------------------------------------
// <copyright file="ODataIgnoredAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.OData.Routing.Attributes
{

    /// <summary>
    /// When used to decorate a <see cref="Controller"/> or Controller method, instructs OData to exclude that 
    /// item from the OData routing conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ODataIgnoredAttribute : Attribute
    {
    }

}
