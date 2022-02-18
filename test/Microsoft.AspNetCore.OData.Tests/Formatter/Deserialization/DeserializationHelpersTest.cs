//-----------------------------------------------------------------------------
// <copyright file="DeserializationHelpersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class DeserializationHelpersTest
    {
        [Theory]
        [InlineData("Property", true, typeof(int))]
        [InlineData("Property", false, typeof(int))]
        [InlineData("PropertyNotPresent", true, null)]
        [InlineData("PropertyNotPresent", false, null)]
        public void GetPropertyType_NonDelta(string propertyName, bool isDelta, Type expectedPropertyType)
        {
            object resource = isDelta ? (object)new Delta<GetPropertyType_TestClass>() : new GetPropertyType_TestClass();
            Assert.Equal(
                expectedPropertyType,
                DeserializationHelpers.GetPropertyType(resource, propertyName));
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_CollectionTypeCanBeInstantiated_And_SettableProperty(string propertyName)
        {
            object value = new SampleClassWithSettableCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name);

            Assert.Equal(
                new[] { 1, 2, 3 },
                value.GetType().GetProperty(propertyName).GetValue(value, index: null) as IEnumerable<int>);
        }

        [Theory]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("ICustomCollectionInterface")]
        public void SetCollectionProperty_CollectionTypeCannotBeInstantiated_And_SettableProperty_Throws(string propertyName)
        {
            object value = new SampleClassWithSettableCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                string.Format("The property '{0}' on type 'Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithSettableCollectionProperties' returned a null value. " +
                "The input stream contains collection items which cannot be added if the instance is null.", propertyName));
        }

        [Theory]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_NonSettableProperty_NonNullValue_WithAddMethod(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name);

            Assert.Equal(
                new[] { 1, 2, 3 },
                value.GetType().GetProperty(propertyName).GetValue(value, index: null) as IEnumerable<int>);
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        public void SetCollectionProperty_NonSettableProperty_ArrayValue_FixedSize_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            Type propertyType = typeof(SampleClassWithNonSettableCollectionProperties).GetProperty(propertyName).PropertyType;
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                string.Format("The value of the property '{0}' on type 'Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithNonSettableCollectionProperties' is an array. " +
                "Consider adding a setter for the property.", propertyName));
        }

        [Theory]
        [InlineData("CustomCollectionWithoutAdd")]
        public void SetCollectionProperty_NonSettableProperty_NonNullValue_NoAdd_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            Type propertyType = typeof(SampleClassWithNonSettableCollectionProperties).GetProperty(propertyName).PropertyType;
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                string.Format("The type '{0}' of the property '{1}' on type 'Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithNonSettableCollectionProperties' does not have an Add method. " +
                "Consider using a collection type that does have an Add method - for example IList<T> or ICollection<T>.", propertyType.FullName, propertyName));
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_NonSettableProperty_NullValue_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            value.GetType().GetProperty(propertyName).SetValue(value, null, null);
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                 string.Format("The property '{0}' on type 'Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithNonSettableCollectionProperties' returned a null value. " +
                 "The input stream contains collection items which cannot be added if the instance is null.", propertyName));
        }

        [Fact]
        public void SetCollectionProperty_CanConvertNonStandardEdmTypes()
        {
            SampleClassWithDifferentCollectionProperties value = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("UnsignedArray", EdmPrimitiveTypeKind.Int32);

            DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name);

            Assert.Equal(
                new uint[] { 1, 2, 3 },
               value.UnsignedArray);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertDataTime_ByDefault()
        {
            // Arrange
            SampleClassWithDifferentCollectionProperties source = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("DateTimeList", EdmPrimitiveTypeKind.DateTimeOffset);
            DateTime dt = new DateTime(2014, 11, 15, 1, 2, 3);
            IList<DateTimeOffset> dtos = new List<DateTimeOffset>
            {
                new DateTimeOffset(dt, TimeSpan.Zero),
                new DateTimeOffset(dt, new TimeSpan(+7, 0, 0)),
                new DateTimeOffset(dt, new TimeSpan(-8, 0, 0))
            };

            IEnumerable<DateTime> expects = dtos.Select(e => e.LocalDateTime);

            // Act
            DeserializationHelpers.SetCollectionProperty(source, edmProperty, dtos, edmProperty.Name, context: null);

            // Assert
            Assert.Equal(expects, source.DateTimeList);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertDataTime_ByTimeZoneInfo()
        {
            // Arrange
            SampleClassWithDifferentCollectionProperties source = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("DateTimeList", EdmPrimitiveTypeKind.DateTimeOffset);

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime dt = new DateTime(2014, 11, 15, 1, 2, 3);
            IList<DateTimeOffset> dtos = new List<DateTimeOffset>
            {
                new DateTimeOffset(dt, TimeSpan.Zero),
                new DateTimeOffset(dt, new TimeSpan(+7, 0, 0)),
                new DateTimeOffset(dt, new TimeSpan(-8, 0, 0))
            };
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                TimeZone = tzi
            };

            // Act
            DeserializationHelpers.SetCollectionProperty(source, edmProperty, dtos, edmProperty.Name, context: context);

            // Assert
            Assert.Equal(new List<DateTime> { dt.AddHours(-8), dt.AddHours(-15), dt }, source.DateTimeList);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertEnumCollection()
        {
            SampleClassWithDifferentCollectionProperties value = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("FlagsEnum", EdmPrimitiveTypeKind.String);

            DeserializationHelpers.SetCollectionProperty(
                value,
                edmProperty,
                value: new List<FlagsEnum> { FlagsEnum.One, FlagsEnum.Four | FlagsEnum.Two | (FlagsEnum)123 },
                propertyName: edmProperty.Name);

            Assert.Equal(
                new FlagsEnum[] { FlagsEnum.One, FlagsEnum.Four | FlagsEnum.Two | (FlagsEnum)123 },
               value.FlagsEnum);
        }

        [Theory]
        [InlineData("NonCollectionString")]
        [InlineData("NonCollectionInt")]
        public void SetCollectionProperty_OnNonCollection_ThrowsSerialization(string propertyName)
        {
            object value = new SampleClassWithDifferentCollectionProperties();
            Type propertyType = typeof(SampleClassWithDifferentCollectionProperties).GetProperty(propertyName).PropertyType;
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
            Error.Format(
            "The type '{0}' of the property '{1}' on type 'Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithDifferentCollectionProperties' must be a collection.",
            propertyType.FullName,
            propertyName));
        }

        [Theory]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_ClearsCollection_IfClearCollectionIsTrue(string propertyName)
        {
            // Arrange
            IEnumerable<int> value = new int[] { 1, 2, 3 };
            object resource = new SampleClassWithNonSettableCollectionProperties
                {
                    ICollection = { 42 },
                    IList = { 42 },
                    Collection = { 42 },
                    List = { 42 },
                    CustomCollectionWithNoEmptyCtor = { 42 },
                    CustomCollection = { 42 }
                };

            // Act
            DeserializationHelpers.SetCollectionProperty(resource, propertyName, null, value, clearCollection: true);

            // Assert
            Assert.Equal(
                value,
                resource.GetType().GetProperty(propertyName).GetValue(resource, index: null) as IEnumerable<int>);
        }

        //[Fact]
        //public void ApplyProperty_DoesNotIgnoreKeyProperty()
        //{
        //    // Arrange
        //    ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
        //    EdmEntityType entityType = new EdmEntityType("namespace", "name");
        //    entityType.AddKeys(entityType.AddStructuralProperty("Key1", typeof(string).GetEdmPrimitiveTypeReference());

        //    EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
        //    ODataDeserializerProvider provider = ODataDeserializerProviderFactory.Create();

        //    var resource = new Mock<IDelta>(MockBehavior.Strict);
        //    Type propertyType = typeof(string);
        //    resource.Setup(r => r.TryGetPropertyType("Key1", out propertyType)).Returns(true).Verifiable();
        //    resource.Setup(r => r.TrySetPropertyValue("Key1", "Value1")).Returns(true).Verifiable();

        //    // Act
        //    DeserializationHelpers.ApplyProperty(property, entityTypeReference, resource.Object, provider,
        //        new ODataDeserializerContext{ Model = new EdmModel() });

        //    // Assert
        //    resource.Verify();
        //}

        [Theory]
        [InlineData("null", null)]
        [InlineData("42", 42)]
        [InlineData("\"abc\"", "abc")]
        public void ConvertValue_Works_WithODataUntypedValue(string rawValue, object expected)
        {
            // Arrange
            object oDataValue = new ODataUntypedValue
            {
                RawValue = rawValue
            };

            // Act
            IEdmTypeReference typeRef = null;
            object value = DeserializationHelpers.ConvertValue(oDataValue, ref typeRef, null, null, out EdmTypeKind typeKind);

            // Assert
            Assert.Equal(EdmTypeKind.Primitive, typeKind);
            Assert.Equal(expected, value);
        }

        [Fact]
        public void ConvertValue_Works_WithODataUntypedValue_Decimal()
        {
            // Arrange
            object oDataValue = new ODataUntypedValue
            {
                RawValue = "42.6"
            };

            // Act
            IEdmTypeReference typeRef = null;
            object value = DeserializationHelpers.ConvertValue(oDataValue, ref typeRef, null, null, out EdmTypeKind typeKind);

            // Assert
            Assert.Equal(EdmTypeKind.Primitive, typeKind);
            Assert.Equal((decimal)42.6, value);
        }

        [Fact]
        public void ConvertValue_Works_WithODataUntypedValue_Double()
        {
            // Arrange
            object oDataValue = new ODataUntypedValue
            {
                RawValue = "-1.643e6"
            };

            // Act
            IEdmTypeReference typeRef = null;
            object value = DeserializationHelpers.ConvertValue(oDataValue, ref typeRef, null, null, out EdmTypeKind typeKind);

            // Assert
            Assert.Equal(EdmTypeKind.Primitive, typeKind);
            Assert.Equal((double)-1643000, value);
        }

        [Theory]
        [InlineData("[abc1.643e6]")]
        [InlineData("{abc1.643e6}")]
        [InlineData("abc1.643e6")]
        public void ConvertValue_ThrowsODataException_WithInvalidValue(string rawValue)
        {
            // Arrange
            object oDataValue = new ODataUntypedValue
            {
                RawValue = rawValue
            };

            // Act
            IEdmTypeReference typeRef = null;
            Action test = () => DeserializationHelpers.ConvertValue(oDataValue, ref typeRef, null, null, out EdmTypeKind typeKind);

            // Assert
            ExceptionAssert.Throws<ODataException>(test,
                $"The given untyped value '{rawValue}' in payload is invalid. Consider using a OData type annotation explicitly.");
        }

        [Fact]
        public void ApplyProperty_FailsWithUsefulErrorMessageOnUnknownProperty()
        {
            // Arrange
            const string HelpfulErrorMessage =
                "The property 'Unknown' does not exist on type 'namespace.name'. Make sure to only use property names " +
                "that are defined by the type.";

            var property = new ODataProperty { Name = "Unknown", Value = "Value" };
            var entityType = new EdmComplexType("namespace", "name");
            entityType.AddStructuralProperty("Known", EdmCoreModel.Instance.GetString(true));

            var entityTypeReference = new EdmComplexTypeReference(entityType, isNullable: false);

            // Act
            var exception = Assert.Throws<ODataException>(() =>
                DeserializationHelpers.ApplyProperty(
                    property,
                    entityTypeReference,
                    resource: null,
                    deserializerProvider: null,
                    readContext: null));

            // Assert
            Assert.Equal(HelpfulErrorMessage, exception.Message);
        }

        [Fact]
        public void GetCollectionElementTypeName_ThrowsODataException_NestedCollection()
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => DeserializationHelpers.GetCollectionElementTypeName("Collection(Edm.Int32)", true),
                "The type 'Collection(Edm.Int32)' is a nested collection type. Nested collection types are not allowed.");
        }

        private static IEdmProperty GetMockEdmProperty(string name, EdmPrimitiveTypeKind elementType)
        {
            Mock<IEdmProperty> property = new Mock<IEdmProperty>();
            property.Setup(p => p.Name).Returns(name);
            IEdmTypeReference elementTypeReference =
                EdmCoreModel.Instance.GetPrimitiveType(elementType).ToEdmTypeReference(isNullable: false);
            property.Setup(p => p.Type)
                    .Returns(new EdmCollectionTypeReference(new EdmCollectionType(elementTypeReference)));
            return property.Object;
        }

        private class GetPropertyType_TestClass
        {
            public int Property { get; set; }
        }

        private class SampleClassWithSettableCollectionProperties
        {
            public int[] Array { get; set; }

            public IEnumerable<int> IEnumerable { get; set; }

            public ICollection<int> ICollection { get; set; }

            public IList<int> IList { get; set; }

            public Collection<int> Collection { get; set; }

            public List<int> List { get; set; }

            public CustomCollection CustomCollection { get; set; }

            public CustomCollectionWithNoEmptyCtor CustomCollectionWithNoEmptyCtor { get; set; }

            public ICustomCollectionInterface<int> ICustomCollectionInterface { get; set; }
        }

        private class SampleClassWithNonSettableCollectionProperties
        {
            public SampleClassWithNonSettableCollectionProperties()
            {
                Array = new int[0];
                IEnumerable = new int[0];
                ICollection = new Collection<int>();
                IList = new List<int>();
                Collection = new Collection<int>();
                List = new List<int>();
                CustomCollection = new CustomCollection();
                CustomCollectionWithNoEmptyCtor = new CustomCollectionWithNoEmptyCtor(100);
                CustomCollectionWithoutAdd = new CustomCollectionWithoutAdd<int>();
            }

            public int[] Array { get; internal set; }

            public IEnumerable<int> IEnumerable { get; internal set; }

            public ICollection<int> ICollection { get; internal set; }

            public IList<int> IList { get; internal set; }

            public Collection<int> Collection { get; internal set; }

            public List<int> List { get; internal set; }

            public CustomCollection CustomCollection { get; internal set; }

            public CustomCollectionWithNoEmptyCtor CustomCollectionWithNoEmptyCtor { get; internal set; }

            public CustomCollectionWithoutAdd<int> CustomCollectionWithoutAdd { get; internal set; }
        }

        private class SampleClassWithDifferentCollectionProperties
        {
            public string NonCollectionString { get; set; }

            public int NonCollectionInt { get; set; }

            public uint[] UnsignedArray { get; set; }

            public FlagsEnum[] FlagsEnum { get; set; }

            public IList<DateTime> DateTimeList { get; set; }
        }

        private class CustomCollection : List<int> { }

        private class CustomCollectionWithNoEmptyCtor : List<int>
        {
            public CustomCollectionWithNoEmptyCtor(int i)
            {
            }
        }

        private interface ICustomCollectionInterface<T> : IEnumerable<T>
        {
        }

        private class CustomCollectionWithoutAdd<T> : IEnumerable<T>
        {
            private List<T> _list = new List<T>();

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
