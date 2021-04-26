// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance
{
    public class ComplexTypeInheritanceTests : WebApiTestBase<ComplexTypeInheritanceTests>
    {
        public ComplexTypeInheritanceTests(WebApiTestFixture<ComplexTypeInheritanceTests> fixture)
            :base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(MetadataController), typeof(WindowsController));

            var edmModel1 = ComplexTypeInheritanceEdmModels.GetConventionModel();
            var edmModel2 = ComplexTypeInheritanceEdmModels.GetExplicitModel();
            services.AddControllers().AddOData(opt => opt.AddModel("convention", edmModel1)
                .AddModel("explicit", edmModel2).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
        }

        public static TheoryDataSet<string, string> MediaTypes
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] mimes = new string[]{
                    "json",
                    "application/json",
                    "application/json;odata.metadata=none",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full"};
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string mime in mimes)
                    {
                        data.Add(mode, mime);
                    }
                }
                return data;
            }
        }

        public static TheoryDataSet<string, string, string,bool> PostToCollectionNewComplexTypeMembers
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] targets = { "OptionalShapes", "PolygonalShapes" };
                bool[] representations = { true, false };
                string[] objects = new string[]
                {
                    @"
{
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Polygon',
        'HasBorder':true,'Vertexes':[
            {'X':21,'Y':12},
            {'X':32,'Y':23},
            {'X':14,'Y':41}
        ]
}",
                    @"
{
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Rectangle',
        'HasBorder':true,
        'Width':3,
        'Height':4,
        'TopLeft':{ 'X':1,'Y':2}
}",
                };

                TheoryDataSet<string, string, string, bool> data = new TheoryDataSet<string, string, string, bool>();

                foreach(string mode in modes)
                {
                    foreach(string obj in objects)
                    {
                        foreach(string target in targets)
                            foreach(bool representation in representations)
                            {
                                data.Add(mode, obj, target, representation);
                            }
                    }
                }
                return data;
            }
        }

        #region CRUD on the entity
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // POST ~/Windows
        public async Task CreateWindow(string mode)
        {
            string requestUri = $"{mode}/Windows";
            string content = @"
{
    'Id':0,
    'Name':'Name4',
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',  
        'Radius':10,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes':
    [
        {
            '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Rectangle',
            'HasBorder':true,
            'Width':3,
            'Height':4,
            'TopLeft':{ 'X':1,'Y':2}
        }
    ]
}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;
            HttpClient client = CreateClient();
            var response = await client.SendAsync(request);

            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.Created == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                response.StatusCode,
                requestUri,
                contentOfString));

            Assert.Equal("4.0", response.Headers.GetValues("OData-Version").Single());
            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("Name4" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(10 == radius,
                String.Format("\nExpected that Radius: 10, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray optionalShapes = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(1 == optionalShapes.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, optionalShapes.Count, requestUri, contentOfString));
            JArray vertexes = optionalShapes[0]["Vertexes"] as JArray;
            Assert.True(4 == vertexes.Count, "The returned OptionalShapes is not as expected");
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // GET ~/Windows?$select=...&$orderby=...&$expand=...
        public async Task QueryCollectionContainingEntity(string mode, string mime)
        {
            string requestUri = $"{mode}/Windows?$select=Id,CurrentShape,OptionalShapes&$orderby=CurrentShape/HasBorder&$expand=Parent&$format={mime}";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsObject<JObject>();
            JArray windows = content["value"] as JArray;
            Assert.True(3 == windows.Count);

            JObject window1 = (JObject)windows.Single(w => (string)w["Id"] == "1");
            JArray optionalShapes = (JArray)window1["OptionalShapes"];
            Assert.True(1 == optionalShapes.Count);

            JObject window2 = (JObject)windows.Single(w => (string)w["Id"] == "2");
            Assert.Equal("1", (string)window2["Parent"]["Id"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // GET ~/Windows?$filter=CurrentShape/HasBorder eq true
        public async Task QueryEntitiesFilteredByComplexType(string mode, string mime)
        {
            string requestUri = $"{mode}/Windows?$filter=CurrentShape/HasBorder eq true&$format={mime}";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsObject<JObject>();
            JArray windows = content["value"] as JArray;
            Assert.True(1 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Windows(3)
        public async Task PutContainingEntity(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(3)";

            string content = @"
{
    'Id':3,
    'Name':'Name30',
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',
        'Radius':2,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes':
    [
        {
            '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Rectangle',
            'HasBorder':true,
            'Width':3,
            'Height':4,
            'TopLeft':{ 'X':1,'Y':2}
        }
    ]
}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("Name30" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray windows = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(1 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(1)
        public async Task PatchContainingEntity(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(1)";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            // We should be able to PATCH nested resource with delta object of the same CLR type.
            var content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',  
        'Radius':1,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes': [ ]
}";
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            request.Content = stringContent;
            request.Content.Headers.ContentLength = content.Length;
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            string contentOfString = await response.Content.ReadAsStringAsync();
            if (HttpStatusCode.OK != response.StatusCode)
            {
                Assert.True(false, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            }
            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("CircleWindow" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(1 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray windows = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(0 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(3)
        public async Task PatchContainingEntity_MismatchedRuntimeTypeError(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(3)";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            // Attempt to PATCH nested resource with delta object of the different CLR type
            // will result an error.
            var content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',
        'Radius':2,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes': [ ]
}";
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            request.Content = stringContent;
            request.Content.Headers.ContentLength = content.Length;
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(3)
        public async Task PatchContainingEntity_DeltaIsBaseType(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(3)";

            // PATCH nested resource with delta object of the base CLR type should work.
            // --- PATCH #1 ---
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            var content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Polygon',
        'HasBorder':true
    },
    'OptionalShapes': [ ]
}";
            string contentOfString = await ExecuteAsync(request, content);

            // Only 'HasBoarder' is updated; 'Vertexes' still has the correct value.
            Assert.Contains("\"HasBorder\":true", contentOfString);
            Assert.Contains("\"Vertexes\":[{\"X\":0,\"Y\":0},{\"X\":2,\"Y\":0},{\"X\":2,\"Y\":2},{\"X\":0,\"Y\":2}]", contentOfString);

            // --- PATCH #2 ---
            request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Polygon',
        'Vertexes':[ {'X':1,'Y':2}, {'X':2,'Y':3}, {'X':4,'Y':8} ]
    },
    'OptionalShapes': [ ]
}";
            contentOfString = await ExecuteAsync(request, content);

            // Only 'Vertexes' is updated;  'HasBoarder' still has the correct value.
            Assert.Contains("\"Vertexes\":[{\"X\":1,\"Y\":2},{\"X\":2,\"Y\":3},{\"X\":4,\"Y\":8}]", contentOfString);
            Assert.Contains("\"HasBorder\":false", contentOfString);
        }

        private async Task<string> ExecuteAsync(HttpRequestMessage request, string content)
        {
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            request.Content = stringContent;
            request.Content.Headers.ContentLength = content.Length;
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // DELETE ~/Windows(1)
        public async Task DeleteWindow(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(1)";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        #endregion

        #region RUD on complex type

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // GET ~/Windows(1)/CurrentShape
        public async Task QueryComplexTypeProperty(string mode, string mime)
        {
            string requestUri = $"{mode}/Windows(1)/CurrentShape?$format={mime}";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsObject<JObject>();
            bool hasBorder = (bool)content["HasBorder"];
            Assert.True(hasBorder,
                String.Format("\nExpected that HasBorder is true, but actually not,\n request uri: {0},\n response payload: {1}", requestUri, contentOfString));
            int radius = (int)content["Radius"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // GET ~/Windows(1)/OptionalShapes/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle
        public async Task GetOptionalShapesPlusCast(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(3)/OptionalShapes/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            JArray optionalShapes = (JArray)contentOfJObject["value"];
            Assert.True(1 == optionalShapes.Count);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // GET ~/Windows(3)/OptionalShapes
        public async Task GetOptionalShapes(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(3)/OptionalShapes";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            JArray optionalShapes = (JArray)contentOfJObject["value"];
            Assert.True(2 == optionalShapes.Count);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // GET ~/Windows(1)/CurrentShape/HasBorder
        public async Task QueryPropertyDefinedInComplexTypeProperty(string mode, string mime)
        {
            string requestUri = $"{mode}/Windows(1)/CurrentShape/HasBorder?$format={mime}";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsObject<JObject>();
            bool hasBorder = (bool)content["value"];
            Assert.True(hasBorder,
                String.Format("\nExpected that HasBorder is true, but actually not,\n request uri: {0},\n response payload: {1}", requestUri, contentOfString));
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // GET ~/Windows(1)/CurrentShape/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle/Radius
        public async Task QueryComplexTypePropertyDefinedOnDerivedType(string mode, string mime)
        {
            string requestUri = $"{mode}/Windows(1)/CurrentShape/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle/Radius?$format={mime}";

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsObject<JObject>();
            int radius = (int)content["value"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Windows(3)/OptionalShapes
        public async Task PutCollectionComplexTypeProperty(string modelMode)
        {
            string requestUri = $"{modelMode}/Windows(3)/OptionalShapes";

            var content = new StringContent(content: @"
{
  'value':[
    {
        '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Polygon',
        'HasBorder':true,'Vertexes':[
        {'X':1,'Y':2},
        {'X':2,'Y':3},
        {'X':4,'Y':8}
      ]
    },
    {
      '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',
        'HasBorder':true,
        'Center':{'X':3,'Y':3},
        'Radius':2
    }
  ]
}
", encoding: Encoding.UTF8, mediaType: "application/json");

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.PutAsync(requestUri, content);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            Assert.True(2 == contentOfJObject.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}",
                2,
                contentOfJObject.Count,
                requestUri,
                contentOfString));
        }

        [Theory]
        [MemberData(nameof(PostToCollectionNewComplexTypeMembers))]
        // POST ~/Windows(3)/OptionalShapes
        public async Task PostToCollectionComplexTypeProperty(string modelMode, string jObject, string targetPropertyResource, bool returnRepresentation)
        {
            //Arrange
            string requestUri = $"{modelMode}/Windows(3)/"+ targetPropertyResource;

            //send a get request to get the current count
            int count = 0;
            HttpClient client = CreateClient();
            using (HttpResponseMessage getResponse = await client.GetAsync(requestUri))
            {
                getResponse.EnsureSuccessStatusCode();

                var json = await getResponse.Content.ReadAsObject<JObject>();
                var state = json.GetValue("value") as JArray;
                count = state.Count;
            }

            //Set up the post request
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(content:jObject, encoding: Encoding.UTF8, mediaType: "application/json");
            if (returnRepresentation)
            {
                requestForPost.Headers.Add("Prefer", "return=representation");
            }
            requestForPost.Content.Headers.ContentLength = jObject.Length;

            //Act & Assert
            HttpResponseMessage response = await client.SendAsync(requestForPost);
            string contentOfString = await response.Content.ReadAsStringAsync();

            if(returnRepresentation)
            {
                JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
                var result = contentOfJObject.GetValue("value") as JArray;
            
                Assert.True(count + 1 == result.Count,
                    String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.NoContent,
                    result.Count,
                    requestUri,
                    contentOfString));
            }
            else
            {
                Assert.True(HttpStatusCode.NoContent == response.StatusCode,
                    String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.NoContent,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            }
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Widnows(1)/CurrentShape
        public async Task PutCurrentShape(string modelMode)
        {
            // Arrange
            string requestUri = $"{modelMode}/Windows(1)/CurrentShape/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle";
            string content = @"
{
    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',
    'Radius':5,
    'Center':{'X':1,'Y':2},
    'HasBorder':true 
}";

            // Act
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            int radius = (int)contentOfJObject["Radius"];
            Assert.True(5 == radius,
                String.Format("\nExpected that Radius: 5, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PATCH ~/Windows(3)/OptionalShapes
        public async Task PatchToCollectionComplexTypePropertyNotSupported(string modelMode)
        {
            // Arrange
            string requestUri = $"{modelMode}/Windows(3)/OptionalShapes";

            // Act
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.PatchAsync(requestUri, "");

            // Assert
            Assert.True(HttpStatusCode.MethodNotAllowed == response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task PatchToSingleComplexTypeProperty(string modelMode)
        {
            // Arrange
            string requestUri = $"{modelMode}/Windows(1)/CurrentShape/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle";
            string content = @"
{
    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle',
    'Radius':15,
    'HasBorder':true
}";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;

            // Act
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            int radius = (int)contentOfJObject["Radius"];
            Assert.Equal(15, radius);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task DeleteToNullableComplexTypeProperty(string modelMode)
        {
            // Arrange
            string requestUri = $"{modelMode}/Windows(1)/CurrentShape";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Delete"), requestUri);

            // Act
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion
    }
}
