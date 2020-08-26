// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;

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
        public IDictionary<string, Action<IContainerBuilder>> PreRoutePrividers { get; } = new Dictionary<string, Action<IContainerBuilder>>();

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
        public ODataOptions AddModel(IEdmModel model)
        {
            return AddModel(string.Empty, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ODataOptions AddModel(string name, IEdmModel model)
        {
            return AddModel(name, model, null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public ODataOptions AddModel(string name, IEdmModel model, Action<IContainerBuilder> configureAction)
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
            PreRoutePrividers[name] = configureAction;
            return this;
        }

        #region Globle Query settings

        private int? _maxTop = 0;

        /// <summary>
        /// Gets or sets a value indicating whether navigation property can be expanded.
        /// </summary>
        public bool EnableExpand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether property can be selected.
        /// </summary>
        public bool EnableSelect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether entity set and property can apply $count.
        /// </summary>
        public bool EnableCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether property can apply $orderby.
        /// </summary>
        public bool EnableOrderBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether property can apply $filter.
        /// </summary>
        public bool EnableFilter { get; set; }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        /// <value>
        /// The max value of $top that a client can request, or <c>null</c> if there is no limit.
        /// </value>
        public int? MaxTop
        {
            get => _maxTop;
            set
            {
                if (value.HasValue && value < 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 0);
                }

                _maxTop = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the service will use skiptoken or not.
        /// </summary>
        public bool EnableSkipToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataOptions Expand()
        {
            EnableExpand = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataOptions Select()
        {
            EnableSelect = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataOptions Filter()
        {
            EnableFilter = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataOptions OrderBy()
        {
            EnableOrderBy = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataOptions Count()
        {
            EnableCount = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataOptions SkipToken()
        {
            EnableSkipToken = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxTopValue"></param>
        /// <returns></returns>
        public ODataOptions SetMaxTop(int? maxTopValue)
        {
            MaxTop = maxTopValue;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DefaultQuerySettings BuildDefaultQuerySettings()
        {
            DefaultQuerySettings settings = new DefaultQuerySettings();

            settings.EnableCount = EnableCount;
            settings.EnableExpand = EnableExpand;
            settings.EnableFilter = EnableFilter;
            settings.EnableOrderBy = EnableOrderBy;
            settings.EnableSelect = EnableSelect;
            settings.EnableSkipToken = EnableSkipToken;
            settings.MaxTop = MaxTop;

            return settings;
        }
        #endregion
    }
}
