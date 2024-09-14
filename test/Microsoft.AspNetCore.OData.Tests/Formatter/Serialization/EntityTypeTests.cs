//-----------------------------------------------------------------------------
// <copyright file="EntityTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization;

public class EntityTypeTests
{
    private IEdmModel _model = GetSampleModel();

    [Fact]
    public async Task EntityTypeSerializesAsODataEntry()
    {
        // Arrange
        const string routeName = "OData";
        IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
        ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

        var request = RequestFactory.Create("Get", "http://localhost/property", opt => opt.AddRouteComponents(routeName, _model));
        request.ODataFeature().Model = _model;
        request.ODataFeature().Path = path;
        request.ODataFeature().RoutePrefix = routeName;

        var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
        var formatter = ODataFormatterHelpers.GetOutputFormatter(payload);
        Employee employee = new Employee
        {
            EmployeeID = 8,
            Birthday = new System.DateTimeOffset(2020, 9, 10, 1, 2, 3, System.TimeSpan.Zero),
            EmployeeName = "Ssa",
            HomeAddress = null
        };
        var content = ODataFormatterHelpers.GetContent(employee, formatter, ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

        // Act & Assert
        string actual = await ODataFormatterHelpers.GetContentResult(content, request);

        Assert.Equal("{\"@odata.context\":\"http://localhost/OData/$metadata#employees/$entity\"," +
            "\"EmployeeID\":8," +
            "\"EmployeeName\":\"Ssa\"," +
            "\"BaseSalary\":0," +
            "\"Birthday\":\"2020-09-10T01:02:03Z\"," +
            "\"WorkCompanyId\":0," +
            "\"HomeAddress\":null" +
            "}", actual);
    }

    private static IEdmModel GetSampleModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Employee>("employees");
        builder.EntitySet<WorkItem>("workitems");
        return builder.GetEdmModel();
    }
}
