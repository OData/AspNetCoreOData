using Microsoft.AspNetCore.Http;
using ODataQueryBuilder;
using ODataQueryBuilder.Extensions;
using ODataQueryBuilder.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.OData.AzureFunctions
{
    internal static class ODataBindingHelper
    {
        internal static ODataQueryOptions BuildODataQueryOptions(
            HttpRequest httpRequest,
            IEdmModel model,
            Type elementClrType,
            string routePrefix)
        {
            ConfigureOData(httpRequest, model, routePrefix);
            ODataQueryContext queryContext = new ODataQueryContext(model, elementClrType, httpRequest.ODataFeature().Path);

            Type queryOptionsType = typeof(ODataQueryOptions<>).MakeGenericType(elementClrType);

            return (ODataQueryOptions)Activator.CreateInstance(queryOptionsType, queryContext, httpRequest);
        }

        private static void ConfigureOData(HttpRequest httpRequest, IEdmModel model, string routePrefix)
        {
            ODataOptions options = new ODataOptions();

            options.AddRouteComponents(routePrefix, model);
            httpRequest.ODataFeature().Services = options.GetRouteServices(routePrefix);

            // Path
            // NOTE: Path should be initialized for each request
            SetODataPath(httpRequest, model, routePrefix);

            // Model
            // Required
            SetEdmModel(httpRequest, model);
        }

        private static void SetODataPath(HttpRequest httpRequest, IEdmModel model, string routePrefix)
        {
            httpRequest.ODataFeature().RoutePrefix = routePrefix;

            Uri serviceRoot = new Uri($"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/{routePrefix}/");
            string odataPath = httpRequest.Path.Value.Substring($"/{routePrefix}/".Length);
            ODataUriParser parser = new ODataUriParser(
                model,
                serviceRoot,
                new Uri(odataPath, UriKind.Relative),
                httpRequest.ODataFeature().Services);
            ODataPath path = parser.ParsePath();
            httpRequest.ODataFeature().Path = path;
        }

        private static void SetEdmModel(HttpRequest httpRequest, IEdmModel model)
        {
            httpRequest.ODataFeature().Model = model;
        }
    }
}
