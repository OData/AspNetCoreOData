//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ApplyQueryValidation
{
    /// <summary>
    /// End-to-end tests asserting that property restrictions are enforced consistently across
    /// <c>$filter</c>, <c>$apply</c> (filter/groupby/aggregate/compute) and top-level <c>$compute</c>.
    /// A property marked as not filterable or configured as not selectable is expected to be rejected
    /// with <see cref="HttpStatusCode.BadRequest"/> regardless of which query option references it, while
    /// the equivalent request against an allowed property is expected to succeed with
    /// <see cref="HttpStatusCode.OK"/>.
    /// </summary>
    public class ApplyQueryValidationTests : WebApiTestBase<ApplyQueryValidationTests>
    {
        public ApplyQueryValidationTests(WebApiTestFixture<ApplyQueryValidationTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = ApplyQueryValidationEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(ApplyValidationItemsController), typeof(RestrictedLimitItemsController), typeof(DepthValidationItemsController), typeof(DepthTwoValidationItemsController), typeof(DepthValidationCollectionShapesController));

            services.AddControllers().AddOData(options =>
                options.EnableQueryFeatures().AddRouteComponents("odata", edmModel));
        }

        // Each case references a restricted property (not filterable, or not selectable) through a
        // query option. The expected/consistent behavior is a 400 response, matching the equivalent
        // $filter/$select. The $filter case is the regression anchor: it already returns 400 today.
        [Theory]
        [InlineData("$apply=filter(RestrictedName eq 'R1')")]
        [InlineData("$apply=groupby((NotSelectableName))")]
        [InlineData("$apply=aggregate(RestrictedAmount with max as MaxRestricted)")]
        [InlineData("$apply=aggregate(Related(RestrictedAmount with sum as SumRestricted))")]
        [InlineData("$apply=compute(RestrictedName eq 'R1' as Flag)")]
        [InlineData("$compute=RestrictedName eq 'R1' as Flag")]
        [InlineData("$filter=RestrictedName eq 'R1'")]
        public async Task QueryRestrictedProperty_ThroughApplyFilterComputeOrSelect_ReturnsBadRequest(string query)
        {
            // Arrange
            string queryUrl = $"odata/ApplyValidationItems?{query}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Each case references an allowed property through the same query options. These already succeed
        // today and must keep succeeding, proving the restriction is selective rather than all-or-nothing.
        [Theory]
        [InlineData("$apply=filter(Name eq 'Alpha')")]
        [InlineData("$apply=groupby((Name))")]
        [InlineData("$apply=aggregate(Amount with sum as TotalAmount)")]
        [InlineData("$apply=aggregate(Related(Amount with sum as SumAmount))")]
        [InlineData("$apply=aggregate($count as Total)")]
        [InlineData("$apply=compute(Amount mul 2 as DoubleAmount)")]
        [InlineData("$compute=Amount mul 2 as DoubleAmount")]
        // A filter() that follows groupby/aggregate references the aggregation alias (a computed value,
        // not a model property). The extended validation must not treat that alias as a restricted
        // property and falsely reject an otherwise legitimate query.
        [InlineData("$apply=groupby((Name),aggregate(Amount with sum as Total))/filter(Total gt 0)")]
        public async Task QueryAllowedProperty_ThroughApplyFilterOrCompute_ReturnsOk(string query)
        {
            // Arrange
            string queryUrl = $"odata/ApplyValidationItems?{query}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // The RestrictedLimitItems endpoint disallows the 'length' function and the 'mul' arithmetic
        // operator. Each case uses an allowed property but a disallowed function/operator inside
        // $apply (groupby/aggregate/compute) or top-level $compute, so the only possible cause of the
        // 400 is the operator/function allow-list now being enforced for those transformations.
        [Theory]
        [InlineData("$compute=length(Name) as L")]
        [InlineData("$apply=compute(length(Name) as L)")]
        [InlineData("$compute=Amount mul 2 as D")]
        [InlineData("$apply=compute(Amount mul 2 as D)")]
        [InlineData("$apply=aggregate(Amount mul 2 with sum as D)")]
        public async Task DisallowedFunctionOrOperator_ThroughApplyOrCompute_ReturnsBadRequest(string query)
        {
            // Arrange
            string queryUrl = $"odata/RestrictedLimitItems?{query}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // The same endpoint still accepts functions/operators that remain allowed, proving the enforcement
        // is selective (allow-list driven) rather than blocking every function/operator in $apply/$compute.
        [Theory]
        [InlineData("$compute=Name eq 'Alpha' as Flag")]
        [InlineData("$apply=compute(Name eq 'Alpha' as Flag)")]
        [InlineData("$apply=aggregate(Amount with sum as TotalAmount)")]
        [InlineData("$apply=groupby((Name))")]
        public async Task AllowedFunctionOrOperator_ThroughApplyOrCompute_ReturnsOk(string query)
        {
            // Arrange
            string queryUrl = $"odata/RestrictedLimitItems?{query}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]
        [InlineData(32, HttpStatusCode.OK)]
        [InlineData(33, HttpStatusCode.OK)]
        [InlineData(34, HttpStatusCode.BadRequest)]
        [InlineData(51, HttpStatusCode.BadRequest)]
        [InlineData(101, HttpStatusCode.BadRequest)]
        public async Task OrderByDepthValidation_PreservesParameterAliasBindingLimit(int references, HttpStatusCode expectedStatus)
        {
            var terms = Enumerable.Repeat("@value", references).ToList();
            // Keep the concat tree balanced so larger alias counts do not create a long chain of nested functions.
            while (terms.Count > 1)
            {
                var next = new List<string>();
                for (int i = 0; i < terms.Count; i += 2)
                {
                    next.Add(i + 1 == terms.Count ? terms[i] : $"concat({terms[i]},{terms[i + 1]})");
                }
                terms = next;
            }
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?$orderby={terms[0]},Id desc&@value='x'&$select=Id");

            Assert.Equal(expectedStatus, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            if (expectedStatus == HttpStatusCode.OK)
            {
                Assert.Equal(new[] { 5, 4, 3 }, JObject.Parse(body)["value"].Select(item => (int)item["Id"]));
            }
            else
            {
                Assert.Contains("The parameter alias expansion count exceeds the maximum allowed value of '100'.", body);
            }
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]
        [InlineData(50, HttpStatusCode.OK)]
        [InlineData(51, HttpStatusCode.OK)]
        [InlineData(99, HttpStatusCode.OK)]
        [InlineData(100, HttpStatusCode.OK)]
        [InlineData(101, HttpStatusCode.BadRequest)]
        public async Task FilterInDepthValidation_PreservesParameterAliasBindingLimit(int references, HttpStatusCode expectedStatus)
        {
            var terms = Enumerable.Repeat("@value", references).ToList();
            // Keep the concat tree balanced so larger alias counts do not create a long chain of nested functions.
            while (terms.Count > 1)
            {
                var next = new List<string>();
                for (int i = 0; i < terms.Count; i += 2)
                {
                    next.Add(i + 1 == terms.Count ? terms[i] : $"concat({terms[i]},{terms[i + 1]})");
                }
                terms = next;
            }
            using HttpClient client = CreateClient();

            string expectedValue = new string('x', references);
            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?$filter={terms[0]} in ('{expectedValue}')&@value='x'&$select=Id");

            Assert.Equal(expectedStatus, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            if (expectedStatus == HttpStatusCode.OK)
            {
                Assert.Equal(new[] { 3, 4, 5 }, JObject.Parse(body)["value"].Select(item => (int)item["Id"]));
            }
            else
            {
                Assert.Contains("The parameter alias expansion count exceeds the maximum allowed value of '100'.", body);
            }
        }

        [Theory]
        [InlineData("DepthValidationItems", "@outer in (true)", HttpStatusCode.BadRequest)]
        [InlineData("DepthTwoValidationItems", "@outer in (true)", HttpStatusCode.OK)]
        [InlineData("DepthValidationItems", "Related/any(a: @outer in (true))", HttpStatusCode.BadRequest)]
        [InlineData("DepthTwoValidationItems", "Related/any(a: @outer in (true))", HttpStatusCode.OK)]
        public async Task FilterInDepthValidation_ResolvesLambdaAliases(string endpoint, string filter, HttpStatusCode expectedStatus)
        {
            using HttpClient client = CreateClient();
            string expression = filter == "@outer in (true)" ? "Related/any(a:a/Related/any(b:b/Id eq -1))" : "Related/any(b:b/Id eq -1)";

            using HttpResponseMessage response = await client.GetAsync($"odata/{endpoint}?$filter={filter}&@outer={expression}&$select=Id");

            Assert.Equal(expectedStatus, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            if (expectedStatus == HttpStatusCode.BadRequest)
            {
                Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", body);
            }
            else
            {
                Assert.Empty(JObject.Parse(body)["value"]);
            }
        }

        [Theory]
        [InlineData("DepthValidationItems", HttpStatusCode.BadRequest)]
        [InlineData("DepthTwoValidationItems", HttpStatusCode.OK)]
        public async Task OrderByDepthValidation_ResolvesLambdaAliases(string endpoint, HttpStatusCode expectedStatus)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/{endpoint}?$orderby=cast(@outer,Edm.Boolean)&@outer=@inner&@inner=Related/any(a:a/Related/all(b:b/Id eq -1))&$select=Id");

            Assert.Equal(expectedStatus, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            if (expectedStatus == HttpStatusCode.BadRequest)
            {
                Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", body);
            }
            else
            {
                Assert.Equal(3, JObject.Parse(body)["value"].Count());
            }
        }

        [Theory]
        [InlineData("$orderby=Related/any(a:a/Related/any(b:b/Id eq -1))")]
        [InlineData("$orderby=Related/any(a:a/Related/all(b:b/Id eq -1))")]
        [InlineData("$orderby=Related/all(a:a/Related/any(b:b/Id eq -1))")]
        [InlineData("$orderby=Related/all(a:a/Related/all(b:b/Id eq -1))")]
        [InlineData("$orderby=not Related/any(a:a/Related/any(b:b/Id eq -1))")]
        [InlineData("$orderby=Related/any(a:a/Related/any(b:b/Id eq -1)) eq true")]
        [InlineData("$orderby=true eq Related/any(a:a/Related/any(b:b/Id eq -1))")]
        [InlineData("$orderby=Related/any(a:a/Related/any(b:b/Id eq -1)) in (true)")]
        [InlineData("$orderby=cast(Related/any(a:a/Related/any(b:b/Id eq -1)),Edm.Boolean)")]
        [InlineData("$orderby=Id,Related/any(a:a/Related/any(b:b/Id eq -1))")]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Id eq -1)) in (true)")]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Id eq -1) in (true))")]
        [InlineData("$apply=filter(Related/any(a:a/Related/any(b:b/Id eq -1)) in (true))")]
        [InlineData("$expand=Related($orderby=Id,Related/any(a:a/Related/any(b:b/Id eq -1));$select=Id)")]
        [InlineData("$expand=Related($compute=Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag;$select=Id,Flag)")]
        [InlineData("$expand=Related($compute=1 as First,Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag;$select=Id,First,Flag)")]
        [InlineData("$expand=Related($expand=Related($compute=Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag;$select=Id,Flag);$select=Id)")]
        [InlineData("$expand=Related/$ref($filter=Related/any(a:a/Related/any(b:b/Id eq -1)))")]
        [InlineData("$expand=Related/$ref($orderby=Related/any(a:a/Related/any(b:b/Id eq -1)))")]
        [InlineData("$expand=Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1)))")]
        [InlineData("$compute=Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))) as Total")]
        [InlineData("$compute=1 as First,Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))) as Total")]
        [InlineData("$compute=Related/any(a:a/Related/$count($filter=Related/any(b:b/Id eq -1)) gt 0) as Flag")]
        [InlineData("$apply=compute(cast(Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))),Edm.Int64) as Total)")]
        [InlineData("$apply=aggregate(cast(Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))),Edm.Int64) with sum as Total)")]
        public async Task AnyAllExpressionDepth_DefaultLimit_RejectsNestedExpressions(string query)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?{query}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", body);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task AnyAllExpressionDepth_DefaultLimit_RejectsNestedExpressionsForEveryCollectionShape(int id)
        {
            using HttpClient client = CreateClient();
            string query = $"$filter=Id eq {id}&$orderby=Related/any(a:a/Related/any(b:b/Id eq -1))&$select=Id";

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationCollectionShapes?{query}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", body);
        }

        [Theory]
        [InlineData("$orderby=Related/any(a:a/Related/any(b:b/Id eq -1))", 3)]
        [InlineData("$orderby=Related/any(a:a/Related/all(b:b/Id eq -1))", 3)]
        [InlineData("$orderby=Related/all(a:a/Related/any(b:b/Id eq -1))", 3)]
        [InlineData("$orderby=Related/all(a:a/Related/all(b:b/Id eq -1))", 3)]
        [InlineData("$orderby=not Related/any(a:a/Related/any(b:b/Id eq -1))", 3)]
        [InlineData("$orderby=true eq Related/any(a:a/Related/any(b:b/Id eq -1))", 3)]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Id eq -1)) in (true)", 0)]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Id eq -1) in (true))", 0)]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Id eq -1))", 0)]
        [InlineData("$compute=Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag", 3)]
        [InlineData("$apply=filter(Related/any(a:a/Related/any(b:b/Id eq -1)) in (true))", 0)]
        [InlineData("$expand=Related($orderby=Id,Related/any(a:a/Related/any(b:b/Id eq -1));$select=Id)", 3)]
        [InlineData("$expand=Related($compute=Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag;$select=Id,Flag)", 3)]
        [InlineData("$expand=Related/$ref($filter=Related/any(a:a/Related/any(b:b/Id eq -1)))", 3)]
        [InlineData("$expand=Related/$ref($orderby=Related/any(a:a/Related/any(b:b/Id eq -1)))", 3)]
        [InlineData("$expand=Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1)))", 3)]
        [InlineData("$compute=Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))) as Total", 3)]
        [InlineData("$compute=Related/any(a:a/Related/$count($filter=Related/any(b:b/Id eq -1)) gt 0) as Flag", 3)]
        [InlineData("$apply=compute(cast(Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))),Edm.Int64) as Total)", 3)]
        [InlineData("$apply=aggregate(cast(Related/$count($filter=Related/any(a:a/Related/any(b:b/Id eq -1))),Edm.Int64) with sum as Total)", 1)]
        public async Task AnyAllExpressionDepth_ConfiguredLimit_AllowsDepthTwo(string query, int expectedCount)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthTwoValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            Assert.Equal(expectedCount, values.Count);
        }

        [Theory]
        [InlineData("$orderby=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)))")]
        [InlineData("$orderby=Related/all(a:a/Related/any(b:b/Related/all(c:c/Id eq -1)))")]
        [InlineData("$orderby=true eq Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)))")]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1))) in (true)")]
        [InlineData("$filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)) in (true))")]
        [InlineData("$expand=Related($orderby=Id,Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)));$select=Id)")]
        [InlineData("$expand=Related($compute=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1))) as Flag;$select=Id,Flag)")]
        [InlineData("$expand=Related/$ref($filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1))))")]
        [InlineData("$expand=Related/$ref($orderby=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1))))")]
        [InlineData("$expand=Related/$count($filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1))))")]
        [InlineData("$compute=Related/$count($filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)))) as Total")]
        [InlineData("$compute=Related/any(a:a/Related/$count($filter=Related/any(b:b/Related/any(c:c/Id eq -1))) gt 0) as Flag")]
        [InlineData("$apply=compute(cast(Related/$count($filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)))),Edm.Int64) as Total)")]
        [InlineData("$apply=aggregate(cast(Related/$count($filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)))),Edm.Int64) with sum as Total)")]
        public async Task AnyAllExpressionDepth_ConfiguredLimit_RejectsDepthThree(string query)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthTwoValidationItems?{query}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("The Any/All nesting limit of '2' has been exceeded.", body);
        }

        [Theory]
        [InlineData("DepthValidationItems", 1, "$filter=Related/any(a:a/Related/any(b:b/Id eq -1))")]
        [InlineData("DepthValidationItems", 1, "$filter=Related/all(a:a/Related/all(b:b/Id eq -1))")]
        [InlineData("DepthValidationItems", 1, "$compute=Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag")]
        [InlineData("DepthValidationItems", 1, "$apply=filter(Related/any(a:a/Related/any(b:b/Id eq -1)))")]
        [InlineData("DepthValidationItems", 1, "$apply=compute(Related/any(a:a/Related/any(b:b/Id eq -1)) as Flag)")]
        [InlineData("DepthTwoValidationItems", 2, "$filter=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1)))")]
        [InlineData("DepthTwoValidationItems", 2, "$compute=Related/any(a:a/Related/any(b:b/Related/any(c:c/Id eq -1))) as Flag")]
        public async Task AnyAllExpressionDepth_ExistingValidation_RejectsExpressionsAboveLimit(string entitySet, int limit, string query)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/{entitySet}?{query}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains($"The Any/All nesting limit of '{limit}' has been exceeded.", body);
        }

        [Theory]
        [InlineData("$select=Id", new[] { 3, 4, 5 })]
        [InlineData("$orderby=Id desc&$select=Id", new[] { 5, 4, 3 })]
        [InlineData("$orderby=Related/any(a:a/Id eq 2),Id&$select=Id", new[] { 5, 3, 4 })]
        [InlineData("$orderby=Related/all(a:a/Id gt 0),Id&$select=Id", new[] { 3, 4, 5 })]
        [InlineData("$orderby=Related/any(),Id&$select=Id", new[] { 3, 4, 5 })]
        [InlineData("$orderby=Related/any(a:a/Id eq 2),Related/all(a:a/Id gt 0),Id&$select=Id", new[] { 5, 3, 4 })]
        [InlineData("$filter=Related/any(a:a/Id eq 2) in (true)&$orderby=Id&$select=Id", new[] { 3, 4 })]
        [InlineData("$filter=Related/any(a:a/Id eq 2) and Related/all(a:a/Id gt 0)&$orderby=Id&$select=Id", new[] { 3, 4 })]
        [InlineData("$filter=Id in (3,5)&$select=Id", new[] { 3, 5 })]
        public async Task AnyAllExpressionDepth_DefaultLimit_AllowsIndependentAndScalarExpressions(string query, int[] expectedIds)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            Assert.Equal(expectedIds, values.Select(item => (int)item["Id"]));
        }

        [Fact]
        public async Task AnyAllExpressionDepth_DefaultLimit_PreservesIndependentComputedValues()
        {
            using HttpClient client = CreateClient();
            string query = "$compute=Related/any(a:a/Id eq 2) as HasTwo,Related/all(a:a/Id gt 0) as AllPositive,Amount mul 2 as DoubleAmount&$select=Id,HasTwo,AllPositive,DoubleAmount";

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            Assert.Equal(new[] { 3, 4, 5 }, values.Select(item => (int)item["Id"]));
            Assert.Equal(new[] { true, true, false }, values.Select(item => (bool)item["HasTwo"]));
            Assert.All(values, item => Assert.True((bool)item["AllPositive"]));
            Assert.Equal(new[] { 60, 80, 100 }, values.Select(item => (int)item["DoubleAmount"]));
        }

        [Theory]
        [InlineData("$compute=Related/$count($filter=Id eq 2) as Total&$select=Id,Total")]
        [InlineData("$apply=compute(cast(Related/$count($filter=Id eq 2),Edm.Int64) as Total)&$select=Id,Total")]
        public async Task AnyAllExpressionDepth_DefaultLimit_PreservesCountFilterValues(string query)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            Assert.Equal(new[] { 3, 4, 5 }, values.Select(item => (int)item["Id"]));
            Assert.Equal(new[] { 1, 1, 0 }, values.Select(item => (int)item["Total"]));
        }

        [Fact]
        public async Task AnyAllExpressionDepth_DefaultLimit_PreservesAggregateValue()
        {
            using HttpClient client = CreateClient();
            string query = "$apply=aggregate(cast(Related/$count($filter=Id eq 2),Edm.Int64) with sum as Total)";

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            Assert.Equal(2, (int)Assert.Single(values)["Total"]);
        }

        [Fact]
        public async Task AnyAllExpressionDepth_DefaultLimit_PreservesIndependentTopLevelAndNestedComputedValues()
        {
            using HttpClient client = CreateClient();
            string query = "$filter=Id eq 5&$compute=Related/any(a:a/Id eq 2) as HasTwo&$select=Id,HasTwo&$expand=Related($compute=Related/any(a:a/Id eq 2) as HasTwo;$select=Id,HasTwo;$orderby=Id)";

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            JToken item = Assert.Single(values);
            Assert.Equal(5, (int)item["Id"]);
            Assert.False((bool)item["HasTwo"]);
            JArray related = Assert.IsType<JArray>(item["Related"]);
            Assert.Equal(new[] { 4, 5 }, related.Select(child => (int)child["Id"]));
            Assert.Equal(new[] { true, false }, related.Select(child => (bool)child["HasTwo"]));
        }

        [Theory]
        [InlineData("$expand=Related/$ref($filter=Related/any(a:a/Id eq 2))", new[] { 4 })]
        [InlineData("$expand=Related/$ref($orderby=Related/any(a:a/Id eq 2))", new[] { 5, 4 })]
        public async Task AnyAllExpressionDepth_DefaultLimit_PreservesReferenceFilterAndOrder(string query, int[] expectedIds)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationItems?$filter=Id eq 5&$select=Id&{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            JArray related = Assert.IsType<JArray>(Assert.Single(values)["Related"]);
            Assert.Equal(expectedIds.Length, related.Count);
            for (int i = 0; i < expectedIds.Length; i++)
            {
                Assert.EndsWith($"({expectedIds[i]})", (string)related[i]["@odata.id"]);
            }
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        [InlineData(4, true)]
        [InlineData(5, true)]
        public async Task AnyAllExpressionDepth_DefaultLimit_PreservesCollectionShapeValues(int id, bool? expected)
        {
            using HttpClient client = CreateClient();
            string query = $"$filter=Id eq {id}&$compute=Related/any() as HasRelated&$select=Id,HasRelated";

            using HttpResponseMessage response = await client.GetAsync($"odata/DepthValidationCollectionShapes?{query}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject body = await response.Content.ReadAsObject<JObject>();
            JArray values = Assert.IsType<JArray>(body["value"]);
            JToken item = Assert.Single(values);
            Assert.Equal(id, (int)item["Id"]);
            Assert.Equal(expected, item["HasRelated"].ToObject<bool?>());
        }

        [Theory]
        [InlineData("DepthValidationItems", "$expand=Related($orderby=Related/any(a:a/Id eq 2))", "The method or operation is not implemented.")]
        [InlineData("DepthTwoValidationItems", "$expand=Related($orderby=Related/any(a:a/Id eq 2))", "The method or operation is not implemented.")]
        [InlineData("DepthValidationItems", "$apply=compute(Related/any(a:a/Id eq 2) as Flag)", "Binding OData QueryNode of kind 'Any' is not supported by 'AggregationBinder'.")]
        [InlineData("DepthTwoValidationItems", "$apply=compute(Related/any(a:a/Id eq 2) as Flag)", "Binding OData QueryNode of kind 'Any' is not supported by 'AggregationBinder'.")]
        [InlineData("DepthValidationItems", "$apply=compute(Related/$count as Total)", "Binding OData QueryNode of kind 'Count' is not supported by 'AggregationBinder'.")]
        [InlineData("DepthTwoValidationItems", "$apply=compute(Related/$count as Total)", "Binding OData QueryNode of kind 'Count' is not supported by 'AggregationBinder'.")]
        [InlineData("DepthValidationItems", "$expand=Related/$ref($compute=Related/any(a:a/Id eq 2) as Flag;$orderby=Flag)", "must be an open type.")]
        [InlineData("DepthTwoValidationItems", "$expand=Related/$ref($compute=Related/any(a:a/Id eq 2) as Flag;$orderby=Flag)", "must be an open type.")]
        public async Task AnyAllExpressionDepth_WithinLimit_PreservesUnsupportedQueryErrors(string entitySet, string query, string expectedMessage)
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response = await client.GetAsync($"odata/{entitySet}?{query}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedMessage, body);
        }

        [Fact]
        public async Task AnyAllExpressionDepth_DefaultLimit_DoesNotRetainDepthBetweenRequests()
        {
            using HttpClient client = CreateClient();
            string nestedQuery = "odata/DepthValidationItems?$orderby=Related/any(a:a/Related/any(b:b/Id eq -1))&$select=Id";

            using HttpResponseMessage first = await client.GetAsync(nestedQuery);
            using HttpResponseMessage withinLimit = await client.GetAsync("odata/DepthValidationItems?$orderby=Related/any(a:a/Id eq 2),Id&$select=Id");
            using HttpResponseMessage removed = await client.GetAsync("odata/DepthValidationItems?$orderby=Id&$select=Id");
            using HttpResponseMessage readded = await client.GetAsync(nestedQuery);

            Assert.Equal(HttpStatusCode.OK, withinLimit.StatusCode);
            JObject withinLimitBody = await withinLimit.Content.ReadAsObject<JObject>();
            Assert.Equal(new[] { 5, 3, 4 }, withinLimitBody["value"].Select(item => (int)item["Id"]));
            Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, first.StatusCode);
            Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", await first.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.BadRequest, readded.StatusCode);
            Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", await readded.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task AnyAllExpressionDepth_ConfiguredLimit_DoesNotChangeOtherEndpoints()
        {
            using HttpClient client = CreateClient();
            string query = "$orderby=Related/any(a:a/Related/any(b:b/Id eq -1))&$select=Id";

            using HttpResponseMessage configured = await client.GetAsync($"odata/DepthTwoValidationItems?{query}");
            using HttpResponseMessage defaultLimit = await client.GetAsync($"odata/DepthValidationItems?{query}");
            using HttpResponseMessage configuredAgain = await client.GetAsync($"odata/DepthTwoValidationItems?{query}");

            Assert.Equal(HttpStatusCode.OK, configured.StatusCode);
            Assert.Equal(HttpStatusCode.OK, configuredAgain.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, defaultLimit.StatusCode);
            Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", await defaultLimit.Content.ReadAsStringAsync());
        }
    }
}
