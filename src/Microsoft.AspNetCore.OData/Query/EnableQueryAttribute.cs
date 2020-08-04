// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable querying using the OData query
    /// syntax. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class EnableQueryAttribute : ActionFilterAttribute
    {
        private const char CommaSeparator = ',';

        // validation settings
        private ODataValidationSettings _validationSettings;
        private string _allowedOrderByProperties;

        // query settings
        private ODataQuerySettings _querySettings;

        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        public EnableQueryAttribute()
        {
            _validationSettings = new ODataValidationSettings();
            _querySettings = new ODataQuerySettings();
        }

        /// <summary>
        /// Gets or sets a value indicating whether query composition should
        /// alter the original query when necessary to ensure a stable sort order.
        /// </summary>
        /// <value>A <c>true</c> value indicates the original query should
        /// be modified when necessary to guarantee a stable sort order.
        /// A <c>false</c> value indicates the sort order can be considered
        /// stable without modifying the query.  Query providers that ensure
        /// a stable sort order should set this value to <c>false</c>.
        /// The default value is <c>true</c>.</value>
        public bool EnsureStableOrdering
        {
            get
            {
                return _querySettings.EnsureStableOrdering;
            }
            set
            {
                _querySettings.EnsureStableOrdering = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition.
        /// </summary>
        /// <value>
        /// The default is <see cref="HandleNullPropagationOption.Default"/>.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation
        {
            get
            {
                return _querySettings.HandleNullPropagation;
            }
            set
            {
                _querySettings.HandleNullPropagation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether constants should be parameterized. Parameterizing constants
        /// would result in better performance with Entity framework.
        /// </summary>
        /// <value>The default value is <c>true</c>.</value>
        public bool EnableConstantParameterization
        {
            get
            {
                return _querySettings.EnableConstantParameterization;
            }
            set
            {
                _querySettings.EnableConstantParameterization = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether queries with expanded navigations should be formulated
        /// to encourage correlated subquery results to be buffered.
        /// Buffering correlated subquery results can reduce the number of queries from N + 1 to 2
        /// by buffering results from the subquery.
        /// </summary>
        /// <value>The default value is <c>false</c>.</value>
        public bool EnableCorrelatedSubqueryBuffering
        {
            get
            {
                return _querySettings.EnableCorrelatedSubqueryBuffering;
            }
            set
            {
                _querySettings.EnableCorrelatedSubqueryBuffering = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth of the Any or All elements nested inside the query. This limit helps prevent
        /// Denial of Service attacks.
        /// </summary>
        /// <value>
        /// The maximum depth of the Any or All elements nested inside the query. The default value is 1.
        /// </value>
        public int MaxAnyAllExpressionDepth
        {
            get
            {
                return _validationSettings.MaxAnyAllExpressionDepth;
            }
            set
            {
                _validationSettings.MaxAnyAllExpressionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of nodes inside the $filter syntax tree.
        /// </summary>
        /// <value>The default value is 100.</value>
        public int MaxNodeCount
        {
            get
            {
                return _validationSettings.MaxNodeCount;
            }
            set
            {
                _validationSettings.MaxNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of query results to send back to clients.
        /// </summary>
        /// <value>
        /// The maximum number of query results to send back to clients.
        /// </value>
        public int PageSize
        {
            get
            {
                return _querySettings.PageSize ?? default(int);
            }
            set
            {
                _querySettings.PageSize = value;
            }
        }

        /// <summary>
        /// Honor $filter inside $expand of non-collection navigation property.
        /// The expanded property is only populated when the filter evaluates to true.
        /// This setting is false by default.
        /// </summary>
        public bool HandleReferenceNavigationPropertyExpandFilter
        {
            get
            {
                return _querySettings.HandleReferenceNavigationPropertyExpandFilter;
            }
            set
            {
                _querySettings.HandleReferenceNavigationPropertyExpandFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the query parameters that are allowed in queries.
        /// </summary>
        /// <value>The default includes all query options: $filter, $skip, $top, $orderby, $expand, $select, $count,
        /// $format, $skiptoken and $deltatoken.</value>
        public AllowedQueryOptions AllowedQueryOptions
        {
            get
            {
                return _validationSettings.AllowedQueryOptions;
            }
            set
            {
                _validationSettings.AllowedQueryOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed functions used in the $filter query. Supported
        /// functions include the following:
        /// <list type="definition">
        /// <item>
        /// <term>String related:</term>
        /// <description>contains, endswith, startswith, length, indexof, substring, tolower, toupper, trim,
        /// concat e.g. ~/Customers?$filter=length(CompanyName) eq 19</description>
        /// </item>
        /// <item>
        /// <term>DateTime related:</term>
        /// <description>year, month, day, hour, minute, second, fractionalseconds, date, time
        /// e.g. ~/Employees?$filter=year(BirthDate) eq 1971</description>
        /// </item>
        /// <item>
        /// <term>Math related:</term>
        /// <description>round, floor, ceiling</description>
        /// </item>
        /// <item>
        /// <term>Type related:</term>
        /// <description>isof, cast</description>
        /// </item>
        /// <item>
        /// <term>Collection related:</term>
        /// <description>any, all</description>
        /// </item>
        /// </list>
        /// </summary>
        public AllowedFunctions AllowedFunctions
        {
            get
            {
                return _validationSettings.AllowedFunctions;
            }
            set
            {
                _validationSettings.AllowedFunctions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed arithmetic operators including 'add', 'sub', 'mul',
        /// 'div', 'mod'.
        /// </summary>
        public AllowedArithmeticOperators AllowedArithmeticOperators
        {
            get
            {
                return _validationSettings.AllowedArithmeticOperators;
            }
            set
            {
                _validationSettings.AllowedArithmeticOperators = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed logical Operators such as 'eq', 'ne', 'gt', 'ge',
        /// 'lt', 'le', 'and', 'or', 'not'.
        /// </summary>
        public AllowedLogicalOperators AllowedLogicalOperators
        {
            get
            {
                return _validationSettings.AllowedLogicalOperators;
            }
            set
            {
                _validationSettings.AllowedLogicalOperators = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets a string with comma separated list of property names. The queryable result can only be
        /// ordered by those properties defined in this list.</para>
        ///
        /// <para>Note, by default this string is null, which means it can be ordered by any property.</para>
        ///
        /// <para>For example, setting this value to null or empty string means that we allow ordering the queryable
        /// result by any properties. Setting this value to "Name" means we only allow queryable result to be ordered
        /// by Name property.</para>
        /// </summary>
        public string AllowedOrderByProperties
        {
            get
            {
                return _allowedOrderByProperties;
            }
            set
            {
                _allowedOrderByProperties = value;

                if (String.IsNullOrEmpty(value))
                {
                    _validationSettings.AllowedOrderByProperties.Clear();
                }
                else
                {
                    // now parse the value and set it to validationSettings
                    string[] properties = _allowedOrderByProperties.Split(CommaSeparator);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        _validationSettings.AllowedOrderByProperties.Add(properties[i].Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the max value of $skip that a client can request.
        /// </summary>
        public int MaxSkip
        {
            get
            {
                return _validationSettings.MaxSkip ?? default(int);
            }
            set
            {
                _validationSettings.MaxSkip = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        public int MaxTop
        {
            get
            {
                return _validationSettings.MaxTop ?? default(int);
            }
            set
            {
                _validationSettings.MaxTop = value;
            }
        }

        /// <summary>
        /// Gets or sets the max expansion depth for the $expand query option. To disable the maximum expansion depth
        /// check, set this property to 0.
        /// </summary>
        public int MaxExpansionDepth
        {
            get { return _validationSettings.MaxExpansionDepth; }
            set
            {
                _validationSettings.MaxExpansionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of expressions that can be present in the $orderby.
        /// </summary>
        public int MaxOrderByNodeCount
        {
            get { return _validationSettings.MaxOrderByNodeCount; }
            set
            {
                _validationSettings.MaxOrderByNodeCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }
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
                throw Error.ArgumentNull("actionExecutedContext");
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
                StatusCodeResult statusCodeResult = actionExecutedContext.Result as StatusCodeResult;
                if (statusCodeResult == null || IsSuccessStatusCode(statusCodeResult.StatusCode))
                {
                    ObjectResult responseContent = actionExecutedContext.Result as ObjectResult;
                    if (responseContent != null)
                    {
                        //throw Error.Argument("actionExecutedContext", SRResources.QueryingRequiresObjectContent,
                        //    actionExecutedContext.Result.GetType().FullName);

                        // Get collection from SingleResult.
                        IQueryable singleResultCollection = null;
                        SingleResult singleResult = responseContent.Value as SingleResult;
                        if (singleResult != null)
                        {
                            // This could be a SingleResult, which has the property Queryable.
                            // But it could be a SingleResult() or SingleResult<T>. Sort by number of parameters
                            // on the property and get the one with the most parameters.
                            PropertyInfo propInfo = responseContent.Value.GetType().GetProperties()
                                .OrderBy(p => p.GetIndexParameters().Count())
                                .Where(p => p.Name.Equals("Queryable"))
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
        /// <param name="createQueryOptionFunction">A function used to create and validate query options.</param>
        /// <param name="createResponseAction">An action used to create a response.</param>
        /// <param name="createErrorAction">A function used to generate error response.</param>
        private object OnActionExecuted(
            ActionExecutedContext actionExecutedContext,
            object responseValue,
            IQueryable singleResultCollection,
            ControllerActionDescriptor actionDescriptor,
            HttpRequest request)
        {
            //if (!_querySettings.PageSize.HasValue && responseValue != null)
            //{
            //    GetModelBoundPageSize(responseValue, singleResultCollection, actionDescriptor, modelFunction, request.Context.Path, createErrorAction);
            //}

            //// Apply the query if there are any query options, if there is a page size set, in the case of
            //// SingleResult or in the case of $count request.
            bool shouldApplyQuery = true;
            // bool shouldApplyQuery = responseValue != null &&
            //    request.RequestUri != null &&
            //    (!String.IsNullOrWhiteSpace(request.RequestUri.Query) ||
            //    _querySettings.PageSize.HasValue ||
            //    _querySettings.ModelBoundPageSize.HasValue ||
            //    singleResultCollection != null ||
            //    request.IsCountRequest() ||
            //    ContainsAutoSelectExpandProperty(responseValue, singleResultCollection, actionDescriptor, modelFunction, request.Context.Path));

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
        /// <param name="modelFunction">A function to get the model.</param>
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
        /// Create and validate a new instance of <see cref="ODataQueryOptions"/> from a query and context.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryContext">The query context.</param>
        /// <returns></returns>
        private ODataQueryOptions CreateAndValidateQueryOptions(HttpRequest request, ODataQueryContext queryContext)
        {
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
        /// Get the ODaya query context.
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

                enumerable = singleResultCollection as IEnumerable;
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Response disposed after being sent.")]
        public virtual void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            IQueryCollection queryParameters = request.Query;
            foreach (var kvp in queryParameters)
            {
                if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                     kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    // we don't support any custom query options that start with $
                    // this should be caught be OnActionExecuted().
                    throw new ArgumentOutOfRangeException(kvp.Key);
                }
            }

            queryOptions.Validate(_validationSettings);
        }

        /// <summary>
        /// Gets the EDM model for the given type and request.Override this method to customize the EDM model used for
        /// querying.
        /// </summary>
        /// <param name = "elementClrType" > The CLR type to retrieve a model for.</param>
        /// <param name = "request" > The request message to retrieve a model for.</param>
        /// <param name = "actionDescriptor" > The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public virtual IEdmModel GetModel(
            Type elementClrType,
            HttpRequest request,
            ActionDescriptor actionDescriptor)
        {
            // Get model for the request
            IEdmModel model = request.GetModel();

            if (model == EdmCoreModel.Instance || model.GetEdmType(elementClrType) == null)
            {
                // user has not configured anything or has registered a model without the element type
                // let's create one just for this type and cache it in the action descriptor
                model = actionDescriptor.GetEdmModel(request, elementClrType);
            }

            Contract.Assert(model != null);
            return model;
        }
    }
}
