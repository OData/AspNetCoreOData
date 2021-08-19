//-----------------------------------------------------------------------------
// <copyright file="MockPropertyInfo.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Moq;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    /// <summary>
    /// A mock to represent a property info.
    /// </summary>
    public sealed class MockPropertyInfo : Mock<PropertyInfo>
    {
        private readonly Mock<MethodInfo> _mockGetMethod = new Mock<MethodInfo>();
        private readonly Mock<MethodInfo> _mockSetMethod = new Mock<MethodInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MockPropertyInfo"/> class.
        /// </summary>
        public MockPropertyInfo()
            : this(typeof(object), "P")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockPropertyInfo"/> class.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="propertyName">The property name.</param>
        public MockPropertyInfo(Type propertyType, string propertyName)
        {
            SetupGet(p => p.DeclaringType).Returns(typeof(object));
            SetupGet(p => p.ReflectedType).Returns(typeof(object));
            SetupGet(p => p.Name).Returns(propertyName);
            SetupGet(p => p.PropertyType).Returns(propertyType);
            SetupGet(p => p.CanRead).Returns(true);
            SetupGet(p => p.CanWrite).Returns(true);
            Setup(p => p.GetGetMethod(It.IsAny<bool>())).Returns(_mockGetMethod.Object);
            Setup(p => p.GetSetMethod(It.IsAny<bool>())).Returns(_mockSetMethod.Object);
            Setup(p => p.Equals(It.IsAny<object>())).Returns<PropertyInfo>(p => ReferenceEquals(Object, p));

            _mockGetMethod.SetupGet(m => m.Attributes).Returns(MethodAttributes.Public);
        }

        /// <summary>
        /// Implicit operator to convert Mock property info to property info.
        /// </summary>
        /// <param name="mockPropertyInfo">The left mock property info.</param>
        public static implicit operator PropertyInfo(MockPropertyInfo mockPropertyInfo)
        {
            return mockPropertyInfo.Object;
        }

        /// <summary>
        /// Set up the property as abstract.
        /// </summary>
        /// <returns></returns>
        public MockPropertyInfo Abstract()
        {
            _mockGetMethod.SetupGet(m => m.Attributes)
                .Returns(_mockGetMethod.Object.Attributes | MethodAttributes.Abstract);

            return this;
        }
    }
}
