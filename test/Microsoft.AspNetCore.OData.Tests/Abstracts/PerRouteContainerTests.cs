// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    //public class PerRouteContainerTests
    //{
    //    [Fact]
    //    public void Ctor_BuildServiceContainer_WithOneModel()
    //    {
    //        // Arrange
    //        IEdmModel model = EdmCoreModel.Instance;
    //        var options = new ODataOptions();
    //        options.AddModel("odata", model);

    //        // Act
    //        PerRouteContainer container = CreateContainer(options);

    //        // Assert
    //        KeyValuePair<string, IServiceProvider> service = Assert.Single(container.Services);
    //        Assert.Equal("odata", service.Key);
    //        Assert.NotNull(service.Value);

    //        IEdmModel actualModel =service.Value.GetService<IEdmModel>();
    //        Assert.Same(model, actualModel);
    //    }

    //    [Fact]
    //    public void Ctor_BuildServiceContainer_WithTwoModels()
    //    {
    //        // Arrange
    //        IEdmModel model = new EdmModel();
    //        var options = new ODataOptions();
    //        options.AddModel("odata", EdmCoreModel.Instance);
    //        options.AddModel("my{data}", model);

    //        // Act
    //        PerRouteContainer container = CreateContainer(options);

    //        // Assert
    //        Assert.Equal(2, container.Services.Count);
    //        IServiceProvider sp1 = container.GetServiceProvider("odata");
    //        Assert.NotNull(sp1);
    //        IEdmModel actualModel = sp1.GetService<IEdmModel>();
    //        Assert.Same(EdmCoreModel.Instance, actualModel);

    //        IServiceProvider sp2 = container.GetServiceProvider("my{data}");
    //        Assert.NotNull(sp2);
    //        actualModel = sp2.GetService<IEdmModel>();
    //        Assert.Same(model, actualModel);

    //        Assert.Null(container.GetServiceProvider("any"));
    //    }

    //    private static PerRouteContainer CreateContainer(ODataOptions options)
    //    {
    //        IOptions<ODataOptions> odataOptions = Options.Create(options);
    //        return new PerRouteContainer(odataOptions);
    //    }
    //}
}
