#if NET7_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Results.MinimalAPIResults
{
    public static class ODataResults
    {
        public static ODataResult ODResult<T>(this IResultExtensions resultExtensions, IQueryable<T> result)
        {
            return new ODataResult(result);
        }
    }

    public class ODataResult : IResult
    {
        private readonly HttpStatusCode _statusCode;

        public ODataResult(object? result, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
            Result = result;
        }

        public object? Result { get; set; }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var serializer = httpContext.RequestServices.GetRequiredService<ODataMinimalSerializer>();

            object? result = null;

            if (Result is ODataResult res)
            {
                result = res.Result;
            }
            else
            {
                result = Result;
            }

            await serializer.WriteAsync(httpContext, result);
        }
    }
}

#endif
