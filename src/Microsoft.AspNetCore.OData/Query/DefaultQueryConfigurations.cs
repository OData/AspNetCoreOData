//-----------------------------------------------------------------------------
// <copyright file="DefaultQueryConfigurations.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.ModelBuilder.Config;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This class describes the default configurations to use during query composition.
    /// </summary>
    public class DefaultQueryConfigurations : DefaultQuerySettings
    {
        private int _maxSelectExpandNodeCount = ODataValidationSettings.DefaultMaxSelectExpandNodeCount;

        // We will add other query settings, for example, $compute, $search here
        // In the next major release, we should remove the inheritance from 'DefaultQuerySettings'.

        /// <summary>
        /// Gets or sets the global maximum number of projection ($select and $expand) items allowed across the whole
        /// projection tree of a single request, including items nested under an expanded navigation property and items
        /// projected from a $compute alias.
        /// </summary>
        /// <remarks>
        /// The default value is 500. Set to 0 to disable the check globally. Individual endpoints can override this value
        /// through <c>EnableQueryAttribute.MaxSelectExpandNodeCount</c> or <c>ODataValidationSettings.MaxSelectExpandNodeCount</c>.
        /// </remarks>
        public int MaxSelectExpandNodeCount
        {
            get => _maxSelectExpandNodeCount;
            set
            {
                if (value < 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(MaxSelectExpandNodeCount), value, 0);
                }

                _maxSelectExpandNodeCount = value;
            }
        }

        internal DefaultQueryConfigurations UpdateAll(DefaultQueryConfigurations options)
        {
            EnableExpand = options.EnableExpand;
            EnableSelect = options.EnableSelect;
            EnableFilter = options.EnableFilter;
            EnableOrderBy = options.EnableOrderBy;
            EnableCount = options.EnableCount;
            EnableSkipToken = options.EnableSkipToken;
            MaxTop = options.MaxTop;
            MaxSelectExpandNodeCount = options.MaxSelectExpandNodeCount;
            return this;
        }
    }
}
