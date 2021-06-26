// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Edm;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ConventionsHelpersTests
    {
        private ODataSerializerContext _writeContext = new ODataSerializerContext { Model = EdmCoreModel.Instance };

        public static TheoryDataSet<object, string> GetEntityKeyValue_SingleKey_DifferentDataTypes_Data
        {
            get
            {
                return new TheoryDataSet<object, string>
                {
                    { 1, "1" },
                    { "1", "'1'" },
                    { new DateTimeOffset(new DateTime(2012,12,31,0,0,0,DateTimeKind.Utc)), "2012-12-31T00:00:00Z" },
                    { new byte[] { 1,2 }, "binary'AQI='" },
                    { false, "false" },
                    { true, "true" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "dddddddd-dddd-dddd-dddd-dddddddddddd" },
                    { new Date(2014, 10, 14), "2014-10-14"},
                    { new TimeOfDay(15, 38, 25, 109), "15:38:25.1090000"},
                };
            }
        }

        [Fact]
        public void GetEntityKeyValue_SingleKey()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.String));
            var entityInstance = new { Property = "key" };

            ResourceContext entityContext = new ResourceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal("'key'", keyValue);
        }

        [Theory]
        [MemberData(nameof(GetEntityKeyValue_SingleKey_DifferentDataTypes_Data))]
        public void GetEntityKeyValue_SingleKey_DifferentDataTypes(object value, object expectedValue)
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.String));
            var entityInstance = new { Property = value };

            ResourceContext entityContext = new ResourceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal(expectedValue, keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys()
        {
            // Arrange
            var entityInstance = new { Key1 = "key1", Key2 = 2, Key3 = true };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.String));
            entityType.AddKeys(entityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            entityType.AddKeys(entityType.AddStructuralProperty("Key3", EdmPrimitiveTypeKind.Boolean));

            ResourceContext entityContext = new ResourceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal("Key1='key1',Key2=2,Key3=true", keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys_ForDateAndTimeOfDay()
        {
            // Arrange
            var entityInstance = new { Key1 = new Date(2015, 12, 2), Key2 = new TimeOfDay(4, 3, 2, 1) };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.Date));
            entityType.AddKeys(entityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.TimeOfDay));

            ResourceContext entityContext = new ResourceContext(_writeContext,
                entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal("Key1=2015-12-02,Key2=04:03:02.0010000", keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_ThrowsForNullKeys()
        {
            // Arrange
            var entityInstance = new { Key = (string)null };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key", EdmPrimitiveTypeKind.String));

            ResourceContext entityContext = new ResourceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ConventionsHelpers.GetEntityKeyValue(entityContext),
                "Key property 'Key' of type 'NS.Name' is null. Key properties cannot have null values.");
        }

        [Fact]
        public void GetEntityKeyValue_ThrowsForNullKeys_WithMultipleKeys()
        {
            // Arrange
            var entityInstance = new { Key1 = "abc", Key2 = "def", Key3 = (string)null };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.String));
            entityType.AddKeys(entityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            entityType.AddKeys(entityType.AddStructuralProperty("Key3", EdmPrimitiveTypeKind.Boolean));

            ResourceContext entityContext = new ResourceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ConventionsHelpers.GetEntityKeyValue(entityContext),
                "Key property 'Key3' of type 'NS.Name' is null. Key properties cannot have null values.");
        }

        [Fact]
        public void GetEntityKeyValue_DerivedType()
        {
            // Arrange
            var entityInstance = new { Key = "key" };
            EdmEntityType baseEntityType = new EdmEntityType("NS", "Name");
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key", EdmPrimitiveTypeKind.String));
            EdmEntityType derivedEntityType = new EdmEntityType("NS", "Derived", baseEntityType);

            ResourceContext entityContext = new ResourceContext(_writeContext, derivedEntityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal("'key'", keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys_DerivedType()
        {
            // Arrange
            var entityInstance = new { Key1 = "key1", Key2 = 2, Key3 = true };
            EdmEntityType baseEntityType = new EdmEntityType("NS", "Name");
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.String));
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key3", EdmPrimitiveTypeKind.Boolean));
            EdmEntityType derivedEntityType = new EdmEntityType("NS", "Derived", baseEntityType);

            ResourceContext entityContext = new ResourceContext(_writeContext, derivedEntityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal("Key1='key1',Key2=2,Key3=true", keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys_DerivedType_ForDateAndTimeOfDay()
        {
            // Arrange
            var entityInstance = new { Key1 = new Date(2015, 2, 26), Key2 = new TimeOfDay(1, 2, 3, 4) };
            EdmEntityType baseEntityType = new EdmEntityType("NS", "Name");
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.Date));
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.TimeOfDay));
            EdmEntityType derivedEntityType = new EdmEntityType("NS", "Derived", baseEntityType);

            ResourceContext entityContext = new ResourceContext(_writeContext,
                derivedEntityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityContext);

            // Assert
            Assert.Equal("Key1=2015-02-26,Key2=01:02:03.0040000", keyValue);
        }

        public static TheoryDataSet<object, string> GetUriRepresentationForValue_DataSet
        {
            get
            {
                return new TheoryDataSet<object, string>()
                {
                    { (bool)true, "true" },
                    { (char)'1', "'1'" },
                    { (char?)'1', "'1'" },
                    { (string)"123", "'123'" },
                    { (char[])new char[] { '1', '2', '3' }, "'123'" },
                    { (int)123, "123" },
                    { (short)123, "123" },
                    { (long)123, "123" },
                    { (ushort)123, "123" },
                    { (uint)123, "123" },
                    { (ulong)123, "123" },
                    { (int?)123, "123" },
                    { (short?)123, "123" },
                    { (long?)123, "123" },
                    { (ushort?)123, "123" },
                    { (uint?)123, "123" },
                    { (ulong?)123, "123" },
                    { (float)123.123, "123.123" },
                    { (double)123.123, "123.123" },
                    { (decimal)123.123, "123.123" },
                    { Guid.Empty, "00000000-0000-0000-0000-000000000000" },
                    { new DateTimeOffset(1,1,1,0,0,0,TimeSpan.Zero), "0001-01-01T00:00:00Z" },
                    { TimeSpan.FromSeconds(86456), "duration'P1DT56S'" },
                    { DateTimeOffset.FromFileTime(0).ToUniversalTime(), "1601-01-01T00:00:00Z" },
                    { SimpleEnum.First, "Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'First'" },
                    { FlagsEnum.One | FlagsEnum.Two, "Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum'One, Two'" },
                    { (SimpleEnum)123, "Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'123'" },
                    { (FlagsEnum)123, "Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum'123'" },
                    { new Date(2014, 10, 14), "2014-10-14"},
                    { new TimeOfDay(15, 38, 25, 109), "15:38:25.1090000"},
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetUriRepresentationForValue_DataSet))]
        public void GetUriRepresentationForValue_Works(object value, string result)
        {
            Assert.Equal(
                result,
                ConventionsHelpers.GetUriRepresentationForValue(value));
        }

        private class GetKeyProperty_validEntityType_TestClass_Id
        {
            public int Id { get; set; }
        }

        private class GetKeyProperty_validEntityType_TestClass2_ClassName
        {
            public string GetKeyProperty_validEntityType_TestClass2_ClassNameId { get; set; }
        }

        private class GetKeyProperty_InValidEntityType_NoId
        {
            public int IDD { get; set; }
        }

        private class GetKeyProperty_InValidEntityType_ComplexId
        {
            public GetProperties_Complex Id { get; set; }
        }

        class GetProperties_Base
        {
            public int Base_I { get; set; }

            private int Pri { get; set; }

            public int InternalGet { internal get; set; }

            public GetProperties_Complex Base_Complex { get; set; }

            public virtual string Base_Str { get; set; }
        }

        class GetProperties_Derived : GetProperties_Base
        {
            public static int SomeStaticProperty1 { get; set; }

            internal static int SomeStaticProperty2 { get; set; }

            private static int SomeStaticProperty3 { get; set; }

            public string Derived_I { get; set; }

            public string PrivateSetPublicGet { get; private set; }

            public string PrivateGetPublicSet { private get; set; }

            public GetProperties_Complex Derived_Complex { get; set; }

            public int[] Collection { get; private set; }

            public string this[string str]
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        class GetProperties_Complex
        {
            public int A { get; set; }
        }

        public class GetProperties_NestParent
        {
            public class Nest
            {
                public NestPropertyType NestProperty { get; set; }
            }

            public class NestPropertyType
            {
            }
        }
    }
}
