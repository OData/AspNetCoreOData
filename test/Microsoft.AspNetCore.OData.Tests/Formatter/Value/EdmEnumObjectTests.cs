//-----------------------------------------------------------------------------
// <copyright file="EdmEnumObjectTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmEnumObjectTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmEnumType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new TestEdmEnumObject((IEdmEnumType)null, "test"), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmEnumTypeReference()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new TestEdmEnumObject((IEdmEnumTypeReference)null, "test"), "type");
        }

        [Fact]
        public void Property_IsNullable()
        {
            TestEdmEnumObject edmObject = new TestEdmEnumObject(new EdmEnumType("NS", "Enum"), "test");

            ReflectionAssert.BooleanProperty(edmObject, e => e.IsNullable, expectedDefaultValue: false);
        }

        [Fact]
        public void GetEdmType_HasSameDefinition_AsInitializedEdmType()
        {
            var enumType = new EdmEnumType("NS", "Enum");
            var edmObject = new TestEdmEnumObject(enumType, "test");

            Assert.Equal(enumType, edmObject.GetEdmType().Definition);
        }

        [Fact]
        public void GetEdmType_AgreesWithPropertyIsNullable()
        {
            var enumType = new EdmEnumType("NS", "Enum");
            var edmObject = new TestEdmEnumObject(enumType, "test");
            edmObject.IsNullable = true;

            Assert.True(edmObject.GetEdmType().IsNullable);
        }

        private class TestEdmEnumObject : EdmEnumObject
        {
            public TestEdmEnumObject(IEdmEnumType edmType, string value)
                : base(edmType, value)
            {
            }

            public TestEdmEnumObject(IEdmEnumTypeReference edmType, string value)
                : base(edmType, value)
            {
            }
        }
    }
}
