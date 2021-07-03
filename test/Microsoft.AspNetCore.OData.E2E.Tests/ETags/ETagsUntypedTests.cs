// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags
{
    public class ETagsUntypedTests : WebApiTestBase<ETagsUntypedTests>
    {
        public ETagsUntypedTests(WebApiTestFixture<ETagsUntypedTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();
            services.ConfigureControllers(typeof(ETagUntypedCustomersController), typeof(MetadataController));
            services.AddControllers().AddOData(opt => opt.Select().AddRouteComponents("odata", edmModel));

            services.AddControllers(opt => opt.Filters.Add(new ETagActionFilterAttribute()));
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            IEdmStructuralProperty customerName = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            EdmEntitySet customers = container.AddEntitySet("ETagUntypedCustomers", customer);

            model.SetOptimisticConcurrencyAnnotation(customers, new[] { customerName });

            return model;
        }

        [Fact]
        public async Task ModelBuilderTest()
        {
            // Arrange
            string expectMetadata =
                "<EntitySet Name=\"ETagUntypedCustomers\" EntityType=\"NS.Customer\">\r\n" +
                "          <Annotation Term=\"Org.OData.Core.V1.OptimisticConcurrency\">\r\n" +
                "            <Collection>\r\n" +
                "              <PropertyPath>Name</PropertyPath>\r\n" +
                "            </Collection>\r\n" +
                "          </Annotation>\r\n" +
                "        </EntitySet>";

            // Remove indentation
            expectMetadata = Regex.Replace(expectMetadata, @"\r\n\s*<", @"<");
            HttpClient client = CreateClient();
            string requestUri = "odata/$metadata";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, content);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();
            Assert.NotNull(edmModel);

            var etagCustomers = edmModel.FindDeclaredEntitySet("ETagUntypedCustomers");
            Assert.NotNull(etagCustomers);

            var annotations = edmModel.FindDeclaredVocabularyAnnotations(etagCustomers);
            IEdmVocabularyAnnotation annotation = Assert.Single(annotations);
            Assert.NotNull(annotation);

            Assert.Same(CoreVocabularyModel.ConcurrencyTerm, annotation.Term);
            Assert.Same(etagCustomers, annotation.Target);

            IEdmVocabularyAnnotation valueAnnotation = annotation as IEdmVocabularyAnnotation;
            Assert.NotNull(valueAnnotation);
            Assert.NotNull(valueAnnotation.Value);

            IEdmCollectionExpression collection = valueAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(collection);
            Assert.Equal(new[] { "Name" }, collection.Elements.Select(e => ((IEdmPathExpression)e).PathSegments.Single()));
        }

        [Fact]
        public async Task PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            // Arrange
            HttpClient client = CreateClient();
            string requestUri = "odata/ETagUntypedCustomers(1)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var etagInHeader = response.Headers.ETag.ToString();
            JObject result = await response.Content.ReadAsObject<JObject>();
            var etagInPayload = (string)result["@odata.etag"];

            Assert.True(etagInPayload == etagInHeader);
            Assert.Equal("W/\"J1NhbSc=\"", etagInPayload);
        }
    }

    public class ETagUntypedCustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get(int key)
        {
            IEdmModel model = Request.GetModel();
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            EdmEntityObject customer = new EdmEntityObject(entityType);
            customer.TrySetPropertyValue("ID", key);
            customer.TrySetPropertyValue("Name", "Sam");
            return Ok(customer);
        }
    }
}