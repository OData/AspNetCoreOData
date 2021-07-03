// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
