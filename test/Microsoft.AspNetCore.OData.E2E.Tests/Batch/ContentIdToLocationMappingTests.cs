//-----------------------------------------------------------------------------
// <copyright file="ContentIdToLocationMappingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch
{
    public class ContentIdToLocationMappingTests : WebApiTestBase<ContentIdToLocationMappingTests>
    {
        private static IEdmModel edmModel;

        public ContentIdToLocationMappingTests(WebApiTestFixture<ContentIdToLocationMappingTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(
                typeof(ContentIdToLocationMappingParentsController),
                typeof(ContentIdToLocationMappingChildrenController));

            edmModel = GetEdmModel();
            services.AddControllers().AddOData(opt =>
            {
                opt.EnableQueryFeatures();
                opt.EnableContinueOnErrorHeader = true;
                opt.AddRouteComponents("ContentIdToLocationMapping", edmModel, new DefaultODataBatchHandler());
            });
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            app.UseODataBatching();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ContentIdToLocationMappingParent>("ContentIdToLocationMappingParents");
            builder.EntitySet<ContentIdToLocationMappingChild>("ContentIdToLocationMappingChildren");
            builder.Namespace = typeof(ContentIdToLocationMappingParent).Namespace;

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanResolveContentIdInODataBindAnnotationAsync()
        {
            // Arrange
            HttpClient client = CreateClient();
            string serviceBase = $"{client.BaseAddress}ContentIdToLocationMapping";
            string requestUri = $"{serviceBase}/$batch";
            string parentsUri = $"{serviceBase}/ContentIdToLocationMappingParents";
            string childrenUri = $"{serviceBase}/ContentIdToLocationMappingChildren";
            string payload = "{" +
                "  \"requests\": [" +
                "    {" +
                "      \"id\": \"1\"," +
                "      \"method\": \"POST\"," +
                $"      \"url\": \"{parentsUri}\"," +
                "      \"headers\": {" +
                "        \"OData-Version\": \"4.0\"," +
                "        \"Content-Type\": \"application/json;odata.metadata=minimal\"," +
                "        \"Accept\": \"application/json;odata.metadata=minimal\"" +
                "      }," +
                "      \"body\": {\"ParentId\":123}" +
                "    }," +
                "    {" +
                "      \"id\": \"2\"," +
                "      \"method\": \"POST\"," +
                $"      \"url\": \"{childrenUri}\"," +
                "      \"headers\": {" +
                "        \"OData-Version\": \"4.0\"," +
                "        \"Content-Type\": \"application/json;odata.metadata=minimal\"," +
                "        \"Accept\": \"application/json;odata.metadata=minimal\"" +
                "      }," +
                "      \"body\": {" +
                "        \"Parent@odata.bind\": \"$1\"" +
                "      }" +
                "    }" +
                "  ]" +
                "}";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
            {
                var batchReader = messageReader.CreateODataBatchReader();
                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            Assert.Equal(201, operationMessage.StatusCode);
                            break;
                    }
                }
            }

            // NOTE: We assert that $1 is successfully resolved from the controller action
            Assert.Equal(2, subResponseCount);
        }
    }

    public class ContentIdToLocationMappingParentsController : ODataController
    {
        public ActionResult Post([FromBody] ContentIdToLocationMappingParent parent)
        {
            return Created(new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{parent.ParentId}"), parent);
        }
    }

    public class ContentIdToLocationMappingChildrenController : ODataController
    {
        public ActionResult Post([FromBody] ContentIdToLocationMappingChild child)
        {
            Assert.Equal(123, child.Parent.ParentId);

            return Created(new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{child.ChildId}"), child);
        }
    }

    public class ContentIdToLocationMappingParent
    {
        public ContentIdToLocationMappingParent()
        {
            Children = new HashSet<ContentIdToLocationMappingChild>();
        }

        [Key]
        public int ParentId
        {
            get; set;
        }

        public virtual ICollection<ContentIdToLocationMappingChild> Children
        {
            get; set;
        }
    }

    public class ContentIdToLocationMappingChild
    {
        [Key]
        public int ChildId
        {
            get; set;
        }

        public int? ParentId
        {
            get; set;
        }

        public virtual ContentIdToLocationMappingParent Parent
        {
            get; set;
        }
    }
}
