using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using NavigationSourceLinkBuilderAnnotation = Microsoft.AspNetCore.OData.Edm.NavigationSourceLinkBuilderAnnotation;
using Microsoft.AspNetCore.OData.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;

namespace ODataAlternateKeySample.Models {
    /// <summary>
    /// OData Entity Serilizer that omits null properties from the response
    /// </summary>
    public class IngoreNullEntityPropertiesSerializer : ODataResourceSerializer
    {
        public IngoreNullEntityPropertiesSerializer(ODataSerializerProvider provider)
            : base(provider) { }

        /// <summary>
        /// Only return properties that are not null
        /// </summary>
        /// <param name="structuralProperty">The EDM structural property being written.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The property be written by the serilizer, a null response will effectively skip this property.</returns>
        public override ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
        {
            var property = base.CreateStructuralProperty(structuralProperty, resourceContext);
            return property.Value != null ? property : null;
        }
    }
}
