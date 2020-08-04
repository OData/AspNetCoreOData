// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Contains the details of a given OData request. These properties should all be mutable.
    /// None of these properties should ever be set to null.
    /// </summary>
    /// <summary>
    /// Provides programmatic configuration for the OData service.
    /// </summary>
    public class ODataOptions
    {
        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not in DefaultODataPathHandler.
        /// </summary>
        /// <remarks>Default value is unspecified (null).</remarks>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// </summary>
        public bool NullDynamicPropertyIsEnabled { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating if batch requests should continue on error.
        /// </summary>
        public bool EnableContinueOnErrorHeader { get; set; }

        /// <summary>
        /// Gets or Sets the set of flags that have options for backward compatibility
        /// </summary>
        public CompatibilityOptions CompatibilityOptions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool EnableAttributeRouting { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, IEdmModel> Models { get; } = new Dictionary<string, IEdmModel>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useAttributeRouting"></param>
        /// <returns></returns>
        public ODataOptions UseAttributeRouting(bool useAttributeRouting)
        {
            EnableAttributeRouting = useAttributeRouting;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ODataOptions UseModel(IEdmModel model)
        {
            return UseModel(string.Empty, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ODataOptions UseModel(string name, IEdmModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (Models.ContainsKey(name))
            {
                throw new Exception($"Contains the same name for the model: {name}");
            }

            Models[name] = model;
            return this;
        }
    }
}
