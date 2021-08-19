//-----------------------------------------------------------------------------
// <copyright file="OperationHelperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class OperationHelperTests
    {
        [Fact]
        public void VerifyAndBuildParameterMappings_ThrowsArgumentNull_InputParameters()
        {
            // Arrange & Act & Assert
            IEdmFunction function = null;
            ExceptionAssert.ThrowsArgumentNull(() => function.VerifyAndBuildParameterMappings(null), "function");

            // Arrange & Act & Assert
            function = new Mock<IEdmFunction>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => function.VerifyAndBuildParameterMappings(null), "parameters");
        }

        [Fact]
        public void VerifyAndBuildParameterMappings_ThrowsODataException_MissMatchParameters()
        {
            // Arrange
            IEdmTypeReference strType = EdmCoreModel.Instance.GetString(false);
            EdmFunction function = new EdmFunction("NS", "Function", strType);
            function.AddParameter("Name", strType);

            // Act & Assert
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "Other", "{Other}" }
            };
            Action test = () => function.VerifyAndBuildParameterMappings(parameters);
            ExceptionAssert.Throws<ODataException>(test, "Missing the required parameter 'Name' is in the operation 'NS.Function' parameter mapping.");

            // Act & Assert
            parameters["Name"] = "{Name}"; // Other is still in the dictionary
            test = () => function.VerifyAndBuildParameterMappings(parameters);
            ExceptionAssert.Throws<ODataException>(test, "Cannot find parameter 'Other' is in the operation 'NS.Function'.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Name")]
        public void VerifyAndBuildParameterMappings_ThrowsODataException_UnwrapperedWithCurlyBraces(string parameterTemplate)
        {
            // Arrange
            IEdmTypeReference strType = EdmCoreModel.Instance.GetString(false);
            EdmFunction function = new EdmFunction("NS", "Function", strType);
            function.AddParameter("Name", strType);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "Name", parameterTemplate }
            };

            // Act & Assert
            Action test = () => function.VerifyAndBuildParameterMappings(parameters);
            ExceptionAssert.Throws<ODataException>(test, $"Parameter template '{parameterTemplate}' in segment 'NS.Function' does not start with '{{' or ends with '}}'.");
        }

        [Fact]
        public void VerifyAndBuildParameterMappings_ThrowsODataException_EmptyTemplate()
        {
            // Arrange
            IEdmTypeReference strType = EdmCoreModel.Instance.GetString(false);
            EdmFunction function = new EdmFunction("NS", "Function", strType);
            function.AddParameter("Name", strType);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "Name", "{}" }
            };

            // Act & Assert
            Action test = () => function.VerifyAndBuildParameterMappings(parameters);
            ExceptionAssert.Throws<ODataException>(test, "Parameter alias 'Name' in segment 'NS.Function' is empty.");
        }

        [Fact]
        public void VerifyAndBuildParameterMappings_Works_FunctionParameter()
        {
            // Arrange
            IEdmTypeReference strType = EdmCoreModel.Instance.GetString(false);
            EdmFunction function = new EdmFunction("NS", "Function", strType);
            function.AddParameter("Name", strType);
            function.AddParameter("Title", strType);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "Name", "{NameValue}" },
                { "Title", "{TitleValue}" }
            };

            // Act
            IDictionary<string, string> actual = function.VerifyAndBuildParameterMappings(parameters);

            // Assert
            Assert.Equal(2, actual.Count);
            Assert.Collection(actual,
                e =>
                {
                    Assert.Equal("Name", e.Key);
                    Assert.Equal("NameValue", e.Value);
                },
                e =>
                {
                    Assert.Equal("Title", e.Key);
                    Assert.Equal("TitleValue", e.Value);
                });
        }

        [Fact]
        public void BuildParameterMappings_ThrowsArgumentNull_Parameters()
        {
            // Arrange & Act & Assert
            IEnumerable<OperationSegmentParameter> parameters = null;
            ExceptionAssert.ThrowsArgumentNull(() => parameters.BuildParameterMappings("function"), "parameters");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Name")]
        public void BuildParameterMappings_ThrowsODataException_UnwrapperedWithCurlyBraces(string parameterTemplate)
        {
            // Arrange
            IEnumerable<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>
            {
                new OperationSegmentParameter("Name", parameterTemplate)
            };

            // Act & Assert
            Action test = () => parameters.BuildParameterMappings("NS.Function");
            ExceptionAssert.Throws<ODataException>(test, $"Parameter template '{parameterTemplate}' in segment 'NS.Function' does not start with '{{' or ends with '}}'.");
        }

        [Fact]
        public void BuildParameterMappings_ThrowsODataException_EmptyTemplate()
        {
            // Arrange
            IEnumerable<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>
            {
                new OperationSegmentParameter("Name", "{}")
            };

            // Act & Assert
            Action test = () => parameters.BuildParameterMappings("NS.Function");
            ExceptionAssert.Throws<ODataException>(test, "Parameter alias 'Name' in segment 'NS.Function' is empty.");
        }

        [Fact]
        public void BuildParameterMappings_Works_FunctionParameter()
        {
            // Arrange
            IEnumerable<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>
            {
                new OperationSegmentParameter("Name", "{NameValue}"),
                new OperationSegmentParameter("Title", "{TitleValue}"),
            };

            // Act
            IDictionary<string, string> actual = parameters.BuildParameterMappings("NS.Function");

            // Assert
            Assert.Equal(2, actual.Count);
            Assert.Collection(actual,
                e =>
                {
                    Assert.Equal("Name", e.Key);
                    Assert.Equal("NameValue", e.Value);
                },
                e =>
                {
                    Assert.Equal("Title", e.Key);
                    Assert.Equal("TitleValue", e.Value);
                });
        }

        [Fact]
        public void GetFunctionParamterMappings_ThrowsArgumentNull_Parameters()
        {
            // Arrange & Act & Assert
            IEdmFunction function = null;
            ExceptionAssert.ThrowsArgumentNull(() => function.GetFunctionParamterMappings(), "function");

            // Arrange & Act & Assert
            IEdmFunctionImport functionImport = null;
            ExceptionAssert.ThrowsArgumentNull(() => functionImport.GetFunctionParamterMappings(), "functionImport");
        }

        [Fact]
        public void GetFunctionParamterMappings_Works_FunctionParameter()
        {
            // Arrange
            IEdmTypeReference strType = EdmCoreModel.Instance.GetString(false);
            EdmFunction function = new EdmFunction("NS", "Function", strType);
            function.AddParameter("Name", strType);
            function.AddParameter("Title", strType);

            // Act
            IDictionary<string, string> parameters = function.GetFunctionParamterMappings();

            // Assert
            Assert.Collection(parameters,
                e =>
                {
                    Assert.Equal("Name", e.Key);
                    Assert.Equal("{Name}", e.Value);
                },
                e =>
                {
                    Assert.Equal("Title", e.Key);
                    Assert.Equal("{Title}", e.Value);
                });

            // Act
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "FunctionImport", function);

            IDictionary<string, string> parametersImport = parameters = function.GetFunctionParamterMappings();

            // Assert
            Assert.Equal(parameters, parametersImport);
        }

        [Fact]
        public void SplitOperationImports_ThrowsArgumentNull_OperationImports()
        {
            // Arrange & Act & Assert
            IEnumerable<IEdmOperationImport> operationImports = null;
            ExceptionAssert.ThrowsArgumentNull(() => operationImports.SplitOperationImports(), "operationImports");
        }

        [Fact]
        public void SplitOperationImports_Works_OperationImports()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            IEdmTypeReference strType = EdmCoreModel.Instance.GetString(false);
            EdmFunction function = new EdmFunction("NS", "Function", strType);
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "functionImport", function);
            EdmAction action = new EdmAction("NS", "Action", strType);
            EdmActionImport actionImport = new EdmActionImport(container, "actionImport", action);

            IEnumerable<IEdmOperationImport> operationImports = new List<IEdmOperationImport>
            {
                functionImport,
                actionImport
            };

            // Act
            (var actionImports, var functionImports) = operationImports.SplitOperationImports();

            // Assert
            Assert.Same(actionImport, Assert.Single(actionImports));
            Assert.Same(functionImport, Assert.Single(functionImports));
        }
    }
}
