// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataSingletonDeserializerTest
    {
        private IEdmModel _edmModel;
        private IEdmSingleton _singleton;
        private readonly ODataDeserializerContext _readContext;
        private readonly IODataDeserializerProvider _deserializerProvider;

        private sealed class EmployeeModel
        {
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
        }

        public ODataSingletonDeserializerTest()
        {
            EdmModel model = new EdmModel();
            var employeeType = new EdmEntityType("NS", "Employee");
            employeeType.AddStructuralProperty("EmployeeId", EdmPrimitiveTypeKind.Int32);
            employeeType.AddStructuralProperty("EmployeeName", EdmPrimitiveTypeKind.String);
            model.AddElement(employeeType);

            EdmEntityContainer defaultContainer = new EdmEntityContainer("NS", "Default");
            model.AddElement(defaultContainer);

            _singleton = new EdmSingleton(defaultContainer, "CEO", employeeType);
            defaultContainer.AddElement(_singleton);

            model.SetAnnotationValue<ClrTypeAnnotation>(employeeType, new ClrTypeAnnotation(typeof(EmployeeModel)));

            _edmModel = model;

            _readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new SingletonSegment(_singleton)),
                Model = _edmModel,
                ResourceType = typeof(EmployeeModel)
            };

            _deserializerProvider = DeserializationServiceProviderHelper.GetServiceProvider().GetRequiredService<IODataDeserializerProvider>();
        }

        [Fact]
        public async Task CanDeserializerSingletonPayloadFromStream()
        {
            // Arrange
            const string payload = "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#CEO\"," +
                "\"EmployeeId\":789," +
                "\"EmployeeName\":\"John Hark\"}";

            ODataResourceDeserializer deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act
            EmployeeModel employee = await deserializer.ReadAsync(
                GetODataMessageReader(payload),
                typeof(EmployeeModel),
                _readContext) as EmployeeModel;

            // Assert
            Assert.NotNull(employee);
            Assert.Equal(789, employee.EmployeeId);
            Assert.Equal("John Hark", employee.EmployeeName);
        }

        private ODataMessageReader GetODataMessageReader(string content)
        {
            HttpRequest request = RequestFactory.Create("Post", "http://localhost/odata/CEO", opt => opt.AddRouteComponents("odata", _edmModel));

            //request.Content = new StringContent(content);
            //request.Headers.Add("OData-Version", "4.0");

            //MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            //mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            //request.Headers.Accept.Add(mediaType);
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            request.Body = new MemoryStream(contentBytes);
            request.ContentType = "application/json";
            request.ContentLength = contentBytes.Length;
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Accept", "application/json;odata.metadata=full");

            return new ODataMessageReader(new HttpRequestODataMessage(request), new ODataMessageReaderSettings(), _edmModel);
        }
    }
}
