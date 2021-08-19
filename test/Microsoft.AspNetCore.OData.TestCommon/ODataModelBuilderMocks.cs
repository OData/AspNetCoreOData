//-----------------------------------------------------------------------------
// <copyright file="ODataModelBuilderMocks.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    public static class ODataModelBuilderMocks
    {
        // Creates a mock of an ODataModelBuilder or any subclass of it that disables model validation
        // in order to reduce verbosity on tests.
        public static T GetModelBuilderMock<T>() where T : ODataModelBuilder
        {
            Mock<T> mock;
            if (typeof(T) == typeof(ODataConventionModelBuilder))
            {
                mock = new Mock<T>();
            }
            else
            {
                mock = new Mock<T>();
            }

            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            return mock.Object;
        }
    }
}
