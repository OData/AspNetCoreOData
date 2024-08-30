//-----------------------------------------------------------------------------
// <copyright file="ODataMvcOptionsSetupTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests;

public class ODataMvcOptionsSetupTests
{
    [Fact]
    public void Configure_ThrowsArgumentNull_Options()
    {
        // Arrange
        ODataMvcOptionsSetup setup = new ODataMvcOptionsSetup();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => setup.Configure(null), "options");
    }

    [Fact]
    public void ODataMvcOptionsSetup_DoesNotSetup_ODataFormatters()
    {
        // Arrange & Act
        MvcOptions options = GetMvcOptions(false);

        // Assert
        Assert.DoesNotContain(options.InputFormatters, e => e is ODataInputFormatter);
        Assert.DoesNotContain(options.OutputFormatters, e => e is ODataOutputFormatter);
    }

    [Fact]
    public void ODataMvcOptionsSetup_Setup_ODataFormatters()
    {
        // Arrange & Act
        MvcOptions options = GetMvcOptions(true);

        // Assert
        Assert.Collection(options.InputFormatters.Take(3),
            e => Assert.IsType<ODataInputFormatter>(e),
            e => Assert.IsType<ODataInputFormatter>(e),
            e => Assert.IsType<ODataInputFormatter>(e));

        Assert.Collection(options.OutputFormatters.Take(3),
            e => Assert.IsType<ODataOutputFormatter>(e),
            e => Assert.IsType<ODataOutputFormatter>(e),
            e => Assert.IsType<ODataOutputFormatter>(e));
    }

    private static MvcOptions GetMvcOptions(bool withOData)
    {
        IServiceCollection services = new ServiceCollection();

        services.AddControllers();

        services.AddTransient<ILoggerFactory, LoggerFactory>();

        if (withOData)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ODataMvcOptionsSetup>());
        }

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;
    }
}
