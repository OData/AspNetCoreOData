//-----------------------------------------------------------------------------
// <copyright file="ODataQueryEndpointFilter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Config;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// The default implementation to <see cref="IODataQueryEndpointFilter"/> to run codes before and after a route handler.
/// This typically is used for minimal api scenario.
/// </summary>
public class ODataQueryEndpointFilter : IODataQueryEndpointFilter
{
    /// <summary>
    /// Gets the validation settings. Customers can use this to configure the validation.
    /// </summary>
    public ODataValidationSettings ValidationSettings { get; } = new ODataValidationSettings();

    /// <summary>
    /// Gets the query settings. Customers can use this to configure each query executing.
    /// </summary>
    /// <remarks>
    /// It could be confusing between DefaultQueryConfigurations and ODataQuerySettings.
    /// DefaultQueryConfigurations is used to config the functionalities for query options. For example: is $filter enabled?
    /// ODataQuerySettings is used to set options for each query executing.
    /// </remarks>
    public ODataQuerySettings QuerySettings { get; } = new ODataQuerySettings();

    /// <summary>
    /// Implements the core logic associated with the filter given a <see cref="EndpointFilterInvocationContext"/>
    /// and the next filter to call in the pipeline.
    /// </summary>
    /// <param name="invocationContext">The <see cref="EndpointFilterInvocationContext"/> associated with the current request/response.</param>
    /// <param name="next">The next filter in the pipeline.</param>
    /// <returns>An awaitable result of calling the handler and apply any modifications made by filters in the pipeline.</returns>
    public virtual async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(invocationContext);
        ArgumentNullException.ThrowIfNull(next);

        var endpoint = invocationContext.HttpContext.GetEndpoint();
        if (endpoint is null)
        {
            return await next(invocationContext);
        }

        // https://github.com/dotnet/aspnetcore/blob/main/src/Http/Routing/src/RouteEndpointDataSource.cs#L171
        // Add MethodInfo and HttpMethodMetadata(if any) as first metadata items as they are intrinsic to the route much like
        // the pattern or default display name. This gives visibility to conventions like WithOpenApi() to intrinsic route details
        // (namely the MethodInfo) even when applied early as group conventions.
        MethodInfo methodInfo = endpoint.Metadata.OfType<MethodInfo>().FirstOrDefault();

        // If the endpoint metadata doesn't contain 'MethodInfo', it's a 'RequestDelegate' method.
        // Maybe we should not take this into consideration? since the RequestDelegate return type is 'Task', we cannot idendify the real return type from Task?
        methodInfo = methodInfo ?? endpoint.RequestDelegate.Method;

        if (methodInfo is null)
        {
            return await next(invocationContext);
        }

        var odataFilterContext = new ODataQueryFilterInvocationContext { MethodInfo = methodInfo, InvocationContext = invocationContext };

        await OnFilterExecutingAsync(odataFilterContext);

        // calling into next filter or the route handler.
        var result = await next(invocationContext);

        return await OnFilterExecutedAsync(result, odataFilterContext);
    }

    /// <summary>
    /// Performs the query composition before route handler is executing.
    /// </summary>
    /// <param name="context">The OData query filter invocation context.</param>
    /// <returns>The <see cref="ValueTask"/>.</returns>
    public virtual async ValueTask OnFilterExecutingAsync(ODataQueryFilterInvocationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        HttpContext httpContext = context.HttpContext;

        // Use RequestQueryData to save the query validatation before route handler executing.
        // This is same logic as EnableQueryAttribute. This is not required. However, it can give a better perf.
        RequestQueryData requestQueryData = new RequestQueryData()
        {
            QueryValidationRunBeforeActionExecution = false,
        };

        httpContext.Items.TryAdd(nameof(RequestQueryData), requestQueryData);

        ODataQueryOptions queryOptions = CreateQueryOptionsOnExecuting(context);
        if (queryOptions == null)
        {
            return; // skip validation if we cannot create the query option on executing.
        }

        // Create and validate the query options.
        requestQueryData.QueryValidationRunBeforeActionExecution = true;
        requestQueryData.ProcessedQueryOptions = queryOptions;

        // We should add async version for the Valiator then we can call them here again to waive the ValueTask.CompletedTask.
        ValidateQuery(httpContext, requestQueryData.ProcessedQueryOptions);

        await ValueTask.CompletedTask;
    }

    /// <summary>
    /// Performs the query composition after route handler is executed.
    /// </summary>
    /// <param name="responseValue">The response value from the route handler.</param>
    /// <param name="context">The OData query filter invocation context.</param>
    /// <returns>The <see cref="ValueTask"/>.</returns>
    public virtual async ValueTask<object> OnFilterExecutedAsync(object responseValue, ODataQueryFilterInvocationContext context)
    {
        if (responseValue == null)
        {
            return null;
        }

        ArgumentNullException.ThrowIfNull(context);
        object originalValue = responseValue;

        // TODO: Process the SingleResult<T> and PageResult<T> as return type later
        IQueryable singleResultCollection = null;

        bool isODataResult = false;
        if (responseValue is ODataResult odataResult)
        {
            isODataResult = true;
            responseValue = odataResult.Value;
        }

        if (!QuerySettings.PageSize.HasValue && responseValue != null)
        {
            GetModelBoundPageSize(context, responseValue, singleResultCollection);
        }

        HttpRequest request = context.HttpContext.Request;
        bool shouldApplyQuery = responseValue != null &&
           request.GetEncodedUrl() != null &&
           (!string.IsNullOrWhiteSpace(request.QueryString.Value) ||
           QuerySettings.PageSize.HasValue ||
           QuerySettings.ModelBoundPageSize.HasValue ||
           singleResultCollection != null);

        if (!shouldApplyQuery)
        {
            return originalValue;
        }

        object queryResult = ExecuteQuery(responseValue, singleResultCollection, context);

        if (isODataResult)
        {
            return new ODataResult(queryResult);
        }

        return queryResult;
    }

    private void GetModelBoundPageSize(ODataQueryFilterInvocationContext context, object responseValue, IQueryable singleResultCollection)
    {
        ODataQueryContext queryContext;

        try
        {
            queryContext = GetODataQueryContext(responseValue, singleResultCollection, context);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(Error.Format(SRResources.UriQueryStringInvalid, e.Message), e);
        }

        ModelBoundQuerySettings querySettings = queryContext.Model.GetModelBoundQuerySettings(queryContext.TargetProperty,
            queryContext.TargetStructuredType);
        if (querySettings != null && querySettings.PageSize.HasValue)
        {
            QuerySettings.ModelBoundPageSize = querySettings.PageSize;
        }
    }

    /// <summary>
    /// Execute the query.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="context">The action context, i.e. action and controller name.</param>
    /// <returns></returns>
    protected virtual object ExecuteQuery(object responseValue, IQueryable singleResultCollection,
         ODataQueryFilterInvocationContext context)
    {
        ODataQueryContext queryContext = GetODataQueryContext(responseValue, singleResultCollection, context);

        // Create and validate the query options.
        ODataQueryOptions queryOptions = CreateAndValidateQueryOptions(context.HttpContext, queryContext);

        ODataQuerySettings querySettings = QuerySettings;

        // apply the query
        IEnumerable enumerable = responseValue as IEnumerable;
        if (enumerable == null || responseValue is string || responseValue is byte[])
        {
            // response is not a collection; we only support $select and $expand on single entities.
            ValidateSelectExpandOnly(queryOptions);

            if (singleResultCollection == null)
            {
                // response is a single entity.
                return ApplyQuery(entity: responseValue, queryOptions: queryOptions, querySettings);
            }
            else
            {
                IQueryable queryable = singleResultCollection as IQueryable;
                queryable = ApplyQuery(queryable, queryOptions, querySettings);
                return SingleOrDefault(queryable, context);
            }
        }
        else
        {
            // response is a collection.
            IQueryable queryable = enumerable as IQueryable ?? enumerable.AsQueryable();
            queryable = ApplyQuery(queryable, queryOptions, querySettings);

            return queryable;
        }
    }

    /// <summary>
    /// Validate the select and expand options.
    /// </summary>
    /// <param name="queryOptions">The query options.</param>
    internal static void ValidateSelectExpandOnly(ODataQueryOptions queryOptions)
    {
        if (queryOptions.Filter != null || queryOptions.Count != null || queryOptions.OrderBy != null
            || queryOptions.Skip != null || queryOptions.Top != null)
        {
            throw new ODataException(Error.Format(SRResources.NonSelectExpandOnSingleEntity));
        }
    }

    /// <summary>
    /// Get a single or default value from a collection.
    /// </summary>
    /// <param name="queryable">The response value as <see cref="IQueryable"/>.</param>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    internal static object SingleOrDefault(IQueryable queryable, ODataQueryFilterInvocationContext context)
    {
        var enumerator = queryable.GetEnumerator();
        try
        {
            var result = enumerator.MoveNext() ? enumerator.Current : null;

            if (enumerator.MoveNext())
            {
                throw new InvalidOperationException(Error.Format(
                    SRResources.SingleResultHasMoreThanOneEntity,
                    context.MethodInfo.Name,
                    "MinimalAPI",
                    "SingleResult"));
            }

            return result;
        }
        finally
        {
            // Ensure any active/open database objects that were created
            // iterating over the IQueryable object are properly closed.
            var disposable = enumerator as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Applies the query to the given entity based on incoming query from uri and query settings.
    /// </summary>
    /// <param name="entity">The original entity from the response message.</param>
    /// <param name="queryOptions">
    /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
    /// </param>
    /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
    public virtual object ApplyQuery(object entity, ODataQueryOptions queryOptions, ODataQuerySettings querySettings)
    {
        if (entity == null)
        {
            throw Error.ArgumentNull("entity");
        }
        if (queryOptions == null)
        {
            throw Error.ArgumentNull("queryOptions");
        }

        return queryOptions.ApplyTo(entity, querySettings);
    }

    /// <summary>
    /// Applies the query to the given IQueryable based on incoming query from uri and query settings. By default,
    /// the implementation supports $top, $skip, $orderby and $filter. Override this method to perform additional
    /// query composition of the query.
    /// </summary>
    /// <param name="queryable">The original queryable instance from the response message.</param>
    /// <param name="queryOptions">
    /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
    /// </param>
    /// <param name="querySettings">The settings.</param>
    public virtual IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions, ODataQuerySettings querySettings)
    {
        ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));
        ArgumentNullException.ThrowIfNull(queryOptions, nameof(queryOptions));

        return queryOptions.ApplyTo(queryable, querySettings);
    }

    /// <summary>
    /// Get the OData query context.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="invocationContext">The action context, i.e. action and controller name.</param>
    /// <returns></returns>
    private ODataQueryContext GetODataQueryContext(object responseValue,
        IQueryable singleResultCollection,
        ODataQueryFilterInvocationContext invocationContext)
    {
        Type elementClrType = GetElementType(responseValue, singleResultCollection, invocationContext);

        IEdmModel model = GetModel(elementClrType, invocationContext);
        if (model == null)
        {
            throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
        }

        return new ODataQueryContext(model, elementClrType/*, request.ODataFeature().Path*/);
    }

    /// <summary>
    /// Create and validate a new instance of <see cref="ODataQueryOptions"/> from a query and context during action executed.
    /// Developers can override this virtual method to provide its own <see cref="ODataQueryOptions"/>.
    /// </summary>
    /// <param name="httpContext">The http context.</param>
    /// <param name="queryContext">The query context.</param>
    /// <returns>The created <see cref="ODataQueryOptions"/>.</returns>
    protected virtual ODataQueryOptions CreateAndValidateQueryOptions(HttpContext httpContext, ODataQueryContext queryContext)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull("httpContext");
        }

        if (queryContext == null)
        {
            throw Error.ArgumentNull("queryContext");
        }

        RequestQueryData requestQueryData = httpContext.Items[nameof(RequestQueryData)] as RequestQueryData;

        if (requestQueryData != null && requestQueryData.QueryValidationRunBeforeActionExecution)
        {
            // processed, just return the query option and skip validation.
            return requestQueryData.ProcessedQueryOptions;
        }

        ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, httpContext.Request);

        ValidateQuery(httpContext, queryOptions);

        return queryOptions;
    }

    /// <summary>
    /// Get the element type.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    internal static Type GetElementType(
        object responseValue,
        IQueryable singleResultCollection,
        ODataQueryFilterInvocationContext context)
    {
        Contract.Assert(responseValue != null);

        IEnumerable enumerable = responseValue as IEnumerable;
        if (enumerable == null)
        {
            if (singleResultCollection == null)
            {
                return responseValue.GetType();
            }

            enumerable = singleResultCollection;
        }

        Type elementClrType = TypeHelper.GetImplementedIEnumerableType(enumerable.GetType());
        if (elementClrType == null)
        {
            throw Error.InvalidOperation("The element type cannot be determined because the type of the content is not IEnumerable<T> or IQueryable<T>.");
        }

        return elementClrType;
    }

    /// <summary>
    /// Validates the OData query in the incoming request. By default, the implementation throws an exception if
    /// the query contains unsupported query parameters. Override this method to perform additional validation of
    /// the query.
    /// </summary>
    /// <param name="httpContext" />
    /// <param name="queryOptions">
    /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
    /// </param>
    protected virtual void ValidateQuery(HttpContext httpContext, ODataQueryOptions queryOptions)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        ArgumentNullException.ThrowIfNull(queryOptions, nameof(queryOptions));

        IQueryCollection queryParameters = httpContext.Request.Query;
        foreach (var kvp in queryParameters)
        {
            if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                 kvp.Key.StartsWith("$", StringComparison.Ordinal))
            {
                // we don't support any custom query options that start with $
                throw new ODataException(Error.Format(SRResources.CustomQueryOptionNotSupportedWithDollarSign, kvp.Key));
            }
        }

        queryOptions.Validate(ValidationSettings);
    }

    /// <summary>
    /// Creates the <see cref="ODataQueryOptions"/> for action executing validation.
    /// </summary>
    /// <param name="context">The OData query filter invocation context.</param>
    /// <returns>The created <see cref="ODataQueryOptions"/> or null if we can't create it during action executing.</returns>
    protected virtual ODataQueryOptions CreateQueryOptionsOnExecuting(ODataQueryFilterInvocationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ODataQueryContext queryContext;

        // For these cases few options are supported like IEnumerable<T>, Task<IEnumerable<T>>, T, Task<T>
        // Other cases where we cannot determine the return type upfront, are not supported
        // Like IActionResult, SingleResult. For such cases, the validation is run in OnActionExecuted
        // When we have the result.

        // From Minimal API doc
        // Minimal endpoints support the following types of return values:
        // 1. string -This includes Task<string> and ValueTask<string>.
        // 2. T(Any other type) - This includes Task<T> and ValueTask<T>.
        // 3. IResult based -This includes Task<IResult> and ValueTask<IResult>

        Type returnType = context.MethodInfo.ReturnType;
        if (returnType is null)
        {
            return null;
        }

        if (returnType.IsGenericType)
        {
            Type genericTypeDef = returnType.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(Task<>) || genericTypeDef == typeof(ValueTask<>))
            {
                returnType = returnType.GetGenericArguments().First();
            }
        }

        if (returnType == typeof(ODataResult))
        {
            return null;
        }

        TypeHelper.IsCollection(returnType, out Type elementType);

        IEdmModel edmModel = GetModel(elementType, context);

        queryContext = new ODataQueryContext(edmModel, elementType);

        // Get or create the service provider based on the model. It should save into the ODataFeature() in the method.
        context.HttpContext.GetOrCreateServiceProvider();
        IODataFeature odataFeature = context.HttpContext.ODataFeature();

        if (odataFeature.Services is null)
        {
            // Why? For example, developer overwrites 'GetModel', and in his version, he didn't create the ServiceProvider.
            odataFeature.Services = context.HttpContext.BuildDefaultServiceProvider(edmModel);
        }

        // Create the query options.
        return new ODataQueryOptions(queryContext, context.HttpContext.Request);
    }

    /// <summary>
    /// Gets the EDM model for the given type and request.Override this method to customize the EDM model used for
    /// querying.
    /// </summary>
    /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
    /// <param name="context">The action descriptor for the action being queried on.</param>
    /// <returns>The EDM model for the given type and request.</returns>
    protected virtual IEdmModel GetModel(Type elementClrType, ODataQueryFilterInvocationContext context)
    {
        HttpContext httpContext = context.HttpContext;

        return httpContext.GetOrCreateEdmModel(elementClrType);
    }
}

