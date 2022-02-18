//-----------------------------------------------------------------------------
// <copyright file="PageResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.Results
{
    /// <summary>
    /// Represents a feed of entities that includes additional information that OData formats support.
    /// </summary>
    /// <remarks>
    /// Currently limited to:
    /// <list type="bullet">
    /// <item><description>The Count of all matching entities on the server (requested using $count=true).</description></item>
    /// <item><description>The NextLink to retrieve the next page of results (added if the server enforces Server Driven Paging).</description></item>
    /// </list>
    /// </remarks>
    [DataContract]
    public abstract class PageResult
    {
        private long? _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageResult"/> class.
        /// </summary>
        /// <param name="nextPageLink">The link for the next page of items in the feed.</param>
        /// <param name="count">The total count of items in the feed.</param>
        protected PageResult(Uri nextPageLink, long? count)
        {
            NextPageLink = nextPageLink;
            Count = count;
        }

        /// <summary>
        /// Gets the link for the next page of items in the feed.
        /// </summary>
        [DataMember]
        public Uri NextPageLink { get; }

        /// <summary>
        /// Gets the total count of items in the feed.
        /// </summary>
        [DataMember]
        public long? Count
        {
            get
            {
                return _count;
            }
            private set
            {
                if (value.HasValue && value.Value < 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value.Value, 0);
                }

                _count = value;
            }
        }

        /// <summary>
        /// Projects the result to a <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> representing the page result.</returns>
        public abstract IDictionary<string, object> ToDictionary();
    }
}
