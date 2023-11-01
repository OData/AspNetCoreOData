//-----------------------------------------------------------------------------
// <copyright file="FilterValidatorContextTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Validator;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
    public class FilterValidatorContextTests
    {
        [Fact]
        public void Clone_ClosesContext()
        {
            // Arrange & Act & Assert
            FilterValidatorContext context = new FilterValidatorContext
            {
                ValidationSettings = new ODataValidationSettings { MaxNodeCount = 99 }
            };

            FilterValidatorContext newContext = context.Clone();
            Assert.Equal(99, newContext.ValidationSettings.MaxNodeCount);
        }

        [Fact]
        public void IncrementNodeCount_IncreasesNodeCount()
        {
            // Arrange & Act & Assert
            FilterValidatorContext context = new FilterValidatorContext
            {
                ValidationSettings = new ODataValidationSettings { MaxNodeCount = 100 }
            };
            Assert.Equal(0, context.CurrentNodeCount);

            context.IncrementNodeCount();
            Assert.Equal(1, context.CurrentNodeCount);
        }

        [Fact]
        public void EnterLambda_IncreasesAnyAllExpressionDepth()
        {
            // Arrange
            FilterValidatorContext context = new FilterValidatorContext
            {
                ValidationSettings = new ODataValidationSettings { MaxAnyAllExpressionDepth = 100 }
            };
            Assert.Equal(0, context.CurrentAnyAllExpressionDepth);

            context.EnterLambda();
            Assert.Equal(1, context.CurrentAnyAllExpressionDepth);
        }

        [Fact]
        public void ExitLambda_DecreasesAnyAllExpressionDepth()
        {
            // Arrange
            FilterValidatorContext context = new FilterValidatorContext
            {
                ValidationSettings = new ODataValidationSettings { MaxAnyAllExpressionDepth = 100 }
            };
            Assert.Equal(0, context.CurrentAnyAllExpressionDepth);

            context.EnterLambda();
            context.EnterLambda();
            Assert.Equal(2, context.CurrentAnyAllExpressionDepth);
            context.ExitLambda();
            Assert.Equal(1, context.CurrentAnyAllExpressionDepth);
        }
    }
}
