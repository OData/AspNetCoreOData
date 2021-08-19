//-----------------------------------------------------------------------------
// <copyright file="CustomAggregateMethodAnnotationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class CustomAggregateMethodAnnotationTests
    {
        [Fact]
        public void CustomAggregateMethodAnnotation_Works_RoundTrip()
        {
            // Arrange
            MethodInfo methodInfo = new Mock<MethodInfo>().Object;

            IDictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>
            {
                { typeof(int), methodInfo }
            };

            CustomAggregateMethodAnnotation annotation = new CustomAggregateMethodAnnotation();

            // Act & Assert
            annotation.AddMethod("token", methods);

            Assert.True(annotation.GetMethodInfo("token", typeof(int), out MethodInfo actual));
            Assert.Same(methodInfo, actual);

            Assert.False(annotation.GetMethodInfo("unknown", typeof(int), out _));
        }
    }
}
