// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatting.Value;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatting.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing the raw value of an <see cref="IEdmPrimitiveType"/>.
    /// </summary>
    public class ODataSerializerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        public ODataSerializerContext()
        {
        }

        /// <summary>
        /// Gets or sets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the metadata level of the response.
        /// </summary>
        public ODataMetadataLevel MetadataLevel { get; set; }

        /// <summary>
        /// Gets a property bag associated with this context to store any generic data.
        /// </summary>
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        public HttpRequest Request { get;set; }

        /// <summary>
        /// Gets or sets the root element name which is used when writing primitive and enum types
        /// </summary>
        public string RootElementName { get; set; }

        internal IEdmTypeReference GetEdmType(object instance, Type type)
        {
            IEdmTypeReference edmType = null;

            //IEdmObject edmObject = instance as IEdmObject;
            //if (edmObject != null)
            //{
            //    edmType = edmObject.GetEdmType();
            //    if (edmType == null)
            //    {
            //        throw Error.InvalidOperation(SRResources.EdmTypeCannotBeNull, edmObject.GetType().FullName,
            //            typeof(IEdmObject).Name);
            //    }
            //}
            //else
            //{
            //    if (Model == null)
            //    {
            //        throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            //    }

            //    _typeMappingCache = _typeMappingCache ?? Model.GetTypeMappingCache();
            //    edmType = _typeMappingCache.GetEdmType(type, Model);

            //    if (edmType == null)
            //    {
            //        if (instance != null)
            //        {
            //            edmType = _typeMappingCache.GetEdmType(instance.GetType(), Model);
            //        }

            //        if (edmType == null)
            //        {
            //            throw Error.InvalidOperation(SRResources.ClrTypeNotInModel, type);
            //        }
            //    }
            //    else if (instance != null)
            //    {
            //        IEdmTypeReference actualType = _typeMappingCache.GetEdmType(instance.GetType(), Model);
            //        if (actualType != null && actualType != edmType)
            //        {
            //            edmType = actualType;
            //        }
            //    }
            //}

            return edmType;
        }
    }
}
