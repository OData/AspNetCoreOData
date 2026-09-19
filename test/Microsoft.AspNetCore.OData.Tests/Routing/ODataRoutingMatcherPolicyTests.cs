//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingMatcherPolicyTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing;

public class ODataRoutingMatcherPolicyTests
{
    [Fact]
    public void AppliesToEndpoints_EndpointWithoutODataRoutingMetadata_ReturnsFalse()
    {
        // Arrange
        Endpoint[] endpoints = new[] { CreateEndpoint("/", null), };

        ODataRoutingMatcherPolicy policy = CreatePolicy();

        // Act
        bool result = policy.AppliesToEndpoints(endpoints);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AppliesToEndpoints_EndpointHasODataRoutingMetadata_ReturnsTrue()
    {
        // Arrange
        IODataRoutingMetadata routingMetadata = new ODataRoutingMetadata();
        Endpoint[] endpoints = new[]
        {
            CreateEndpoint("/", routingMetadata),
            CreateEndpoint("/", null),
        };

        ODataRoutingMatcherPolicy policy = CreatePolicy();

        // Act
        bool result = policy.AppliesToEndpoints(endpoints);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ApplyAsync_PrefersEndpointsWithODataRoutingMetadata()
    {
        // Arrange
        IEdmModel model = EdmCoreModel.Instance;
        IODataRoutingMetadata routingMetadata = new ODataRoutingMetadata("odata", model, new ODataPathTemplate());

        Endpoint[] endpoints = new[]
        {
            CreateEndpoint("/", routingMetadata, new HttpMethodMetadata(new[] { "get" })),
            CreateEndpoint("/", routingMetadata, new HttpMethodMetadata(new[] { "post" })),
            CreateEndpoint("/", routingMetadata, new HttpMethodMetadata(new[] { "delete" }))
        };

        CandidateSet candidateSet = CreateCandidateSet(endpoints);

        HttpContext httpContext = CreateHttpContext("POST");

        HttpMethodMatcherPolicy httpMethodPolicy = new HttpMethodMatcherPolicy();

        ODataRoutingMatcherPolicy policy = CreatePolicy();

        // Act
        await httpMethodPolicy.ApplyAsync(httpContext, candidateSet);
        await policy.ApplyAsync(httpContext, candidateSet);

        // Assert
        Assert.False(candidateSet.IsValidCandidate(0));
        Assert.True(candidateSet.IsValidCandidate(1));
        Assert.False(candidateSet.IsValidCandidate(2));
    }

    [Fact]
    public async Task ApplyAsync_UsesFirstRoutingMetadataThatTranslates()
    {
        // Arrange
        IEdmModel firstModel = new EdmModel();
        IEdmModel secondModel = new EdmModel();
        IEdmModel thirdModel = new EdmModel();
        ODataPathTemplate firstTemplate = new ODataPathTemplate();
        ODataPathTemplate secondTemplate = new ODataPathTemplate();
        ODataPathTemplate thirdTemplate = new ODataPathTemplate();
        IODataRoutingMetadata firstMetadata = new ODataRoutingMetadata("first", firstModel, firstTemplate);
        IODataRoutingMetadata secondMetadata = new ODataRoutingMetadata("second", secondModel, secondTemplate);
        IODataRoutingMetadata thirdMetadata = new ODataRoutingMetadata("third", thirdModel, thirdTemplate);
        ODataPath expectedPath = new ODataPath();

        var translator = new Mock<IODataTemplateTranslator>();
        translator
            .Setup(a => a.Translate(firstTemplate, It.IsAny<ODataTemplateTranslateContext>()))
            .Returns((ODataPath)null);
        translator
            .Setup(a => a.Translate(secondTemplate, It.IsAny<ODataTemplateTranslateContext>()))
            .Returns((ODataPathTemplate _, ODataTemplateTranslateContext context) =>
            {
                context.UpdatedValues["key"] = "updated";
                return expectedPath;
            });

        Endpoint[] endpoints = new[]
        {
            CreateEndpoint("/", firstMetadata, secondMetadata, thirdMetadata)
        };
        CandidateSet candidateSet = CreateCandidateSet(endpoints);
        HttpContext httpContext = CreateHttpContext("GET");
        ODataRoutingMatcherPolicy policy = CreatePolicy(translator.Object);

        // Act
        await policy.ApplyAsync(httpContext, candidateSet);

        // Assert
        Assert.True(candidateSet.IsValidCandidate(0));
        Assert.Equal("second", httpContext.ODataFeature().RoutePrefix);
        Assert.Same(secondModel, httpContext.ODataFeature().Model);
        Assert.Same(expectedPath, httpContext.ODataFeature().Path);
        Assert.Equal("updated", candidateSet[0].Values["key"]);
        translator.Verify(
            a => a.Translate(firstTemplate, It.Is<ODataTemplateTranslateContext>(c => c.Model == firstModel)),
            Times.Once);
        translator.Verify(
            a => a.Translate(secondTemplate, It.Is<ODataTemplateTranslateContext>(c => c.Model == secondModel)),
            Times.Once);
        translator.Verify(
            a => a.Translate(thirdTemplate, It.IsAny<ODataTemplateTranslateContext>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyAsync_InvalidatesCandidateWhenAllRoutingMetadataFailToTranslate()
    {
        // Arrange
        IODataRoutingMetadata firstMetadata =
            new ODataRoutingMetadata("first", new EdmModel(), new ODataPathTemplate());
        IODataRoutingMetadata secondMetadata =
            new ODataRoutingMetadata("second", new EdmModel(), new ODataPathTemplate());

        var translator = new Mock<IODataTemplateTranslator>();
        translator
            .Setup(a => a.Translate(It.IsAny<ODataPathTemplate>(), It.IsAny<ODataTemplateTranslateContext>()))
            .Returns((ODataPath)null);

        Endpoint[] endpoints = new[]
        {
            CreateEndpoint("/", firstMetadata, secondMetadata)
        };
        CandidateSet candidateSet = CreateCandidateSet(endpoints);
        HttpContext httpContext = CreateHttpContext("GET");
        ODataRoutingMatcherPolicy policy = CreatePolicy(translator.Object);

        // Act
        await policy.ApplyAsync(httpContext, candidateSet);

        // Assert
        Assert.False(candidateSet.IsValidCandidate(0));
        Assert.Null(httpContext.ODataFeature().Path);
        translator.Verify(
            a => a.Translate(It.IsAny<ODataPathTemplate>(), It.IsAny<ODataTemplateTranslateContext>()),
            Times.Exactly(2));
    }

    private static RouteEndpoint CreateEndpoint(string template, IODataRoutingMetadata odataMetadata, params object[] more)
    {
        var metadata = new List<object>();
        if (odataMetadata != null)
        {
            metadata.Add(odataMetadata);
        }

        if (more != null)
        {
            metadata.AddRange(more);
        }

        return new RouteEndpoint(
            (context) => Task.CompletedTask,
            RoutePatternFactory.Parse(template),
            0,
            new EndpointMetadataCollection(metadata),
            $"test: {template} - { odataMetadata?.Prefix ?? ""}");
    }

    private static CandidateSet CreateCandidateSet(Endpoint[] endpoints)
    {
        var values = new RouteValueDictionary[endpoints.Length];
        for (var i = 0; i < endpoints.Length; i++)
        {
            values[i] = new RouteValueDictionary();
        }

        CandidateSet candidateSet = new CandidateSet(endpoints, values, new int[endpoints.Length]);
        return candidateSet;
    }

    private static ODataRoutingMatcherPolicy CreatePolicy(IODataTemplateTranslator translator = null)
    {
        if (translator == null)
        {
            var translatorMock = new Mock<IODataTemplateTranslator>();
            translatorMock
                .Setup(a => a.Translate(It.IsAny<ODataPathTemplate>(), It.IsAny<ODataTemplateTranslateContext>()))
                .Returns(new ODataPath());
            translator = translatorMock.Object;
        }

        return new ODataRoutingMatcherPolicy(translator);
    }

    private static HttpContext CreateHttpContext(string httpMethod)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = httpMethod;
        return httpContext;
    }
}
