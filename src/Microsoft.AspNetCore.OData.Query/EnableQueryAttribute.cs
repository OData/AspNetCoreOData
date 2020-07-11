// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Validator;

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

                        //// Execution the action.
                        //object queryResult = OnActionExecuted(
                        //    responseContent.Value,
                        //    singleResultCollection,
                        //    new WebApiActionDescriptor(actionDescriptor as ControllerActionDescriptor),
                        //    new WebApiRequestMessage(request),
                        //    (elementClrType) => GetModel(elementClrType, request, actionDescriptor),
                        //    (queryContext) => CreateAndValidateQueryOptions(request, queryContext),
                        //    (statusCode) => actionExecutedContext.Result = new StatusCodeResult((int)statusCode),
                        //    (statusCode, message, exception) => actionExecutedContext.Result = CreateBadRequestResult(message, exception));

                        //if (queryResult != null)
                        //{
                        //    responseContent.Value = queryResult;
                        //}
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
        /// <param name="responseValue">The response content value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <param name="request">The internal request.</param>
        /// <param name="modelFunction">A function to get the model.</param>
        /// <param name="createQueryOptionFunction">A function used to create and validate query options.</param>
        /// <param name="createResponseAction">An action used to create a response.</param>
        /// <param name="createErrorAction">A function used to generate error response.</param>
        private object OnActionExecuted(
            object responseValue,
            IQueryable singleResultCollection,
            ActionDescriptor actionDescriptor,
            HttpRequest request,
      //      Func<ODataQueryContext, ODataQueryOptions> createQueryOptionFunction,
            Action<HttpStatusCode> createResponseAction,
            Action<HttpStatusCode, string, Exception> createErrorAction)
        {
            //if (!_querySettings.PageSize.HasValue && responseValue != null)
            //{
            //    GetModelBoundPageSize(responseValue, singleResultCollection, actionDescriptor, modelFunction, request.Context.Path, createErrorAction);
            //}

            //// Apply the query if there are any query options, if there is a page size set, in the case of
            //// SingleResult or in the case of $count request.
            bool shouldApplyQuery = false;
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
                    object queryResult = null;
                    //object queryResult = ExecuteQuery(responseValue, singleResultCollection, actionDescriptor, modelFunction, request, createQueryOptionFunction);
                    //if (queryResult == null && (request.Context.Path == null || singleResultCollection != null))
                    //{
                    //    // This is the case in which a regular OData service uses the EnableQuery attribute.
                    //    // For OData services ODataNullValueMessageHandler should be plugged in for the service
                    //    // if this behavior is desired.
                    //    // For non OData services this behavior is equivalent as the one in the v3 version in order
                    //    // to reduce the friction when they decide to move to use the v4 EnableQueryAttribute.
                    //    createResponseAction(HttpStatusCode.NotFound);
                    //}

                    returnValue = queryResult;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.QueryParameterNotSupported, e.Message),
                        e);
                }
                catch (NotImplementedException e)
                {
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                        e);
                }
                catch (NotSupportedException e)
                {
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                        e);
                }
                catch (InvalidOperationException e)
                {
                    // Will also catch ODataException here because ODataException derives from InvalidOperationException.
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                        e);
                }
            }

            return returnValue;
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
    }
}
