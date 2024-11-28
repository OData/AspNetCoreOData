//-----------------------------------------------------------------------------
// <copyright file="EnableQueryAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This class defines an attribute that can be applied to an action to enable querying using the OData query
/// syntax. To avoid processing unexpected or malicious queries, use the validation settings on
/// <see cref="EnableQueryAttribute"/> to validate incoming queries.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want to be able to subclass this type.")]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public partial class EnableQueryAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Performs the query composition before action is executing.
    /// </summary>
    /// <param name="actionExecutingContext">The action executing context.</param>
    public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
    {
        if (actionExecutingContext == null)
        {
            throw new ArgumentNullException(nameof(actionExecutingContext));
        }

        base.OnActionExecuting(actionExecutingContext);

        RequestQueryData requestQueryData = new RequestQueryData()
        {
            QueryValidationRunBeforeActionExecution = false,
        };

        actionExecutingContext.HttpContext.Items.TryAdd(nameof(RequestQueryData), requestQueryData);

        try
        {
            ODataQueryOptions queryOptions = CreateQueryOptionsOnExecuting(actionExecutingContext);

            if (queryOptions == null)
            {
                return; // skip validation
            }

            // Create and validate the query options.
            requestQueryData.QueryValidationRunBeforeActionExecution = true;
            requestQueryData.ProcessedQueryOptions = queryOptions;
                
            HttpRequest request = actionExecutingContext.HttpContext.Request;
            ValidateQuery(request, requestQueryData.ProcessedQueryOptions);
        }
        catch (ArgumentOutOfRangeException e)
        {
            actionExecutingContext.Result = CreateBadRequestResult(
                Error.Format(SRResources.QueryParameterNotSupported, e.Message),
                e);
        }
        catch (NotImplementedException e)
        {
            actionExecutingContext.Result = CreateBadRequestResult(
                Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                e);
        }
        catch (NotSupportedException e)
        {
            actionExecutingContext.Result = CreateBadRequestResult(
                Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                e);
        }
        catch (InvalidOperationException e)
        {
            // Will also catch ODataException here because ODataException derives from InvalidOperationException.
            actionExecutingContext.Result = CreateBadRequestResult(
                Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                e);
        }
    }

    /// <summary>
    /// Creates the <see cref="ODataQueryOptions"/> for action executing validation.
    /// </summary>
    /// <param name="actionExecutingContext">The action executing context.</param>
    /// <returns>The created <see cref="ODataQueryOptions"/> or null if we can't create it during action executing.</returns>
    protected virtual ODataQueryOptions CreateQueryOptionsOnExecuting(ActionExecutingContext actionExecutingContext)
    {
        if (actionExecutingContext == null)
        {
            throw new ArgumentNullException(nameof(actionExecutingContext));
        }

        HttpRequest request = actionExecutingContext.HttpContext.Request;
        ODataPath path = request.ODataFeature().Path;

        _querySettings.TimeZone = request.GetTimeZoneInfo();

        ODataQueryContext queryContext;

        // For OData based controllers.
        if (path != null)
        {
            IEdmType edmType = path.GetEdmType();

            // When $count is at the end, the return type is always int. Trying to instead fetch the return type of the actual type being counted on.
            if (request.IsCountRequest())
            {
                ODataPathSegment[] pathSegments = path.ToArray();
                edmType = pathSegments[pathSegments.Length - 2].EdmType;
            }

            IEdmType elementType = edmType.AsElementType();
            IEdmModel edmModel = request.GetModel();

            // For Swagger metadata request. elementType is null.
            if (elementType == null || edmModel == null)
            {
                return null;
            }

            if (elementType.IsUntyped())
            {
                // TODO: so far, we don't know how to process query on Edm.Untyped.
                // So, if the query data type is Edm.Untyped, or collection of Edm.Untyped,
                // Let's simply skip it now.
                return null;
            }

            Type clrType = edmModel.GetClrType(elementType.ToEdmTypeReference(isNullable: false));

            // CLRType can be missing if untyped registrations were made.
            if (clrType != null)
            {
                queryContext = new ODataQueryContext(edmModel, clrType, path);
            }
            else
            {
                // In case where CLRType is missing, $count, $expand verifications cannot be done.
                // More importantly $expand required ODataQueryContext with clrType which cannot be done
                // If the model is untyped. Hence for such cases, letting the validation run post action.
                return null;
            }
        }
        else
        {
            // For non-OData Json based controllers.
            // For these cases few options are supported like IEnumerable<T>, Task<IEnumerable<T>>, T, Task<T>
            // Other cases where we cannot determine the return type upfront, are not supported
            // Like IActionResult, SingleResult. For such cases, the validation is run in OnActionExecuted
            // When we have the result.
            ControllerActionDescriptor controllerActionDescriptor = actionExecutingContext.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null)
            {
                return null;
            }

            Type returnType = controllerActionDescriptor.MethodInfo.ReturnType;
            if (actionExecutingContext.Controller is IDiscoverEndpointType) 
                returnType = ((IDiscoverEndpointType)actionExecutingContext.Controller).GetEndpointType();            
            Type elementType;

            // For Task<> get the base object.
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                returnType = returnType.GetGenericArguments().First();
            }

            // For NetCore2.2+ new type ActionResult<> was created which encapsulates IActionResult and T result.
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                returnType = returnType.GetGenericArguments().First();
            }

            if (TypeHelper.IsCollection(returnType))
            {
                elementType = TypeHelper.GetImplementedIEnumerableType(returnType);
            }
            else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                elementType = returnType.GetGenericArguments().First();
            }
            else
            {
                return null;
            }

            IEdmModel edmModel = GetModel(elementType, request, controllerActionDescriptor);
            queryContext = new ODataQueryContext(edmModel, elementType);
        }

        // Create and validate the query options.
        return new ODataQueryOptions(queryContext, request);
    }

    /// <summary>
    /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
    /// returning response message. It then validates the query from uri based on the validation settings on
    /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
    /// the response message.
    /// </summary>
    /// <param name="actionExecutedContext">The context related to this action, including the response message,
    /// request message and HttpConfiguration etc.</param>
    public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
    {
        if (actionExecutedContext == null)
        {
            throw new ArgumentNullException(nameof(actionExecutedContext));
        }

        HttpRequest request = actionExecutedContext.HttpContext.Request;
        if (request == null)
        {
            throw Error.Argument("actionExecutedContext", SRResources.ActionExecutedContextMustHaveRequest);
        }

        ActionDescriptor actionDescriptor = actionExecutedContext.ActionDescriptor;
        if (actionDescriptor == null)
        {
            throw Error.Argument("actionExecutedContext", SRResources.ActionContextMustHaveDescriptor);
        }

        HttpResponse response = actionExecutedContext.HttpContext.Response;

        // Check is the response is set and successful.
        if (response != null && IsSuccessStatusCode(response.StatusCode) && actionExecutedContext.Result != null)
        {
            // actionExecutedContext.Result might also indicate a status code that has not yet
            // been applied to the result; make sure it's also successful.
            IStatusCodeActionResult statusCodeResult = actionExecutedContext.Result as IStatusCodeActionResult;
            if (statusCodeResult?.StatusCode == null || IsSuccessStatusCode(statusCodeResult.StatusCode.Value))
            {
                ObjectResult responseContent = actionExecutedContext.Result as ObjectResult;

                ControllerActionDescriptor controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
                Type returnType = controllerActionDescriptor.MethodInfo.ReturnType;

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
                {
                    returnType = returnType.GetGenericArguments().First();

                    if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
                    {
                        responseContent.DeclaredType = returnType;
                    }
                }

                if (responseContent != null)
                {
                    // Get collection from SingleResult.
                    IQueryable singleResultCollection = null;
                    SingleResult singleResult = responseContent.Value as SingleResult;
                    if (singleResult != null)
                    {
                        // This could be a SingleResult, which has the property Queryable.
                        // But it could be a SingleResult() or SingleResult<T>. Sort by number of parameters
                        // on the property and get the one with the most parameters.
                        PropertyInfo propInfo = responseContent.Value.GetType().GetProperties()
                            .OrderBy(p => p.GetIndexParameters().Length)
                            .Where(p => p.Name.Equals("Queryable", StringComparison.Ordinal))
                            .LastOrDefault();

                        singleResultCollection = propInfo.GetValue(singleResult) as IQueryable;
                    }

                    // Execution the action.
                    object queryResult = OnActionExecuted(
                        actionExecutedContext,
                        responseContent.Value,
                        singleResultCollection,
                        actionDescriptor as ControllerActionDescriptor,
                        request);

                    if (queryResult != null)
                    {
                        responseContent.Value = queryResult;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
    /// returning response message. It then validates the query from uri based on the validation settings on
    /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
    /// the response message.
    /// </summary>
    /// <param name="actionExecutedContext">.</param>
    /// <param name="responseValue">The response content value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <param name="request">The internal request.</param>
    private object OnActionExecuted(
        ActionExecutedContext actionExecutedContext,
        object responseValue,
        IQueryable singleResultCollection,
        ControllerActionDescriptor actionDescriptor,
        HttpRequest request)
    {
        if (!_querySettings.PageSize.HasValue && responseValue != null)
        {
            GetModelBoundPageSize(actionExecutedContext, responseValue, singleResultCollection, actionDescriptor, request);
        }

        // Apply the query if there are any query options, if there is a page size set, in the case of
        // SingleResult or in the case of $count request.
        bool shouldApplyQuery = responseValue != null &&
           request.GetEncodedUrl() != null &&
           (!String.IsNullOrWhiteSpace(request.QueryString.Value) ||
           _querySettings.PageSize.HasValue ||
           _querySettings.ModelBoundPageSize.HasValue ||
           singleResultCollection != null ||
           request.IsCountRequest() ||
           ContainsAutoSelectExpandProperty(responseValue, singleResultCollection, actionDescriptor, request));

        object returnValue = null;
        if (shouldApplyQuery)
        {
            try
            {
                object queryResult = ExecuteQuery(responseValue, singleResultCollection, actionDescriptor, request);
                if (queryResult == null && (request.ODataFeature().Path == null || singleResultCollection != null))
                {
                    // This is the case in which a regular OData service uses the EnableQuery attribute.
                    // For OData services ODataNullValueMessageHandler should be plugged in for the service
                    // if this behavior is desired.
                    // For non OData services this behavior is equivalent as the one in the v3 version in order
                    // to reduce the friction when they decide to move to use the v4 EnableQueryAttribute.
                    actionExecutedContext.Result = new StatusCodeResult((int)HttpStatusCode.NotFound);
                }

                returnValue = queryResult;
            }
            catch (ArgumentOutOfRangeException e)
            {
                actionExecutedContext.Result = CreateBadRequestResult(Error.Format(SRResources.QueryParameterNotSupported, e.Message), e);
            }
            catch (NotImplementedException e)
            {
                actionExecutedContext.Result = CreateBadRequestResult(Error.Format(SRResources.UriQueryStringInvalid, e.Message), e);
            }
            catch (NotSupportedException e)
            {
                actionExecutedContext.Result = CreateBadRequestResult(Error.Format(SRResources.UriQueryStringInvalid, e.Message), e);
            }
            catch (InvalidOperationException e)
            {
                // Will also catch ODataException here because ODataException derives from InvalidOperationException.
                actionExecutedContext.Result = CreateBadRequestResult(Error.Format(SRResources.UriQueryStringInvalid, e.Message), e);
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Get the page size.
    /// </summary>
    /// <param name="actionExecutedContext">The response value.</param>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <param name="request">The request.</param>
    private void GetModelBoundPageSize(
        ActionExecutedContext actionExecutedContext,
        object responseValue,
        IQueryable singleResultCollection,
        ControllerActionDescriptor actionDescriptor,
        HttpRequest request)
    {
        ODataQueryContext queryContext;

        try
        {
            queryContext = GetODataQueryContext(responseValue, singleResultCollection, actionDescriptor, request);
        }
        catch (InvalidOperationException e)
        {
            actionExecutedContext.Result = CreateBadRequestResult(Error.Format(SRResources.UriQueryStringInvalid, e.Message), e);
            return;
        }

        ModelBoundQuerySettings querySettings = queryContext.Model.GetModelBoundQuerySettings(queryContext.TargetProperty,
            queryContext.TargetStructuredType);
        if (querySettings != null && querySettings.PageSize.HasValue)
        {
            _querySettings.ModelBoundPageSize = querySettings.PageSize;
        }
    }

    /// <summary>
    /// Create a BadRequestObjectResult.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>A BadRequestObjectResult.</returns>
    private static BadRequestObjectResult CreateBadRequestResult(string message, Exception exception)
    {
        SerializableError error = CreateErrorResponse(message, exception);
        return new BadRequestObjectResult(error);
    }

    /// <summary>
    /// Create an error response.
    /// </summary>
    /// <param name="message">The message of the error.</param>
    /// <param name="exception">The error exception if any.</param>
    /// <returns>A SerializableError.</returns>
    /// <remarks>This function is recursive.</remarks>
    public static SerializableError CreateErrorResponse(string message, Exception exception = null)
    {
        // The key values mimic the behavior of HttpError in AspNet. It's a fine format
        // and many of the test cases expect it.
        SerializableError error = new SerializableError();
        if (!String.IsNullOrEmpty(message))
        {
            error.Add(SerializableErrorKeys.MessageKey, message);
        }

        if (exception != null)
        {
            error.Add(SerializableErrorKeys.ExceptionMessageKey, exception.Message);
            error.Add(SerializableErrorKeys.ExceptionTypeKey, exception.GetType().FullName);
            error.Add(SerializableErrorKeys.StackTraceKey, exception.StackTrace);
            if (exception.InnerException != null)
            {
                error.Add(SerializableErrorKeys.InnerExceptionKey, CreateErrorResponse(String.Empty, exception.InnerException));
            }
        }

        return error;
    }

    /// <summary>
    /// Determine if the status code indicates success.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <returns>True if the response has a success status code; false otherwise.</returns>
    private static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }

    /// <summary>
    /// Execute the query.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <param name="request">The internal request.</param>
    /// <returns></returns>
    private object ExecuteQuery(
        object responseValue,
        IQueryable singleResultCollection,
        ControllerActionDescriptor actionDescriptor,
        HttpRequest request)
    {
        ODataQueryContext queryContext = GetODataQueryContext(responseValue, singleResultCollection, actionDescriptor, request);

        // Create and validate the query options.
        ODataQueryOptions queryOptions = CreateAndValidateQueryOptions(request, queryContext);

        // apply the query
        IEnumerable enumerable = responseValue as IEnumerable;
        if (enumerable == null || responseValue is string || responseValue is byte[])
        {
            // response is not a collection; we only support $select and $expand on single entities.
            ValidateSelectExpandOnly(queryOptions);

            if (singleResultCollection == null)
            {
                // response is a single entity.
                return ApplyQuery(entity: responseValue, queryOptions: queryOptions);
            }
            else
            {
                IQueryable queryable = singleResultCollection as IQueryable;
                queryable = ApplyQuery(queryable, queryOptions);
                return SingleOrDefault(queryable, actionDescriptor);
            }
        }
        else
        {
            // response is a collection.
            IQueryable queryable = (enumerable as IQueryable) ?? enumerable.AsQueryable();
            queryable = ApplyQuery(queryable, queryOptions);

            if (request.IsCountRequest())
            {
                long? count = request.ODataFeature().TotalCount;

                if (count.HasValue)
                {
                    // Return the count value if it is a $count request.
                    return count.Value;
                }
            }

            return queryable;
        }
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
    public virtual IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        if (queryable == null)
        {
            throw Error.ArgumentNull("queryable");
        }
        if (queryOptions == null)
        {
            throw Error.ArgumentNull("queryOptions");
        }

        return queryOptions.ApplyTo(queryable, _querySettings);
    }

    /// <summary>
    /// Applies the query to the given entity based on incoming query from uri and query settings.
    /// </summary>
    /// <param name="entity">The original entity from the response message.</param>
    /// <param name="queryOptions">
    /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
    /// </param>
    /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
    public virtual object ApplyQuery(object entity, ODataQueryOptions queryOptions)
    {
        if (entity == null)
        {
            throw Error.ArgumentNull("entity");
        }
        if (queryOptions == null)
        {
            throw Error.ArgumentNull("queryOptions");
        }

        return queryOptions.ApplyTo(entity, _querySettings);
    }

    /// <summary>
    /// Create and validate a new instance of <see cref="ODataQueryOptions"/> from a query and context during action executed.
    /// Developers can override this virtual method to provide its own <see cref="ODataQueryOptions"/>.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="queryContext">The query context.</param>
    /// <returns>The created <see cref="ODataQueryOptions"/>.</returns>
    protected virtual ODataQueryOptions CreateAndValidateQueryOptions(HttpRequest request, ODataQueryContext queryContext)
    {
        if (request == null)
        {
            throw Error.ArgumentNull("request");
        }

        if (queryContext == null)
        {
            throw Error.ArgumentNull("queryContext");
        }

        RequestQueryData requestQueryData = request.HttpContext.Items[nameof(RequestQueryData)] as RequestQueryData;

        if (requestQueryData != null && requestQueryData.QueryValidationRunBeforeActionExecution)
        {
            // processed, just return the query option and skip validation.
            return requestQueryData.ProcessedQueryOptions;
        }

        ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, request);

        ValidateQuery(request, queryOptions);

        return queryOptions;
    }

    /// <summary>
    /// Get a single or default value from a collection.
    /// </summary>
    /// <param name="queryable">The response value as <see cref="IQueryable"/>.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <returns></returns>
    internal static object SingleOrDefault(
        IQueryable queryable,
        ControllerActionDescriptor actionDescriptor)
    {
        var enumerator = queryable.GetEnumerator();
        try
        {
            var result = enumerator.MoveNext() ? enumerator.Current : null;

            if (enumerator.MoveNext())
            {
                throw new InvalidOperationException(Error.Format(
                    SRResources.SingleResultHasMoreThanOneEntity,
                    actionDescriptor.ActionName,
                    actionDescriptor.ControllerName,
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
    /// Get the OData query context.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <param name="request">The OData path.</param>
    /// <returns></returns>
    private ODataQueryContext GetODataQueryContext(
        object responseValue,
        IQueryable singleResultCollection,
        ControllerActionDescriptor actionDescriptor,
        HttpRequest request)
    {
        Type elementClrType = GetElementType(responseValue, singleResultCollection, actionDescriptor);

        IEdmModel model = GetModel(elementClrType, request, actionDescriptor);
        if (model == null)
        {
            throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
        }

        return new ODataQueryContext(model, elementClrType, request.ODataFeature().Path);
    }

    /// <summary>
    /// Get the element type.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <returns></returns>
    internal static Type GetElementType(
        object responseValue,
        IQueryable singleResultCollection,
        ControllerActionDescriptor actionDescriptor)
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
            // The element type cannot be determined because the type of the content
            // is not IEnumerable<T> or IQueryable<T>.
            throw Error.InvalidOperation(
                SRResources.FailedToRetrieveTypeToBuildEdmModel,
                typeof(EnableQueryAttribute).Name,
                actionDescriptor.ActionName,
                actionDescriptor.ControllerName,
                responseValue.GetType().FullName);
        }

        return elementClrType;
    }

    /// <summary>
    /// Validates the OData query in the incoming request. By default, the implementation throws an exception if
    /// the query contains unsupported query parameters. Override this method to perform additional validation of
    /// the query.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="queryOptions">
    /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
    /// </param>
    public virtual void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        if (request == null)
        {
            throw Error.ArgumentNull(nameof(request));
        }

        if (queryOptions == null)
        {
            throw Error.ArgumentNull(nameof(queryOptions));
        }

        IQueryCollection queryParameters = request.Query;
        foreach (var kvp in queryParameters)
        {
            if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                 kvp.Key.StartsWith("$", StringComparison.Ordinal))
            {
                // we don't support any custom query options that start with $
                // this should be caught be OnActionExecuted().
                throw new ODataException(Error.Format(SRResources.CustomQueryOptionNotSupportedWithDollarSign, kvp.Key));
            }
        }

        queryOptions.Validate(_validationSettings);
    }

    /// <summary>
    /// Determine if the query contains auto select and expand property.
    /// </summary>
    /// <param name="responseValue">The response value.</param>
    /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
    /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
    /// <param name="request">The Http request.</param>
    /// <returns>true/false</returns>
    private bool ContainsAutoSelectExpandProperty(object responseValue, IQueryable singleResultCollection,
        ControllerActionDescriptor actionDescriptor, HttpRequest request)
    {
        Type elementClrType = GetElementType(responseValue, singleResultCollection, actionDescriptor);

        IEdmModel model = GetModel(elementClrType, request, actionDescriptor);
        if (model == null)
        {
            throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
        }
        IEdmType edmType = model.GetEdmTypeReference(elementClrType)?.Definition;

        IEdmStructuredType structuredType = edmType as IEdmStructuredType;
        ODataPath path = request.ODataFeature().Path;

        IEdmProperty pathProperty = null;
        IEdmStructuredType pathStructuredType = null;
        if (path != null)
        {
            (pathProperty, pathStructuredType, _) = path.GetPropertyAndStructuredTypeFromPath();
        }

        // Take the type and property from path first, it's higher priority than the value type.
        if (pathStructuredType != null && pathProperty != null)
        {
            return model.HasAutoExpandProperty(pathStructuredType, pathProperty) || model.HasAutoSelectProperty(pathStructuredType, pathProperty);
        }
        else if (structuredType != null)
        {
            return model.HasAutoExpandProperty(structuredType, null) || model.HasAutoSelectProperty(structuredType, null);
        }

        return false;
    }

    /// <summary>
    /// Gets the EDM model for the given type and request.Override this method to customize the EDM model used for
    /// querying.
    /// </summary>
    /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
    /// <param name="request">The request message to retrieve a model for.</param>
    /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
    /// <returns>The EDM model for the given type and request.</returns>
    public virtual IEdmModel GetModel(
        Type elementClrType,
        HttpRequest request,
        ActionDescriptor actionDescriptor)
    {
        // Get model for the request
        IEdmModel model = request.GetModel();

        if (model == null ||
            model == EdmCoreModel.Instance || model.GetEdmType(elementClrType) == null)
        {
            // user has not configured anything or has registered a model without the element type
            // let's create one just for this type and cache it in the action descriptor
            model = actionDescriptor.GetEdmModel(request, elementClrType);
        }

        Contract.Assert(model != null);
        return model;
    }

    /// <summary>
    /// Holds request level query information.
    /// </summary>
    private class RequestQueryData
    {
        /// <summary>
        /// Gets or sets a value indicating whether query validation was run before action (controller method) is executed.
        /// </summary>
        /// <remarks>
        /// Marks if the query validation was run before the action execution. This is not always possible.
        /// For cases where the run failed before action execution. We will run validation on result.
        /// </remarks>
        public bool QueryValidationRunBeforeActionExecution { get; set; }

        /// <summary>
        /// Gets or sets the processed query options.
        /// </summary>
        /// <remarks>
        /// Stores the processed query options to be used later if OnActionExecuting was able to verify the query.
        /// This is because ValidateQuery internally modifies query options (expands are prime example of this).
        /// </remarks>
        public ODataQueryOptions ProcessedQueryOptions { get; set; }
    }
}
