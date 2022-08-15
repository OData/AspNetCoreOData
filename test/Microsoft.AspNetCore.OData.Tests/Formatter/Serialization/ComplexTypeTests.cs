//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Formatter.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        [Fact]
        public async Task ComplexTypeSerializesAsOData()
        {
            // Arrange
            string routeName = "OData";
            IEdmModel model = GetSampleModel();
            var request = RequestFactory.Create("Get", "http://localhost/property", opt => opt.AddRouteComponents(routeName, model));
            var addressComplexType = model.SchemaElements.OfType<IEdmComplexType>().Single(d => d.Name.Equals("Address"));
            request.ODataFeature().Path = new ODataPath(new ValueSegment(addressComplexType));
            request.ODataFeature().Model = model;
            request.ODataFeature().RoutePrefix = routeName;

            var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
            var formatter = ODataFormatterHelpers.GetOutputFormatter(payload);

            Address address = new Address
            {
                Street = "abc",
                City = "efg",
                State = "opq",
                ZipCode = "98029",
                CountryOrRegion = "Mars"
            };

            var content = ODataFormatterHelpers.GetContent(address, formatter, ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            string actual = await ODataFormatterHelpers.GetContentResult(content, request);

            Assert.Equal("{\"@odata.context\":\"http://localhost/OData/$metadata#Microsoft.AspNetCore.OData.Tests.Formatter.Models.Address\"," +
                "\"Street\":\"abc\"," +
                "\"City\":\"efg\"," +
                "\"State\":\"opq\"," +
                "\"ZipCode\":\"98029\"," +
                "\"CountryOrRegion\":\"Mars\"}", actual);
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Address>();
            return builder.GetEdmModel();
        }
    }
}
