//-----------------------------------------------------------------------------
// <copyright file="ODataRouteOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Represents the configurable options on a conventional routing template building.
    /// </summary>
    public class ODataRouteOptions
    {
        // The default route options.
        internal static readonly ODataRouteOptions Default = new ODataRouteOptions();

        private bool _enableKeyInParenthesis;
        private bool _enableKeyAsSegment;
        private bool _enableQualifiedOperationCall;
        private bool _enableUnqualifiedOperationCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteOptions" /> class.
        /// </summary>
        public ODataRouteOptions()
        {
            _enableKeyInParenthesis = true;
            _enableKeyAsSegment = true;
            _enableQualifiedOperationCall = true;
            _enableUnqualifiedOperationCall = true;
        }

        /// <summary>
        /// Gets/sets a value indicating whether to enable case insensitive for the controller name in conventional routing.
        /// </summary>
        public bool EnableControllerNameCaseInsensitive { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether to enable case insensitive for the property name in conventional routing.
        /// </summary>
        public bool EnablePropertyNameCaseInsensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/entityset({key}).
        /// Used in conventional routing.
        /// </summary>
        public bool EnableKeyInParenthesis
        {
            get => _enableKeyInParenthesis;
            set
            {
                if (!value && !_enableKeyAsSegment)
                {
                    throw new ODataException(SRResources.RouteOptionDisabledKeySegment);
                }

                _enableKeyInParenthesis = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/entityset/{key}
        /// Used in conventional routing.
        /// </summary>
        public bool EnableKeyAsSegment
        {
            get => _enableKeyAsSegment;
            set
            {
                if (!value && !_enableKeyInParenthesis)
                {
                    throw new ODataException(SRResources.RouteOptionDisabledKeySegment);
                }

                _enableKeyAsSegment = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/Namespace.MyFunction(parameters...)
        /// Used in conventional routing.
        /// </summary>
        public bool EnableQualifiedOperationCall
        {
            get => _enableQualifiedOperationCall;
            set
            {
                if (!value && !_enableUnqualifiedOperationCall)
                {
                    throw new ODataException(SRResources.RouteOptionDisabledOperationSegment);
                }

                _enableQualifiedOperationCall = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/MyFunction(parameters...)
        /// Used in conventional routing.
        /// </summary>
        public bool EnableUnqualifiedOperationCall
        {
            get => _enableUnqualifiedOperationCall;
            set
            {
                if (!value && !_enableQualifiedOperationCall)
                {
                    throw new ODataException(SRResources.RouteOptionDisabledOperationSegment);
                }

                _enableUnqualifiedOperationCall = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate non parenthesis for non-parameter function.
        /// Used in conventional routing.
        /// </summary>
        public bool EnableNonParenthesisForEmptyParameterFunction { get; set; } = false;
    }
}
