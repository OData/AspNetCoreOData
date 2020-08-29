// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Batch;
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
    public class ODataOptions
    {
        #region Settings
        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// By default, it supports key as segment only if the key is single key.
        /// </summary>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; set; } = ODataUrlKeyDelimiter.Slash;

        /// <summary>
        /// Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically whether to recognize keys as segments or not.
        /// </summary>
        /// <param name="keyDelimiter">The key demimiter.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions SetUrlKeyDelimiter(ODataUrlKeyDelimiter keyDelimiter)
        {
            UrlKeyDelimiter = keyDelimiter;
            return this;
        }

        /// <summary>
        /// Gets or Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// By default, it's true;
        /// </summary>
        public bool OmitNullDynamicProperty { get; set; } = true;

        /// <summary>
        ///Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// </summary>
        /// <param name="enabled">The boolean value.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions SetNullDynamicProperty(bool enabled)
        {
            OmitNullDynamicProperty = enabled;
            return this;
        }

        /// <summary>
        /// Gets or Sets a value indicating if batch requests should continue on error.
        /// By default, it's false.
        /// </summary>
        public bool EnableContinueOnErrorHeader { get; set; }

        /// <summary>
        /// Sets a value indicating if batch requests should continue on error.
        /// </summary>
        /// <param name="enableContinueOnError">The boolean value.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions SetContinueOnErrorHeader(bool enableContinueOnError)
        {
            EnableContinueOnErrorHeader = enableContinueOnError;
            return this;
        }

        /// <summary>
        /// Gets or Sets the set of flags that have options for backward compatibility.
        /// </summary>
        public CompatibilityOptions CompatibilityOptions { get; set; }

        /// <summary>
        /// Sets the set of flags that have options for backward compatibility.
        /// </summary>
        /// <param name="enabled">The boolean value.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions SetCompatibilityOptions(bool enabled)
        {
            EnableAttributeRouting = enabled;
            return this;
        }

        /// <summary>
        /// Gets or Sets a value indicating if attribute routing is enabled or not.
        /// By default, it's enabled.
        /// </summary>
        public bool EnableAttributeRouting { get; set; } = true;

        /// <summary>
        /// Sets a value indicating if attribute routing is enabled or not.
        /// </summary>
        /// <param name="enabled">The boolean value.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions SetAttributeRouting(bool enabled)
        {
            EnableAttributeRouting = enabled;
            return this;
        }
        #endregion

        #region Models

        /// <summary>
        /// Gets the configured Edm models.
        /// </summary>
        public IDictionary<string, (IEdmModel, Action<IContainerBuilder>)> Models { get; } = new Dictionary<string, (IEdmModel, Action<IContainerBuilder>)>();

        /// <summary>
        /// Gets the configured service builder.
        /// </summary>
        public IDictionary<string, Action<IContainerBuilder>> PrePrefixPrividers { get; } = new Dictionary<string, Action<IContainerBuilder>>();

        /// <summary>
        /// Add an Edm model without prefix.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions AddModel(IEdmModel model)
        {
            return AddModel(string.Empty, model, configureAction: null);
        }

        /// <summary>
        /// Add a model without prefix using given batch handler.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="batchHandler">The batch handler <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions AddModel(IEdmModel model, ODataBatchHandler batchHandler)
        {
            return AddModel(string.Empty, model, builder => builder.AddService(ServiceLifetime.Singleton, sp => batchHandler));
        }

        /// <summary>
        /// Add a model with prefix.
        /// </summary>
        /// <param name="prefix">The model related prefix. It could be null which means there's no prefix when access this model.</param>
        /// <param name="model">The Edm model.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions AddModel(string prefix, IEdmModel model)
        {
            return AddModel(prefix, model, configureAction: null);
        }

        /// <summary>
        /// Add a model with prefix using given batch handler.
        /// </summary>
        /// <param name="prefix">The model related prefix. It could be null which means there's no prefix when access this model.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="batchHandler">The $batch handler <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions AddModel(string prefix, IEdmModel model, ODataBatchHandler batchHandler)
        {
            return AddModel(prefix, model, builder => builder.AddService(ServiceLifetime.Singleton, sp => batchHandler));
        }

        /// <summary>
        /// Adds OData model using the service configuration.
        /// </summary>
        /// <param name="prefix">The model related prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="configureAction">The sub service configuration action.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions AddModel(string prefix, IEdmModel model, Action<IContainerBuilder> configureAction)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (Models.ContainsKey(prefix))
            {
                throw Error.InvalidOperation(SRResources.ModelPrefixAlreadyUsed, prefix);
            }

            Models[prefix] = (model, configureAction);
            return this;
        }
        #endregion

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
        /// Enable $expand query options.
        /// </summary>
        /// <returns>The calling itself.</returns>
        public ODataOptions Expand()
        {
            EnableExpand = true;
            return this;
        }

        /// <summary>
        /// Enable $select query options.
        /// </summary>
        /// <returns>The calling itself.</returns>
        public ODataOptions Select()
        {
            EnableSelect = true;
            return this;
        }

        /// <summary>
        /// Enable $filter query options.
        /// </summary>
        /// <returns>The calling itself.</returns>
        public ODataOptions Filter()
        {
            EnableFilter = true;
            return this;
        }

        /// <summary>
        /// Enable $orderby query options.
        /// </summary>
        /// <returns>The calling itself.</returns>
        public ODataOptions OrderBy()
        {
            EnableOrderBy = true;
            return this;
        }

        /// <summary>
        /// Enable $count query options.
        /// </summary>
        /// <returns>The calling itself.</returns>
        public ODataOptions Count()
        {
            EnableCount = true;
            return this;
        }

        /// <summary>
        /// Enable $skiptop query option.
        /// </summary>
        /// <returns>The calling itself.</returns>
        public ODataOptions SkipToken()
        {
            EnableSkipToken = true;
            return this;
        }

        /// <summary>
        /// Setup the max top value.
        /// </summary>
        /// <param name="maxTopValue">The max top value.</param>
        /// <returns>The calling itself.</returns>
        public ODataOptions SetMaxTop(int? maxTopValue)
        {
            if (maxTopValue.HasValue && maxTopValue.Value < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxTopValue), maxTopValue, 0);
            }

            MaxTop = maxTopValue;
            return this;
        }

        /// <summary>
        /// Build the default QueryOption settings
        /// </summary>
        /// <returns>The default query options settings.</returns>
        internal DefaultQuerySettings BuildDefaultQuerySettings()
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
