//-----------------------------------------------------------------------------
// <copyright file="TestComputeWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper
{
    internal class TestComputeWrapper<T> : TestGroupByWrapper, IComputeWrapper<T>
    {
        private bool merged;
        public T Instance { get; set; }
        public IEdmModel Model { get; set; }

        public override Dictionary<string, object> Values
        {
            get
            {
                EnsureValues();
                return base.Values;
            }
        }

        private void EnsureValues()
        {
            if (!this.merged)
            {
                // Properties available via Instance can be structural properties or generated in previous transformations
                if (this.Instance is DynamicTypeWrapper instanceContainer)
                {
                    // Add properties generated in previous transformations to the dictionary
                    base.Values.MergeWithReplace(instanceContainer.Values);
                }
                else
                {
                    // Add structural properties to the dictionary
                    // We need to use injected model to real property names
                    var structuredTypeReference = Model.GetEdmTypeReference(typeof(T)) as IEdmStructuredTypeReference;

                    TypedEdmStructuredObject typedEdmStructuredObject;
                    if (structuredTypeReference is IEdmComplexTypeReference complexTypeReference)
                    {
                        typedEdmStructuredObject = new TypedEdmComplexObject(Instance, complexTypeReference, Model);
                    }
                    else
                    {
                        var entityTypeReference = structuredTypeReference as IEdmEntityTypeReference;
                        typedEdmStructuredObject = new TypedEdmEntityObject(Instance, entityTypeReference, Model);
                    }

                    var primitiveProperties = structuredTypeReference.DeclaredStructuralProperties().Where(p => p.Type.IsPrimitive()).Select(p => p.Name);
                    foreach (var propertyName in primitiveProperties)
                    {
                        if (typedEdmStructuredObject.TryGetPropertyValue(propertyName, out object value))
                        {
                            base.Values[propertyName] = value;
                        }
                    }
                }

                this.merged = true;
            }
        }
    }
}
