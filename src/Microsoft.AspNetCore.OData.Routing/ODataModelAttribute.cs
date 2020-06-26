// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ODataModelAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public ODataModelAttribute(string model)
        {
            Model = model;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Model { get; }
    }
}
