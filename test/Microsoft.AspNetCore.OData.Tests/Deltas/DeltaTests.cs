//-----------------------------------------------------------------------------
// <copyright file="DeltaTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Deltas
{
    public class DeltaTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_StructuralType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new Delta<Base>(structuralType: null), "structuralType");
        }

        [Fact]
        public void Ctor_ThrowsInvalidOperation_If_EntityType_IsNotAssignable_To_TEntityType()
        {
            ExceptionAssert.Throws<InvalidOperationException>(
                () => new Delta<Derived>(typeof(AnotherDerived)),
                "The actual entity type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+AnotherDerived' is not assignable to the expected type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+Derived'.");
        }

        [Fact]
        public void Can_Declare_A_Delta_Of_An_AbstractClass()
        {
            Delta<AbstractBase> abstractDelta = null;
            Assert.Null(abstractDelta);
        }

        public static IEnumerable<object[]> DeltaModelPropertyNamesData
        {
            get
            {
                MethodInfo getDefaultValue = typeof(DeltaTest).GetMethod("GetDefaultValue");

                var defaultValues = typeof(DeltaModel).GetProperties().Select(p => new[] { p.Name, getDefaultValue.MakeGenericMethod(p.PropertyType).Invoke(obj: null, parameters: null) });
                return defaultValues.Concat(new object[][]
                {
                    new object[] { "StringProperty" , "42" },
                    new object[] { "ComplexModelProperty", new ComplexModel { ComplexIntProperty = 42, ComplexNullableIntProperty = null } },
                    new object[] { "CollectionProperty", new Collection<int> { 1, 2, 3 }}
                });
            }
        }

        [Fact]
        public void TryGetPropertyValue_ThrowsArgumentNull_original()
        {
            // Arrange & Act
            var delta = new Delta<Base>();
            ExceptionAssert.ThrowsArgumentNull(() => delta.TryGetPropertyValue(null, out _), "name");
        }

        [Fact]
        public void TryGetPropertyType_ThrowsArgumentNull_original()
        {
            // Arrange & Act
            var delta = new Delta<Base>();
            ExceptionAssert.ThrowsArgumentNull(() => delta.TryGetPropertyType(null, out _), "name");
        }

        [Theory]
        [MemberData(nameof(DeltaModelPropertyNamesData))]
        public void RoundTrip_Properties(string propertyName, object value)
        {
            Delta<DeltaModel> delta = new Delta<DeltaModel>();

            Type propertyType;
            Assert.True(delta.TryGetPropertyType(propertyName, out propertyType));

            Assert.True(delta.TrySetPropertyValue(propertyName, value));

            object retrievedValue;
            delta.TryGetPropertyValue(propertyName, out retrievedValue);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void RoundTrip_Properties_InDynamicContainer()
        {
            // Arrange
            Type dynamicType = typeof(AddressWithDynamicContainer);
            PropertyInfo dynamicDictionaryPropertyinfo = dynamicType.GetProperty("Dynamics");
            Delta<AddressWithDynamicContainer> delta = new Delta<AddressWithDynamicContainer>(
                dynamicType, null, dynamicDictionaryPropertyinfo);

            // Act & Assert
            Type propertyType;
            string propertyName = "DynamicPropertyName";
            Assert.False(delta.TryGetPropertyType(propertyName, out propertyType));

            // Act & Assert
            object value = 42;
            Assert.True(delta.TrySetPropertyValue(propertyName, value));

            // Act & Assert
            object retrievedValue;
            delta.TryGetPropertyValue(propertyName, out retrievedValue);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TrySetPropertyValue_ThrowsInvalidOperation_IfDynamicContainerWithoutSetter()
        {
            // Arrange
            Type dynamicType = typeof(AddressWithDynamicContainer);
            PropertyInfo dynamicDictionaryPropertyinfo = dynamicType.GetProperty("NonSetDynamics");
            Delta<AddressWithDynamicContainer> delta = new Delta<AddressWithDynamicContainer>(
                dynamicType, null, dynamicDictionaryPropertyinfo);

            // Act
            Action test = () => delta.TrySetPropertyValue("AnyDynamicName", 42);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test,
                "The dynamic dictionary property 'NonSetDynamics' of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+AddressWithDynamicContainer' cannot be set. The dynamic property dictionary must have a setter.");
        }

        [Fact]
        public void CanGetChangedPropertyNames()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "NY", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            var idelta = delta as IDelta;
            // modify in the way we expect the formatter too.
            idelta.TrySetPropertyValue("City", "Sammamish");
            Assert.Single(idelta.GetChangedPropertyNames());
            Assert.Equal("City", idelta.GetChangedPropertyNames().Single());

            // read the property back
            object city = null;
            Assert.True(idelta.TryGetPropertyValue("City", out city));
            Assert.Equal("Sammamish", city);

            // modify the way people will through custom code
            delta.StreetAddress = "23213 NE 15th Ct";
            var mods = idelta.GetChangedPropertyNames().ToArray();
            Assert.Equal(2, mods.Count());
            Assert.Contains("StreetAddress", mods);
            Assert.Contains("City", mods);
            Assert.Equal("23213 NE 15th Ct", delta.StreetAddress);
        }

        [Fact]
        public void CanReadUnmodifiedDefaultValuesFromDelta()
        {
            dynamic patch = new Delta<AddressEntity>();
            Assert.Equal(0, patch.ID);
            Assert.Null(patch.City);
            Assert.Null(patch.State);
            Assert.Null(patch.StreetAddress);
            Assert.Equal(0, patch.ZipCode);
        }

        [Fact]
        public void CanPatch()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "WA", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            delta.City = "Sammamish";
            delta.StreetAddress = "23213 NE 15th Ct";

            delta.Patch(original);
            // unchanged
            Assert.Equal(1, original.ID);
            Assert.Equal(98074, original.ZipCode);
            Assert.Equal("WA", original.State);
            // changed
            Assert.Equal("Sammamish", original.City);
            Assert.Equal("23213 NE 15th Ct", original.StreetAddress);
        }

        [Fact]
        public void CanPatch_OpenType()
        {
            // Arrange
            var address = new SimpleOpenAddress
            {
                City = "City",
                Street = "Street",
                Properties = new Dictionary<string, object>
                {
                    { "IntProp", 9 },
                    { "ListProp", new List<int> {1, 2, 3} }
                }
            };

            PropertyInfo propertyInfo = typeof(SimpleOpenAddress).GetProperty("Properties");
            var delta = new Delta<SimpleOpenAddress>(typeof(SimpleOpenAddress), null, propertyInfo);
            delta.TrySetPropertyValue("City", "ChangedCity");
            delta.TrySetPropertyValue("IntProp", 1);

            // Act
            delta.Patch(address);

            // Assert
            // unchanged
            Assert.Equal("Street", address.Street);
            Assert.Equal(new List<int> { 1, 2, 3 }, address.Properties["ListProp"]);

            // changed
            Assert.Equal("ChangedCity", address.City);
            Assert.Equal(1, address.Properties["IntProp"]);
        }

        [Fact]
        public void CanPut_OpenType()
        {
            // Arrange
            var address = new SimpleOpenAddress
            {
                City = "City",
                Street = "Street",
                Properties = new Dictionary<string, object>
                {
                    { "IntProp", 9 },
                    { "ListProp", new List<int> {1, 2, 3} }
                }
            };

            PropertyInfo propertyInfo = typeof(SimpleOpenAddress).GetProperty("Properties");
            var delta = new Delta<SimpleOpenAddress>(typeof(SimpleOpenAddress), null, propertyInfo);
            delta.TrySetPropertyValue("City", "ChangedCity");
            delta.TrySetPropertyValue("IntProp", 1);

            // Act
            delta.Put(address);

            // Assert
            Assert.Equal("ChangedCity", address.City);
            Assert.Null(address.Street);
            Assert.Equal(1, address.Properties["IntProp"]);
            Assert.False(address.Properties.ContainsKey("ListProp"));
        }

        [Fact]
        public void CopyUnchangedValues_ThrowsArgumentNull_original()
        {
            // Arrange & Act
            var delta = new Delta<Base>();
            ExceptionAssert.ThrowsArgumentNull(() => delta.CopyUnchangedValues(null), "original");
        }

        [Fact]
        public void CopyChangedValues_ThrowsArgumentNull_original()
        {
            // Arrange & Act
            var delta = new Delta<Base>();
            ExceptionAssert.ThrowsArgumentNull(() => delta.CopyChangedValues(null), "original");
        }

        [Fact]
        public void CanCopyUnchangedValues()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "WA", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            delta.City = "Sammamish";
            delta.StreetAddress = "23213 NE 15th Ct";
            var idelta = delta as Delta<AddressEntity>;
            idelta.CopyUnchangedValues(original);
            // unchanged values have been reset to defaults
            Assert.Equal(0, original.ID);
            Assert.Equal(0, original.ZipCode);
            Assert.Null(original.State);
            // changed values have been left unmodified
            Assert.Equal("Redmond", original.City);
            Assert.Equal("21110 NE 44th St", original.StreetAddress);
        }

        [Fact]
        public void CanPut()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "WA", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            delta.City = "Sammamish";
            delta.StreetAddress = "23213 NE 15th Ct";
            var idelta = delta as Delta<AddressEntity>;
            idelta.Put(original);

            // unchanged values have been reset to defaults
            Assert.Equal(0, original.ID);
            Assert.Equal(0, original.ZipCode);
            Assert.Null(original.State);
            // changed values have been updated to values in delta
            Assert.Equal("Sammamish", original.City);
            Assert.Equal("23213 NE 15th Ct", original.StreetAddress);
        }

        [Fact]
        public void CanClear()
        {
            dynamic delta = new Delta<AddressEntity>();
            delta.StreetAddress = "Test";
            var idelta = delta as IDelta;
            Assert.Single(idelta.GetChangedPropertyNames());
            idelta.Clear();
            Assert.Empty(idelta.GetChangedPropertyNames());
        }

        [Fact]
        public void CanCreateDeltaOfDerivedTypes()
        {
            var delta = new Delta<Base>(typeof(Derived));
            Assert.IsType<Derived>(delta.GetInstance());
        }

        [Fact]
        public void CanChangeDerivedClassProperties()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));

            // Act
            delta.DerivedInt = 10;

            // Assert
            Assert.Equal(delta.GetChangedPropertyNames(), new[] { "DerivedInt" });
        }

        [Fact]
        public void Patch_Patches_DerivedTypeProperties()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));
            delta.DerivedInt = 42;
            Derived derived = new Derived();

            // Act
            delta.Patch(derived);

            // Assert
            Assert.Equal(42, derived.DerivedInt);
            Assert.Equal(0, derived.BaseInt);
            Assert.Null(derived.BaseString);
            Assert.Null(derived.DerivedString);
        }

        [Fact]
        public void Put_Clears_DerivedTypeProperties()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));
            delta.DerivedInt = 24;
            Derived derived = new Derived { BaseInt = 42, DerivedInt = 0, BaseString = "42", DerivedString = "42" };

            // Act
            delta.Put(derived);

            // Assert
            Assert.Equal(24, derived.DerivedInt);
            Assert.Equal(0, derived.BaseInt);
            Assert.Null(derived.BaseString);
            Assert.Null(derived.DerivedString);
        }

        [Fact]
        public void Put_DoesNotClear_PropertiesNotOnEntityType()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));
            delta.DerivedInt = 24;
            DerivedDerived derived = new DerivedDerived { BaseInt = 42, DerivedInt = 0, BaseString = "42", DerivedString = "42", DerivedDerivedInt = 42, DerivedDerivedString = "42" };

            // Act
            delta.Put(derived);

            // Assert
            Assert.Equal("42", derived.DerivedDerivedString);
            Assert.Equal(42, derived.DerivedDerivedInt);
        }

        [Fact]
        public void Put_DoesNotClear_NonUpdatableProperties()
        {
            // Arrange
            string expectedString = "hello, world";
            int expectedInt = 24;
            var delta = new Delta<Base>(typeof(Base), new[] { "BaseInt" });
            delta.TrySetPropertyValue("BaseInt", expectedInt);

            Base entity = new Base { BaseInt = 42, BaseString = expectedString };

            // Act
            delta.Put(entity);

            // Assert
            Assert.Equal(expectedInt, entity.BaseInt);
            Assert.Equal(expectedString, entity.BaseString);
        }

        [Fact]
        public void Patch_ClearsAndAddsTo_CollectionPropertiesWithNoSetter()
        {
            // Arrange
            dynamic delta = new Delta<DeltaModelWithCollection>();
            delta.CollectionPropertyWithoutSet = new[] { 1, 2, 3 };
            DeltaModelWithCollection model = new DeltaModelWithCollection { CollectionPropertyWithoutSet = { 42 } };

            // Act
            delta.Patch(model);

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, model.CollectionPropertyWithoutSet);
        }

        [Fact]
        public void Delta_Fails_IfCollectionPropertyDoesNotHaveSetAndHasNullValue()
        {
            // Arrange
            dynamic delta = new Delta<InvalidDeltaModel>();

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => delta.CollectionPropertyWithoutSetAndNullValue = new[] { "1" },
                "The property 'CollectionPropertyWithoutSetAndNullValue' on type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+InvalidD" +
                "eltaModel' returned a null value. The input stream contains collection items which cannot be added if " +
                "the instance is null.");
        }

        [Fact]
        public void Delta_Fails_IfCollectionPropertyDoesNotHaveSetAndClear()
        {
            // Arrange
            dynamic delta = new Delta<InvalidDeltaModel>();

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => delta.CollectionPropertyWithoutSetAndClear = new[] { "1" },
                "The type 'System.Int32[]' of the property 'CollectionPropertyWithoutSetAndClear' on type 'Microsoft." +
                "AspNetCore.OData.Tests.Deltas.DeltaTest+InvalidDeltaModel' does not have a Clear method. Consider using a collection type" +
                " that does have a Clear method, such as IList<T> or ICollection<T>.");
        }

        [Fact]
        public void Patch_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => delta.Patch(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+Derived' on an entity of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+AnotherDerived'.");
        }

        [Fact]
        public void Put_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => delta.Put(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+Derived' on an entity of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+AnotherDerived'.");
        }

        [Fact]
        public void CopyChangedValues_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => delta.CopyChangedValues(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+Derived' on an entity of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+AnotherDerived'.");
        }

        [Fact]
        public void CopyUnchangedValues_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => delta.CopyUnchangedValues(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+Derived' on an entity of type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaTest+AnotherDerived'.");
        }

        public static TheoryDataSet<string, string, object> ODataFormatter_Can_Read_Delta_DataSet
        {
            get
            {
                return new TheoryDataSet<string, string, object>()
                {
                    { "IntProperty", "23", 23 },
                    { "LongProperty", String.Format(CultureInfo.InvariantCulture, "'{0}'", Int64.MaxValue), Int64.MaxValue }, // longs are serialized as strings in odata json
                    { "LongProperty", String.Format(CultureInfo.InvariantCulture, "'{0}'", Int64.MinValue), Int64.MinValue }, // longs are serialized as strings in odata json
                    { "NullableIntProperty", "null", null },
                    { "BoolProperty", "true", true },
                    { "NullableBoolProperty", "null", null },
                    // TODO: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    // { "DateTimeProperty", "'1992-01-01'", new DateTime(1992, 1, 1) },
                    { "DateTimeOffsetProperty", "'1992-01-01'", new DateTimeOffset(new DateTime(1992, 1, 1)) },
                    { "NullableDateTimeOffsetProperty", "'1992-01-01'", new DateTimeOffset(new DateTime(1992, 1, 1)) },
                    { "NullableDateTimeOffsetProperty", "null", null },
                    { "DateProperty", "'1997-07-01'", new Date(1997, 7, 1) },
                    { "NullableDateProperty", "null", null },
                    { "TimeOfDayProperty", "'10:11:12.0130000'", new TimeOfDay(10, 11, 12, 13) },
                    { "NullableTimeOfDayProperty", "null", null },
                    { "StringProperty", "'42'", "42" },
                    { "ComplexModelProperty", "{ 'ComplexIntProperty' : 42 }", new ComplexModel { ComplexIntProperty = 42 } },
                    { "CollectionProperty", "[ 1, 2, 3 ]", new Collection<int> { 1, 2, 3} },
                    { "ComplexModelCollectionProperty", "[ { 'ComplexIntProperty' : 42 } ]", new Collection<ComplexModel> { new ComplexModel { ComplexIntProperty = 42 } } }
                };
            }
        }

        public static TheoryDataSet<Type> TypedDelta_Returns_Correct_ExpectedClrType_And_ActualType_DataSet
        {
            get
            {
                return new TheoryDataSet<Type>()
                {
                    { typeof(SimpleOpenCustomer) },
                    { typeof(SimpleVipCustomer) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(TypedDelta_Returns_Correct_ExpectedClrType_And_ActualType_DataSet))]
        public void TypedDelta_Returns_Correct_ExpectedClrType_And_ActualType(Type actualType)
        {
            // Arrange
            ITypedDelta delta = new Delta<SimpleOpenCustomer>(actualType);

            // Act
            Type actualActualType = delta.StructuredType;
            Type actualExpectedType = delta.ExpectedClrType;

            // Assert
            Assert.Equal(typeof(SimpleOpenCustomer), actualExpectedType);
            Assert.Equal(actualType, actualActualType);
        }

        public static T GetDefaultValue<T>()
        {
            return default(T);
        }

        private class DeltaModel
        {
            public int IntProperty { get; set; }

            public int? NullableIntProperty { get; set; }

            public long LongProperty { get; set; }

            public long? NullableLongProperty { get; set; }

            public bool BoolProperty { get; set; }

            public bool? NullableBoolProperty { get; set; }

            public Guid GuidProperty { get; set; }

            public Guid? NullableGuidProperty { get; set; }

            public DateTimeOffset DateTimeOffsetProperty { get; set; }

            public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

            public Date DateProperty { get; set; }

            public Date? NullableDateProperty { get; set; }

            public TimeOfDay TimeOfDayProperty { get; set; }

            public TimeOfDay? NullableTimeOfDayProperty { get; set; }

            public string StringProperty { get; set; }

            public ComplexModel ComplexModelProperty { get; set; }

            public Collection<int> CollectionProperty { get; set; }

            public Collection<ComplexModel> ComplexModelCollectionProperty { get; set; }
        }

        private class DeltaModelWithCollection
        {
            public DeltaModelWithCollection()
            {
                CollectionPropertyWithoutSet = new Collection<int>();
                ComplexModelCollectionPropertyWithOutSet = new Collection<ComplexModel>();
            }

            public Collection<int> CollectionPropertyWithoutSet { get; private set; }

            public Collection<ComplexModel> ComplexModelCollectionPropertyWithOutSet { get; private set; }
        }

        private class InvalidDeltaModel
        {
            public InvalidDeltaModel()
            {
                CollectionPropertyWithoutSetAndClear = new int[0];
            }

            public IEnumerable<int> CollectionPropertyWithoutSetAndNullValue { get; private set; }

            public IEnumerable<int> CollectionPropertyWithoutSetAndClear { get; private set; }
        }

        private class ComplexModel
        {
            public int ComplexIntProperty { get; set; }

            public int? ComplexNullableIntProperty { get; set; }

            public override bool Equals(object obj)
            {
                ComplexModel model = obj as ComplexModel;

                if (model == null)
                {
                    return false;
                }

                return ComplexIntProperty == model.ComplexIntProperty && ComplexNullableIntProperty == model.ComplexNullableIntProperty;
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }

        private abstract class AbstractBase
        {
        }

        private class Base
        {
            public int BaseInt { get; set; }

            public string BaseString { get; set; }
        }

        private class Derived : Base
        {
            public int DerivedInt { get; set; }

            public string DerivedString { get; set; }
        }

        private class DerivedDerived : Derived
        {
            public int DerivedDerivedInt { get; set; }

            public string DerivedDerivedString { get; set; }
        }

        private class AnotherDerived : Base
        {
        }

        [DataContract(Namespace = "com.contoso", Name = "DeltaModelAlias")]
        private class DeltaModelWithAlias
        {
            [DataMember(Name = "StringPropertyAlias")]
            public string StringProperty { get; set; }

            [DataMember(Name = "ComplexModelPropertyAlias")]
            public ComplexModelWithAlias ComplexModelProperty { get; set; }

            [DataMember(Name = "CollectionPropertyAlias")]
            public Collection<int> CollectionProperty { get; set; }

            [DataMember(Name = "ComplexModelCollectionPropertyAlias")]
            public Collection<ComplexModelWithAlias> ComplexModelCollectionProperty { get; set; }
        }

        [DataContract(Namespace = "com.contoso", Name = "ComplexModelAlias")]
        private class ComplexModelWithAlias
        {
            [DataMember(Name = "ComplexIntPropertyAlias")]
            public int ComplexIntProperty { get; set; }

            [DataMember(Name = "ComplexNullableIntPropertyAlias")]
            public int? ComplexNullableIntProperty { get; set; }

            public override bool Equals(object obj)
            {
                ComplexModelWithAlias model = obj as ComplexModelWithAlias;

                if (model == null)
                {
                    return false;
                }

                return ComplexIntProperty == model.ComplexIntProperty && ComplexNullableIntProperty == model.ComplexNullableIntProperty;
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }

        public class AddressEntity
        {
            public int ID { get; set; }

            public string StreetAddress { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public int ZipCode { get; set; }
        }

        public class AddressWithDynamicContainer
        {
            public int ID { get; set; }

            public string City { get; set; }

            public IDictionary<string, object> Dynamics { get; set; }

            public IDictionary<string, object> NonSetDynamics { get; }
        }
    }
}
