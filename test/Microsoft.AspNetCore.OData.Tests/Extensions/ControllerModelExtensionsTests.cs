//-----------------------------------------------------------------------------
// <copyright file="ControllerModelExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class ControllerModelExtensionsTests
    {
        [Fact]
        public void IsODataIgnored_ThrowsArgumentNull_Controller()
        {
            // Arrange & Act & Assert
            ControllerModel controller = null;
            ExceptionAssert.ThrowsArgumentNull(() => controller.IsODataIgnored(), "controller");
        }

        [Fact]
        public void HasAttribute_ThrowsArgumentNull_Controller()
        {
            // Arrange & Act & Assert
            ControllerModel controller = null;
            ExceptionAssert.ThrowsArgumentNull(() => controller.HasAttribute<Attribute>(), "controller");
        }

        [Fact]
        public void GetAttribute_ThrowsArgumentNull_Controller()
        {
            // Arrange & Act & Assert
            ControllerModel controller = null;
            ExceptionAssert.ThrowsArgumentNull(() => controller.GetAttribute<Attribute>(), "controller");
        }
    }
}
