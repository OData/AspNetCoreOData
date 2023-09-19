#if NET7_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Results.MinimalAPIResults
{
    public static class ODataResults
    {
        public static ODataResult ODataQuery<T>(this IResultExtensions resultExtensions, IQueryable<T> result)
        {
            return new ODataResult(result);
        }

        public static ODataResult ODataQuery<T>(this IResultExtensions resultExtensions, T result)
        {
            return new ODataResult(result);
        }

        public static ODataResult ODataEntity<T>(this IResultExtensions resultExtensions, T result)
        {
            return new ODataResult(result);
        }

        public static ODataResult ODataEntity(this IResultExtensions resultExtensions, object result)
        {
            return new ODataResult(result);
        }

        public static ODataResult OData(this IResultExtensions resultExtensions, object result, HttpStatusCode statusCode)
        {
            return new ODataResult(result, statusCode);
        }

        public static ODataResult OData<T>(this IResultExtensions resultExtensions, T result, HttpStatusCode statusCode)
        {
            return new ODataResult(result, statusCode);
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
            object res = Result;

            if (Result is ODataResult odataResult)
            {
                res = odataResult.Result;
            }
            else
            { 
            }

            await httpContext.WriteODataPayloadAsync(res, res.GetType());
        }
    }
}

#endif
