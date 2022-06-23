using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.OData.Edm;

namespace ODataAlternateKeySample.Models {
    /// <summary>
    /// Provider that selects the IngoreNullEntityPropertiesSerializer that omits null properties on resources from the response
    /// </summary>
    public class IngoreNullEntityPropertiesSerializerProvider : ODataSerializerProvider
    {
        private readonly ODataResourceSerializer _entityTypeSerializer;

        public IngoreNullEntityPropertiesSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer) {
            _entityTypeSerializer = new IngoreNullEntityPropertiesSerializer(this);
        }

        /// <inheritdoc />
        public override IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            System.Diagnostics.Debug.WriteLine("GetEdmTypeSerializer ==== ");

            // Support for Entity types AND Complex types
            if (edmType.Definition.TypeKind == EdmTypeKind.Entity || edmType.Definition.TypeKind == EdmTypeKind.Complex)
            {
                System.Diagnostics.Debug.WriteLine("edmType.Definition.TypeKind == EdmTypeKind.Entity || edmType.Definition.TypeKind == EdmTypeKind.Complex");
                return _entityTypeSerializer;
            }
            else
                return base.GetEdmTypeSerializer(edmType);
        }
    }
}