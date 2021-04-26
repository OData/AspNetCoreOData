// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.StreamProperty
{
    public class StreamPropertyTests : WebApiTestBase<StreamPropertyTests>
    {
        private static IEdmModel EdmModel;

        public StreamPropertyTests(WebApiTestFixture<StreamPropertyTests> fixture)
           : base(fixture)
        {
        }

        [Fact]
        public async Task GetMetadata_WithStreamProperty()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/$metadata");

            // Assert
            var payload = await response.Content.ReadAsStringAsync();
            Assert.Contains("<Property Name=\"Photo\" Type=\"Edm.Stream\">" +
                "<Annotation Term=\"Org.OData.Core.V1.AcceptableMediaTypes\">" +
                  "<Collection>" +
                    "<String>application/javascript</String>" +
                    "<String>image/png</String>" +
                  "</Collection>" +
                "</Annotation>" +
              "</Property>", payload);
        }

        [Fact]
        public async Task Get_EntityWithStreamProperty()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/StreamCustomers(1)");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("http://localhost/odata/$metadata#StreamCustomers/$entity", result["@odata.context"]);
            Assert.Equal("#Microsoft.AspNetCore.OData.E2E.Tests.StreamProperty.StreamCustomer", result["@odata.type"]);
            Assert.Equal("\u0002\u0003\u0004\u0005", result["PhotoText"]);
            Assert.Equal("http://localhost/odata/StreamCustomers(1)/Photo", result["Photo@odata.mediaEditLink"]);
            Assert.Equal("http://localhost/odata/StreamCustomers(1)/Photo", result["Photo@odata.mediaReadLink"]);
        }

        [Fact]
        public async Task Get_SingleStreamProperty()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/StreamCustomers(2)/Photo");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/octet-stream", response.Content.Headers.ContentType.MediaType);
            var stream = await response.Content.ReadAsStreamAsync();

            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            Assert.Equal("\u0003\u0004\u0005\u0006", text);

            byte[] byteArray = ReadAllBytes(stream);
            Assert.Equal(new byte[] { 3, 4, 5, 6 }, byteArray);
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            EdmModel = GetEdmModel();

            services.ConfigureControllers(typeof(MetadataController), typeof(StreamCustomersController));

            services.AddControllers().AddOData(opt => opt.AddModel("odata", EdmModel));
        }

        public static byte[] ReadAllBytes(Stream instream)
        {
            if (instream is MemoryStream)
            {
                return ((MemoryStream)instream).ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                instream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<StreamCustomer>("StreamCustomers");
            EdmModel model = builder.GetEdmModel() as EdmModel;

            IEdmEntityType streamCustomerType = model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(c => c.Name == "StreamCustomer");
            Assert.NotNull(streamCustomerType);
            IEdmProperty photoProperty = streamCustomerType.FindProperty("Photo");

            EdmStringConstant strConstant1 = new EdmStringConstant("application/javascript");
            EdmStringConstant strConstant2 = new EdmStringConstant("image/png");
            EdmCollectionExpression collectionExpression = new EdmCollectionExpression(strConstant1, strConstant2);
            EdmVocabularyAnnotation annotation = new EdmVocabularyAnnotation(photoProperty, CoreVocabularyModel.AcceptableMediaTypesTerm, collectionExpression);
            annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
            model.AddVocabularyAnnotation(annotation);

            return model;
        }
    }

    public class StreamCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // this property saves the string of the Photo
        public string PhotoText { get; set; }

        public Stream Photo { get; set; }
    }
}