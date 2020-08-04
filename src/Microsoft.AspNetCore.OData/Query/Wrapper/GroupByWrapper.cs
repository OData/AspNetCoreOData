// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Query.Container;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    [JsonConverter(typeof(DynamicTypeWrapperConverter))]
    internal class GroupByWrapper : DynamicTypeWrapper
    {
        private Dictionary<string, object> _values;
        protected static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual AggregationPropertyContainer GroupByContainer { get; set; }

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual AggregationPropertyContainer Container { get; set; }

        public override Dictionary<string, object> Values
        {
            get
            {
                EnsureValues();
                return this._values;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var compareWith = obj as GroupByWrapper;
            if (compareWith == null)
            {
                return false;
            }
            var dictionary1 = this.Values;
            var dictionary2 = compareWith.Values;
            return dictionary1.Count == dictionary2.Count && !dictionary1.Except(dictionary2).Any();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            EnsureValues();
            long hash = 1870403278L; //Arbitrary number from Anonymous Type GetHashCode implementation
            foreach (var v in this.Values.Values)
            {
                hash = (hash * -1521134295L) + (v == null ? 0 : v.GetHashCode());
            }

            return (int)hash;
        }

        private void EnsureValues()
        {
            if (_values == null)
            {
                if (this.GroupByContainer != null)
                {
                    this._values = this.GroupByContainer.ToDictionary(DefaultPropertyMapper);
                }
                else
                {
                    this._values = new Dictionary<string, object>();
                }

                if (this.Container != null)
                {
                    _values.MergeWithReplace(this.Container.ToDictionary(DefaultPropertyMapper));
                }
            }
        }
    }
}