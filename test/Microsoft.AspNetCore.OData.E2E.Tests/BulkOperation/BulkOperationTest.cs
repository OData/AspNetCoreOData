//-----------------------------------------------------------------------------
// <copyright file="BulkOperationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation.BulkOperationController;

namespace Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation
{
    public class BulkOperationTest : WebApiTestBase<BulkOperationTest>
    {
        public BulkOperationTest(WebApiTestFixture<BulkOperationTest> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(EmployeesController));

            var edmModel1 = BulkOperationEdmModel.GetConventionModel();
            var edmModel2 = BulkOperationEdmModel.GetExplicitModel();
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("convention", edmModel1)
                .AddRouteComponents("explicit", edmModel2).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
        }

        [Fact]
        public async Task DeltaSet_WithNestedFriends_WithNestedOrders_IsSerializedSuccessfully()
        {
            //Arrange
            string requestUri = "/convention/Employees";

            HttpClient client = CreateClient();

            var content = @"{
                '@context':'http://host/service/$metadata#Employees/$delta',
                'value':[
                    {'ID':1,'Name':'Employee1','Friends@delta':[{'Id':1,'Name':'Friend1','Orders@delta':[{'Id':1,'Price': 10},{'Id':2,'Price':20} ]},{'Id':2,'Name':'Friend2'}]},
                    {'ID':2,'Name':'Employee2','Friends@delta':[{'Id':3,'Name':'Friend3','Orders@delta':[{'Id':3,'Price': 30}, {'Id':4,'Price':40} ]},{'Id':4,'Name':'Friend4'}]}
                ]}";

            string expectedResponse = "{" +
                "\"@context\":\"http://localhost/convention/$metadata#Employees/$delta\"," +
                "\"value\":[" +
                "{\"ID\":1,\"Name\":\"Employee1\",\"Friends@delta\":[{\"Id\":1,\"Name\":\"Friend1\",\"Orders@delta\":[{\"Id\":1,\"Price\":10},{\"Id\":2,\"Price\":20}]},{\"Id\":2,\"Name\":\"Friend2\"}]}," +
                "{\"ID\":2,\"Name\":\"Employee2\",\"Friends@delta\":[{\"Id\":3,\"Name\":\"Friend3\",\"Orders@delta\":[{\"Id\":3,\"Price\":30},{\"Id\":4,\"Price\":40}]},{\"Id\":4,\"Name\":\"Friend4\"}]}]}";

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");

            // Act & Assert
            using HttpResponseMessage response = await client.PatchAsync(requestUri, stringContent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(expectedResponse.ToString().ToLower(), json.ToString().ToLower());
        }

        [Fact]
        public async Task DeltaSet_WithDeletedAndODataId_IsSerializedSuccessfully()
        {
            //Arrange
            string requestUri = "/convention/Employees";
            HttpClient client = CreateClient();

            var content = @"{
                '@context':'http://host/service/$metadata#Employees/$delta',
                'value':[
                    {'ID':1,'Name':'Employee1','Friends@odata.delta':[{'@removed':{'reason':'changed'},'Id':1}]},
                    {'ID':2,'Name':'Employee2','Friends@odata.delta':[{'@id':'Friends(1)'}]}
                ]}";

            string expectedResponse = "{\"@context\":\"http://localhost/convention/$metadata#Employees/$delta\",\"value\":[{\"ID\":1,\"Name\":\"Employee1\",\"Friends@delta\":[{\"@removed\":{\"reason\":\"changed\"},\"@id\":\"http://host/service/Friends(1)\"}]},{\"ID\":2,\"Name\":\"Employee2\",\"Friends@delta\":[{\"Id\":1}]}]}";

            var requestForUpdate = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForUpdate.Content = stringContent;

            //Act & Assert
            using HttpResponseMessage response = await client.SendAsync(requestForUpdate);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(expectedResponse.ToString().ToLower(), json.ToString().ToLower());
        }
    }
}
