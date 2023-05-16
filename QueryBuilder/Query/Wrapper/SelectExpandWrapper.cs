using System;
using System.Collections.Generic;
using QueryBuilder.Edm;
using QueryBuilder.Formatter.Value;
using QueryBuilder.Query.Container;
using Microsoft.OData.Edm;

namespace QueryBuilder.Query.Wrapper
{
    internal abstract class SelectExpandWrapper : IEdmEntityObject, ISelectExpandWrapper
    {
        private static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();
        private static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider =
            (IEdmModel m, IEdmStructuredType t) => DefaultPropertyMapper;

        private Dictionary<string, object> _containerDict;
        private TypedEdmStructuredObject _typedEdmStructuredObject;

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public PropertyContainer Container { get; set; }

        /// <summary>
        /// The Edm Model associated with the wrapper.
        /// EntityFramework does not let us inject non primitive constant values (like IEdmModel).
        /// However, we can always 'parameterize" this non-constant value.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <inheritdoc />
        public object UntypedInstance { get; set; }

        /// <summary>
        /// Gets or sets the instance type name
        /// </summary>
        public string InstanceType { get; set; }

        /// <summary>
        /// Indicates whether the underlying instance can be used to obtain property values.
        /// </summary>
        public bool UseInstanceForProperties { get; set; }

        /// <inheritdoc />
        public IEdmTypeReference GetEdmType()
        {
            IEdmModel model = Model;

            if (InstanceType != null)
            {
                IEdmStructuredType structuredType = model.FindType(InstanceType) as IEdmStructuredType;
                IEdmEntityType entityType = structuredType as IEdmEntityType;

                if (entityType != null)
                {
                    return entityType.ToEdmTypeReference(true);
                }

                return structuredType.ToEdmTypeReference(true);
            }

            Type elementType = GetElementType();

            return model.GetEdmTypeReference(elementType);
        }

        /// <inheritdoc />
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            // look into the container first to see if it has that property. container would have it 
            // if the property was expanded.
            if (Container != null)
            {
                _containerDict = _containerDict ?? Container.ToDictionary(DefaultPropertyMapper, includeAutoSelected: true);
                if (_containerDict.TryGetValue(propertyName, out value))
                {
                    return true;
                }
            }

            // fall back to the instance.
            if (UseInstanceForProperties && UntypedInstance != null)
            {
                IEdmTypeReference edmTypeReference = GetEdmType();
                IEdmModel model = Model;
                if (edmTypeReference is IEdmComplexTypeReference)
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                        new TypedEdmComplexObject(UntypedInstance, edmTypeReference as IEdmComplexTypeReference, model);
                }
                else
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                        new TypedEdmEntityObject(UntypedInstance, edmTypeReference as IEdmEntityTypeReference, model);
                }

                return _typedEdmStructuredObject.TryGetPropertyValue(propertyName, out value);
            }

            value = null;
            return false;
        }

        public IDictionary<string, object> ToDictionary()
        {
            return ToDictionary(_mapperProvider);
        }

        public IDictionary<string, object> ToDictionary(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
        {
            if (mapperProvider == null)
            {
                throw Error.ArgumentNull("mapperProvider");
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            IEdmStructuredType type = GetEdmType().AsStructured().StructuredDefinition();

            IPropertyMapper mapper = mapperProvider(Model, type);
            if (mapper == null)
            {
                throw Error.InvalidOperation(SRResources.InvalidPropertyMapper, typeof(IPropertyMapper).FullName,
                    type.FullTypeName());
            }

            if (Container != null)
            {
                dictionary = Container.ToDictionary(mapper, includeAutoSelected: false);
            }

            // The user asked for all the structural properties on this instance.
            if (UseInstanceForProperties && UntypedInstance != null)
            {
                foreach (IEdmStructuralProperty property in type.StructuralProperties())
                {
                    object propertyValue;
                    if (TryGetPropertyValue(property.Name, out propertyValue))
                    {
                        string mappingName = mapper.MapProperty(property.Name);
                        if (mappingName != null)
                        {
                            if (String.IsNullOrWhiteSpace(mappingName))
                            {
                                throw Error.InvalidOperation(SRResources.InvalidPropertyMapping, property.Name);
                            }

                            dictionary[mappingName] = propertyValue;
                        }
                    }
                }
            }

            return dictionary;
        }

        protected abstract Type GetElementType();
    }
}
