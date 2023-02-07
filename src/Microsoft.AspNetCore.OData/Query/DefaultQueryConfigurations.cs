//-----------------------------------------------------------------------------
// <copyright file="DefaultQueryConfigurations.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This class describes the default configurations to use during query composition.
    /// </summary>
    public class DefaultQueryConfigurations
    {
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
    }
}
