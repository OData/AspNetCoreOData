// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing
{
    public class AttributeRoutingTests : WebApiTestBase<AttributeRoutingTests>
    {
        public AttributeRoutingTests(WebApiTestFixture<AttributeRoutingTests> fixture)
           : base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(DogsController), typeof(CatsController), typeof(OwnersController));

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Dog>("Dogs").EntityType.Collection.Function("BestDog").Returns<string>();
            builder.EntitySet<Owner>("Owners").EntityType.Collection.Function("BestOwner").Returns<string>();
            var model1 = builder.GetEdmModel();

            builder = new ODataConventionModelBuilder();
            builder.EntitySet<Cat>("Cats").EntityType.Collection.Function("BestCat").Returns<string>();
            builder.EntitySet<Owner>("Owners").EntityType.Collection.Function("BestOwner").Returns<string>();
            var model2 = builder.GetEdmModel();

            services.AddControllers().AddOData(opt => opt.AddModel("dog", model1).AddModel("cat", model2));
        }

        [Theory]
        [InlineData("/dog/Dogs")]
        [InlineData("/cat/Cats")]
        [InlineData("/dog/Owners")]
        [InlineData("/cat/Owners")]
        public async Task CanCallTheRoutingWithoutAttributeRoutingDecorated(string requestUri)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();

            string payload = await response.Content.ReadAsStringAsync();
            Assert.NotNull(payload);
            using (JsonDocument document = JsonDocument.Parse(payload))
            {
                bool found = document.RootElement.TryGetProperty("value", out JsonElement value);
                Assert.True(found);
                Assert.Equal(5, value.EnumerateArray().Count());
            }
        }

        [Theory]
        [InlineData("/dog/Dogs/Default.BestDog", "Dog 1")]
        [InlineData("/dog/Owners/Default.BestOwner", "Owner 1")]
        [InlineData("/dog/Owners/BestOwner", "Owner 1")]
        [InlineData("/cat/Cats/Default.BestCat", "Cat 1")]
        [InlineData("/cat/Owners/Default.BestOwner", "Owner 1")]
        public async Task CanCallTheRoutingWithAttributeRoutingDecorated(string url, string expected)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(expected, payload);
        }
    }

    public class DogsController : ODataController
    {
        private static IList<Dog> dogs = Enumerable.Range(1, 5)
            .Select(e => { return new Dog() { Id = e, Name = "Dog " + e.ToString() }; })
            .ToList();

        // This method will have the following routing:
        // ~/dog/Dogs
        // ~/dog/Dogs/$count
        public IActionResult Get()
        {
            return Ok(dogs);
        }

        [HttpGet("~/dog/Dogs/Default.BestDog")]
        public IActionResult WhoIsTheBestDog()
        {
            return Ok(dogs.First().Name);
        }
    }

    [ODataRouting]
    public class CatsController : ControllerBase
    {
        private static IList<Cat> cats = Enumerable.Range(1, 5)
            .Select(e => { return new Cat() { Id = e, Name = "Cat " + e.ToString() }; })
            .ToList();

        // This method will have the following routing:
        // ~/cat/Cats
        // ~/cat/Cats/$count
        public IActionResult Get()
        {
            return Ok(cats);
        }

        [HttpGet("cat/Cats/Default.BestCat")]
        public IActionResult WhoIsTheBestCat()
        {
            return Ok(cats.First().Name);
        }
    }

    public class OwnersController : ODataController
    {
        private static IList<Owner> owners = Enumerable.Range(1, 5)
            .Select(e => { return new Owner() { Id = e, Name = "Owner " + e.ToString() }; })
            .ToList();

        // This method will have the following routing:
        // ~/dog/Owners
        // ~/cat/Owners
        // ~/dog/Owners/$count
        // ~/cat/Owners/$count
        public IActionResult Get()
        {
            return Ok(owners);
        }

        [HttpGet("dog/Owners/Default.BestOwner")]
        [HttpGet("dog/Owners/BestOwner")]
        [HttpGet("cat/Owners/Default.BestOwner")]
        public IActionResult WhoIsTheBestOwner()
        {
            return Ok(owners.First().Name);
        }
    }

    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}