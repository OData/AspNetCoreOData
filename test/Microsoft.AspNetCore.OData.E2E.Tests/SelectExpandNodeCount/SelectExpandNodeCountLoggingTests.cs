//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountLoggingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SelectExpandNodeCount
{
    /// <summary>
    /// End-to-end tests that verify a request rejected for exceeding MaxSelectExpandNodeCount is recorded through
    /// the existing query validation error logging feature when it is turned on once for every action through the
    /// global <see cref="ODataOptions.EnableQueryValidationErrorLogging"/> value.
    /// </summary>
    public class SelectExpandNodeCountLoggingTests : WebApiTestBase<SelectExpandNodeCountLoggingTests>
    {
        private static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

        public SelectExpandNodeCountLoggingTests(WebApiTestFixture<SelectExpandNodeCountLoggingTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(LimitedProjectionItemsController));

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddProvider(LoggerProvider);
            });

            IEdmModel model = SelectExpandNodeCountEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryValidationErrorLogging = true; // Turn logging on once for every [EnableQuery] action.
                options.EnableQueryFeatures();
                options.AddRouteComponents("odata", model);
            });
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        [Fact]
        public async Task RequestExceedingMaxSelectExpandNodeCount_WritesDiagnostic()
        {
            // Arrange
            LoggerProvider.Clear();
            HttpClient client = CreateClient();

            // The endpoint allows 5 items; this request resolves to 6 $select items.
            string requestUri = "odata/LimitedProjectionItems?$select=Id,P1,P2,P3,P4,P5";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
            Assert.Equal(LogLevel.Warning, entry.Level); // the default QueryValidationErrorLogLevel
            Assert.Equal("$select=Id,P1,P2,P3,P4,P5", entry.GetFieldValue("QueryOptions"));
            Assert.NotNull(entry.Exception);
            Assert.Contains(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, 5, "MaxSelectExpandNodeCount"), entry.Exception.Message);
        }

        [Fact]
        public async Task RequestWithinMaxSelectExpandNodeCount_WritesNoDiagnostic()
        {
            // Arrange
            LoggerProvider.Clear();
            HttpClient client = CreateClient();

            // The endpoint allows 5 items; this request resolves to 5 $select items.
            string requestUri = "odata/LimitedProjectionItems?$select=Id,P1,P2,P3,P4";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(GetQueryValidationEntries());
        }

        private static IReadOnlyList<CapturedLogEntry> GetQueryValidationEntries()
        {
            return LoggerProvider.Entries
                .Where(entry => entry.Category == typeof(EnableQueryAttribute).FullName)
                .ToList();
        }
    }
}
