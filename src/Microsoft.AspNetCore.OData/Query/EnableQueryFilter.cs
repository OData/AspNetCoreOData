#if NET7_0
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Results.MinimalAPIResults;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// An endpoint filter for applying query options to an 
    /// </summary>
    public partial class EnableQueryFilter : IEndpointFilter
    {
        /// <inheritdoc/>
        public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            onEndpointExecuting(context);

            var result = await next(context);

            var res = OnEndPointExecuted(context, result);

            return new ODataResult(res);
        }

        private void onEndpointExecuting(EndpointFilterInvocationContext context)
        {
            RequestQueryData requestQueryData = new RequestQueryData()
            {
                QueryValidationRunBeforeActionExecution = false,
            };

            context.HttpContext.Items.TryAdd(nameof(RequestQueryData), requestQueryData);

            try
            {
                ODataQueryOptions queryOptions = CreateQueryOptionsOnExecuting(context);

                // Create and validate the query options.
                requestQueryData.QueryValidationRunBeforeActionExecution = true;
                requestQueryData.ProcessedQueryOptions = queryOptions;

                HttpRequest request = context.HttpContext.Request;
                ValidateQuery(request, requestQueryData.ProcessedQueryOptions);
            }
            catch (ArgumentOutOfRangeException e)
            {
                //Add logic for handling the different exceptions. 
                throw new Exception();
            }
            catch (NotImplementedException e)
            {
                throw new NotImplementedException();
            }
            catch (NotSupportedException e)
            {
                throw new NotSupportedException();
            }
            catch (InvalidOperationException e)
            {
                // Will also catch ODataException here because ODataException derives from InvalidOperationException.
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Creates the <see cref="ODataQueryOptions"/> for action executing validation.
        /// </summary>
        /// <param name="context">The endpoint  executing context.</param>
        /// <returns>The created <see cref="ODataQueryOptions"/> or null if we can't create it during action executing.</returns>
        protected virtual ODataQueryOptions CreateQueryOptionsOnExecuting(EndpointFilterInvocationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpRequest request = context.HttpContext.Request;
            var odataFeature = context.HttpContext.ODataFeature();
            ODataPath path = odataFeature.Path;

            _querySettings.TimeZone = request.GetTimeZoneInfo();

            ODataQueryContext queryContext;

            IEdmType edmType = path.GetEdmType();

            // When $count is at the end, the return type is always int. Trying to instead fetch the return type of the actual type being counted on.
            if (request.IsCountRequest())
            {
                ODataPathSegment[] pathSegments = path.ToArray();
                edmType = pathSegments[pathSegments.Length - 2].EdmType;
            }

            IEdmType elementType = edmType.AsElementType();
            if (elementType.IsUntyped())
            {
                // TODO: so far, we don't know how to process query on Edm.Untyped.
                // So, if the query data type is Edm.Untyped, or collection of Edm.Untyped,
                // Let's simply skip it now.
                return null;
            }

            IEdmModel edmModel = request.GetModel();

            // For Swagger metadata request. elementType is null.
            if (elementType == null || edmModel == null)
            {
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

            // Create and validate the query options.
            return new ODataQueryOptions(queryContext, request);
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
        internal virtual void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
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

        /// <summary>
        /// Performs the query composition after endpoint is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
        /// </summary>
        /// <param name="context">The context related to this action, including the response message,
        /// request message and HttpConfiguration etc.</param>
        private object? OnEndPointExecuted(EndpointFilterInvocationContext context, object? result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.Argument("context", SRResources.ActionExecutedContextMustHaveRequest);
            }


            HttpResponse response = context.HttpContext.Response;

            // Check is the response is set and successful.
            if (response != null && IsSuccessStatusCode(response.StatusCode) && result != null)
            {
                // actionExecutedContext.Result might also indicate a status code that has not yet
                // been applied to the result; make sure it's also successful.
                IStatusCodeActionResult statusCodeResult = result as IStatusCodeActionResult;
                if (statusCodeResult?.StatusCode == null || IsSuccessStatusCode(statusCodeResult.StatusCode.Value))
                {
                    ODataResult responseContent = result as ODataResult;
                    if (responseContent != null)
                    {
                        // Get collection from SingleResult.
                        IQueryable singleResultCollection = null;
                        SingleResult singleResult = result as SingleResult;
                        if (singleResult != null)
                        {
                            // This could be a SingleResult, which has the property Queryable.
                            // But it could be a SingleResult() or SingleResult<T>. Sort by number of parameters
                            // on the property and get the one with the most parameters.
                            PropertyInfo propInfo = result.GetType().GetProperties()
                                .OrderBy(p => p.GetIndexParameters().Length)
                                .Where(p => p.Name.Equals("Queryable", StringComparison.Ordinal))
                                .LastOrDefault();

                            singleResultCollection = propInfo.GetValue(singleResult) as IQueryable;
                        }

                        // Execution the action.
                        object? queryResult = OnActionExecuted(
                            context,
                            responseContent.Result,
                            singleResultCollection,
                            request);

                        if (queryResult != null)
                        {
                            result = queryResult;
                        }
                    }
                    else
                    {
                        // Get collection from SingleResult.
                        IQueryable singleResultCollection = null;
                        SingleResult singleResult = result as SingleResult;
                        if (singleResult != null)
                        {
                            // This could be a SingleResult, which has the property Queryable.
                            // But it could be a SingleResult() or SingleResult<T>. Sort by number of parameters
                            // on the property and get the one with the most parameters.
                            PropertyInfo propInfo = result.GetType().GetProperties()
                                .OrderBy(p => p.GetIndexParameters().Length)
                                .Where(p => p.Name.Equals("Queryable", StringComparison.Ordinal))
                                .LastOrDefault();

                            singleResultCollection = propInfo.GetValue(singleResult) as IQueryable;
                        }

                        // Execution the action.
                        object? queryResult = OnActionExecuted(
                            context,
                            result,
                            singleResultCollection,
                            request);

                        if (queryResult != null)
                        {
                            result = queryResult;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
        /// </summary>
        /// <param name="context">.</param>
        /// <param name="responseValue">The response content value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="request">The internal request.</param>
        internal object OnActionExecuted(
            EndpointFilterInvocationContext context,
            object? responseValue,
            IQueryable singleResultCollection,
            HttpRequest request)
        {
            if (!_querySettings.PageSize.HasValue && responseValue != null)
            {
                GetModelBoundPageSize(context, responseValue, singleResultCollection, request);
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
               ContainsAutoSelectExpandProperty(responseValue, singleResultCollection, request));

            object returnValue = null;
            if (shouldApplyQuery)
            {
                try
                {
                    object? queryResult = ExecuteQuery(responseValue, singleResultCollection, request);
                    if (queryResult == null && (request.ODataFeature().Path == null || singleResultCollection != null))
                    {
                        // This is the case in which a regular OData service uses the EnableQuery attribute.
                        // For OData services ODataNullValueMessageHandler should be plugged in for the service
                        // if this behavior is desired.
                        // For non OData services this behavior is equivalent as the one in the v3 version in order
                        // to reduce the friction when they decide to move to use the v4 EnableQueryAttribute.
                       // result = new StatusCodeResult((int)HttpStatusCode.NotFound);
                    }

                    returnValue = queryResult;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    //Add logic for handling the different exceptions. 
                    throw;
                }
                catch (NotImplementedException e)
                {
                    throw;
                }
                catch (NotSupportedException e)
                {
                    throw;
                }
                catch (InvalidOperationException e)
                {
                    throw;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Get the page size.
        /// </summary>
        /// <param name="context">The response value.</param>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="request">The request.</param>
        private void GetModelBoundPageSize(
            EndpointFilterInvocationContext context,
            object? responseValue,
            IQueryable singleResultCollection,
            HttpRequest request)
        {
            ODataQueryContext queryContext;

            try
            {
                Type elementClrType = GetElementType(responseValue, singleResultCollection);
                queryContext = new ODataQueryContext(request.GetModel(), elementClrType, request.ODataFeature().Path);
            }
            catch (InvalidOperationException e)
            {
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
        /// Get the ODaya query context.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="request">The OData path.</param>
        /// <returns></returns>
        private ODataQueryContext GetODataQueryContext(
            object responseValue,
            IQueryable singleResultCollection,
            HttpRequest request)
        {
            Type elementClrType = GetElementType(responseValue, singleResultCollection);

            IEdmModel model = request.GetModel();
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
        /// <returns></returns>
        internal static Type GetElementType(
            object? responseValue,
            IQueryable singleResultCollection)
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
                    responseValue.GetType().FullName);
            }

            return elementClrType;
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
        /// Determine if the query contains auto select and expand property.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="request">The Http request.</param>
        /// <returns>true/false</returns>
        private bool ContainsAutoSelectExpandProperty(object responseValue, IQueryable singleResultCollection,
            HttpRequest request)
        {
            Type elementClrType = GetElementType(responseValue, singleResultCollection);

            IEdmModel model = request.GetModel();
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
        /// Execute the query.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="request">The internal request.</param>
        /// <returns></returns>
        private object? ExecuteQuery(
            object? responseValue,
            IQueryable singleResultCollection,
            HttpRequest request)
        {
            ODataQueryContext queryContext = GetODataQueryContext(responseValue, singleResultCollection, request);

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
                    IQueryable queryable = responseValue as IQueryable;
                    return queryOptions.ApplyTo(queryable, _querySettings);
                }
                else
                {
                    IQueryable queryable = singleResultCollection as IQueryable;
                    queryable = queryOptions.ApplyTo(queryable, _querySettings);
                    return SingleOrDefault(queryable as IQueryable);
                }
            }
            else
            {
                // response is a collection.
                IQueryable queryable = (responseValue as IQueryable) ?? enumerable.AsQueryable();
                queryable = queryOptions.ApplyTo(queryable, _querySettings);

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
        /// Get a single or default value from a collection.
        /// </summary>
        /// <param name="queryable">The response value as <see cref="IQueryable"/>.</param>
        /// <returns></returns>
        internal static object? SingleOrDefault(
            IQueryable queryable)
        {
            var enumerator = queryable.GetEnumerator();
            try
            {
                var result = enumerator.MoveNext() ? enumerator.Current : null;

                if (enumerator.MoveNext())
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.SingleResultHasMoreThanOneEntity,
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
    }

}

#endif
