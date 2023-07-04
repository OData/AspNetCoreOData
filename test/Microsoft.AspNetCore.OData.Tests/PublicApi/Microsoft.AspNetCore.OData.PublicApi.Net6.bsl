[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.ODataApplicationBuilderExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseODataBatching (Microsoft.AspNetCore.Builder.IApplicationBuilder app)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseODataQueryRequest (Microsoft.AspNetCore.Builder.IApplicationBuilder app)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseODataRouteDebug (Microsoft.AspNetCore.Builder.IApplicationBuilder app)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseODataRouteDebug (Microsoft.AspNetCore.Builder.IApplicationBuilder app, string routePattern)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.ODataMvcBuilderExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IMvcBuilder AddOData (Microsoft.Extensions.DependencyInjection.IMvcBuilder builder)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IMvcBuilder AddOData (Microsoft.Extensions.DependencyInjection.IMvcBuilder builder, System.Action`1[[Microsoft.AspNetCore.OData.ODataOptions]] setupAction)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IMvcBuilder AddOData (Microsoft.Extensions.DependencyInjection.IMvcBuilder builder, System.Action`2[[Microsoft.AspNetCore.OData.ODataOptions],[System.IServiceProvider]] setupAction)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.ODataMvcCoreBuilderExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder AddOData (Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder builder)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder AddOData (Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder builder, System.Action`1[[Microsoft.AspNetCore.OData.ODataOptions]] setupAction)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder AddOData (Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder builder, System.Action`2[[Microsoft.AspNetCore.OData.ODataOptions],[System.IServiceProvider]] setupAction)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.ODataServiceCollectionExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddODataQueryFilter (Microsoft.Extensions.DependencyInjection.IServiceCollection services)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddODataQueryFilter (Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.AspNetCore.Mvc.Filters.IActionFilter queryFilter)
}

public sealed class Microsoft.AspNetCore.OData.ODataUriFunctions {
	public static void AddCustomUriFunction (string functionName, Microsoft.OData.UriParser.FunctionSignatureWithReturnType functionSignature, System.Reflection.MethodInfo methodInfo)
	public static bool RemoveCustomUriFunction (string functionName, Microsoft.OData.UriParser.FunctionSignatureWithReturnType functionSignature, System.Reflection.MethodInfo methodInfo)
}

public class Microsoft.AspNetCore.OData.ODataJsonOptionsSetup : IConfigureOptions`1 {
	public ODataJsonOptionsSetup ()

	public virtual void Configure (Microsoft.AspNetCore.Mvc.JsonOptions options)
}

public class Microsoft.AspNetCore.OData.ODataMvcOptionsSetup : IConfigureOptions`1 {
	public ODataMvcOptionsSetup ()

	public virtual void Configure (Microsoft.AspNetCore.Mvc.MvcOptions options)
}

public class Microsoft.AspNetCore.OData.ODataOptions {
	public ODataOptions ()

	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Routing.Conventions.IODataControllerActionConvention]] Conventions  { public get; }
	bool EnableAttributeRouting  { public get; public set; }
	bool EnableContinueOnErrorHeader  { public get; public set; }
	bool EnableNoDollarQueryOptions  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.DefaultQueryConfigurations QueryConfigurations  { public get; }
	[
	ObsoleteAttribute(),
	]
	Microsoft.OData.ModelBuilder.Config.DefaultQuerySettings QuerySettings  { public get; }

	[
	TupleElementNamesAttribute(),
	]
	System.Collections.Generic.IDictionary`2[[System.String],[System.ValueTuple`2[[Microsoft.OData.Edm.IEdmModel],[System.IServiceProvider]]]] RouteComponents  { public get; }

	Microsoft.AspNetCore.OData.Routing.ODataRouteOptions RouteOptions  { public get; }
	System.TimeZoneInfo TimeZone  { public get; public set; }
	Microsoft.OData.ODataUrlKeyDelimiter UrlKeyDelimiter  { public get; public set; }

	public Microsoft.AspNetCore.OData.ODataOptions AddRouteComponents (Microsoft.OData.Edm.IEdmModel model)
	public Microsoft.AspNetCore.OData.ODataOptions AddRouteComponents (Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Batch.ODataBatchHandler batchHandler)
	public Microsoft.AspNetCore.OData.ODataOptions AddRouteComponents (string routePrefix, Microsoft.OData.Edm.IEdmModel model)
	public Microsoft.AspNetCore.OData.ODataOptions AddRouteComponents (string routePrefix, Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Batch.ODataBatchHandler batchHandler)
	public Microsoft.AspNetCore.OData.ODataOptions AddRouteComponents (string routePrefix, Microsoft.OData.Edm.IEdmModel model, System.Action`1[[Microsoft.Extensions.DependencyInjection.IServiceCollection]] configureServices)
	public Microsoft.AspNetCore.OData.ODataOptions AddRouteComponents (string routePrefix, Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.ODataVersion version, System.Action`1[[Microsoft.Extensions.DependencyInjection.IServiceCollection]] configureServices)
	public Microsoft.AspNetCore.OData.ODataOptions Count ()
	public Microsoft.AspNetCore.OData.ODataOptions EnableQueryFeatures (params System.Nullable`1[[System.Int32]] maxTopValue)
	public Microsoft.AspNetCore.OData.ODataOptions Expand ()
	public Microsoft.AspNetCore.OData.ODataOptions Filter ()
	public System.IServiceProvider GetRouteServices (string routePrefix)
	public Microsoft.AspNetCore.OData.ODataOptions OrderBy ()
	public Microsoft.AspNetCore.OData.ODataOptions Select ()
	public Microsoft.AspNetCore.OData.ODataOptions SetMaxTop (System.Nullable`1[[System.Int32]] maxTopValue)
	public Microsoft.AspNetCore.OData.ODataOptions SkipToken ()
}

public class Microsoft.AspNetCore.OData.ODataOptionsSetup : IConfigureOptions`1 {
	public ODataOptionsSetup (Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.AspNetCore.OData.Routing.Parser.IODataPathTemplateParser parser)

	public virtual void Configure (Microsoft.AspNetCore.OData.ODataOptions options)
}

public interface Microsoft.AspNetCore.OData.Abstracts.IETagHandler {
	Microsoft.Net.Http.Headers.EntityTagHeaderValue CreateETag (System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] properties, params System.TimeZoneInfo timeZoneInfo)
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ParseETag (Microsoft.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
}

public interface Microsoft.AspNetCore.OData.Abstracts.IODataBatchFeature {
	System.Nullable`1[[System.Guid]] BatchId  { public abstract get; public abstract set; }
	System.Nullable`1[[System.Guid]] ChangeSetId  { public abstract get; public abstract set; }
	string ContentId  { public abstract get; public abstract set; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ContentIdMapping  { public abstract get; }
}

public interface Microsoft.AspNetCore.OData.Abstracts.IODataFeature {
	Microsoft.OData.UriParser.Aggregation.ApplyClause ApplyClause  { public abstract get; public abstract set; }
	string BaseAddress  { public abstract get; public abstract set; }
	Microsoft.AspNetCore.Routing.RouteValueDictionary BatchRouteData  { public abstract get; }
	System.Uri DeltaLink  { public abstract get; public abstract set; }
	System.Net.EndPoint Endpoint  { public abstract get; public abstract set; }
	Microsoft.OData.Edm.IEdmModel Model  { public abstract get; public abstract set; }
	System.Uri NextLink  { public abstract get; public abstract set; }
	Microsoft.OData.UriParser.ODataPath Path  { public abstract get; public abstract set; }
	Microsoft.Extensions.DependencyInjection.IServiceScope RequestScope  { public abstract get; public abstract set; }
	string RoutePrefix  { public abstract get; public abstract set; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] RoutingConventionsStore  { public abstract get; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public abstract get; public abstract set; }
	System.IServiceProvider Services  { public abstract get; public abstract set; }
	System.Nullable`1[[System.Int64]] TotalCount  { public abstract get; public abstract set; }
	System.Func`1[[System.Int64]] TotalCountFunc  { public abstract get; public abstract set; }
}

[
AttributeUsageAttribute(),
]
public class Microsoft.AspNetCore.OData.Abstracts.ETagActionFilterAttribute : Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute, IActionFilter, IAsyncActionFilter, IAsyncResultFilter, IFilterMetadata, IOrderedFilter, IResultFilter {
	public ETagActionFilterAttribute ()

	public virtual void OnActionExecuted (Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext actionExecutedContext)
}

public class Microsoft.AspNetCore.OData.Abstracts.HttpRequestScope {
	public HttpRequestScope ()

	Microsoft.AspNetCore.Http.HttpRequest HttpRequest  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public class Microsoft.AspNetCore.OData.Abstracts.NonValidatingParameterBindingAttribute : Microsoft.AspNetCore.Mvc.ModelBinderAttribute, IBinderTypeProviderMetadata, IBindingSourceMetadata, IModelNameProvider, IPropertyValidationFilter {
	public NonValidatingParameterBindingAttribute ()

	Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource BindingSource  { public virtual get; }

	public virtual bool ShouldValidateEntry (Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationEntry entry, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationEntry parentEntry)
}

public class Microsoft.AspNetCore.OData.Abstracts.ODataBatchFeature : IODataBatchFeature {
	public ODataBatchFeature ()

	System.Nullable`1[[System.Guid]] BatchId  { public virtual get; public virtual set; }
	System.Nullable`1[[System.Guid]] ChangeSetId  { public virtual get; public virtual set; }
	string ContentId  { public virtual get; public virtual set; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ContentIdMapping  { public virtual get; }
}

public class Microsoft.AspNetCore.OData.Abstracts.ODataFeature : IODataFeature {
	public ODataFeature ()

	Microsoft.OData.UriParser.Aggregation.ApplyClause ApplyClause  { public virtual get; public virtual set; }
	string BaseAddress  { public virtual get; public virtual set; }
	Microsoft.AspNetCore.Routing.RouteValueDictionary BatchRouteData  { public virtual get; }
	System.Uri DeltaLink  { public virtual get; public virtual set; }
	System.Net.EndPoint Endpoint  { public virtual get; public virtual set; }
	Microsoft.OData.Edm.IEdmModel Model  { public virtual get; public virtual set; }
	System.Uri NextLink  { public virtual get; public virtual set; }
	Microsoft.OData.UriParser.ODataPath Path  { public virtual get; public virtual set; }
	Microsoft.Extensions.DependencyInjection.IServiceScope RequestScope  { public virtual get; public virtual set; }
	string RoutePrefix  { public virtual get; public virtual set; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] RoutingConventionsStore  { public virtual get; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public virtual get; public virtual set; }
	System.IServiceProvider Services  { public virtual get; public virtual set; }
	System.Nullable`1[[System.Int64]] TotalCount  { public virtual get; public virtual set; }
	System.Func`1[[System.Int64]] TotalCountFunc  { public virtual get; public virtual set; }
}

public abstract class Microsoft.AspNetCore.OData.Batch.ODataBatchHandler {
	protected ODataBatchHandler ()

	Microsoft.OData.ODataMessageQuotas MessageQuotas  { public get; }
	string PrefixName  { public get; public set; }

	public virtual System.Threading.Tasks.Task CreateResponseMessageAsync (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] responses, Microsoft.AspNetCore.Http.HttpRequest request)
	public virtual System.Uri GetBaseUri (Microsoft.AspNetCore.Http.HttpRequest request)
	public abstract System.Threading.Tasks.Task ProcessBatchAsync (Microsoft.AspNetCore.Http.HttpContext context, Microsoft.AspNetCore.Http.RequestDelegate nextHandler)
	public virtual System.Threading.Tasks.Task`1[[System.Boolean]] ValidateRequest (Microsoft.AspNetCore.Http.HttpRequest request)
}

public abstract class Microsoft.AspNetCore.OData.Batch.ODataBatchRequestItem {
	protected ODataBatchRequestItem ()

	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ContentIdToLocationMapping  { public get; public set; }

	public abstract System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (Microsoft.AspNetCore.Http.RequestDelegate handler)
	[
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task SendRequestAsync (Microsoft.AspNetCore.Http.RequestDelegate handler, Microsoft.AspNetCore.Http.HttpContext context, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] contentIdToLocationMapping)
}

public abstract class Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem {
	protected ODataBatchResponseItem ()

	internal abstract bool IsResponseSuccessful ()
	[
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task WriteMessageAsync (Microsoft.OData.ODataBatchWriter writer, Microsoft.AspNetCore.Http.HttpContext context)

	public abstract System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Batch.HttpRequestExtensions {
	[
	ExtensionAttribute(),
	]
	public static void CopyAbsoluteUrl (Microsoft.AspNetCore.Http.HttpRequest request, System.Uri uri)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataMessageReader GetODataMessageReader (Microsoft.AspNetCore.Http.HttpRequest request, System.IServiceProvider requestContainer)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Batch.ODataBatchHttpRequestExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Nullable`1[[System.Guid]] GetODataBatchId (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static System.Nullable`1[[System.Guid]] GetODataChangeSetId (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static string GetODataContentId (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static System.Collections.Generic.IDictionary`2[[System.String],[System.String]] GetODataContentIdMapping (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static bool IsODataBatchRequest (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static void SetODataBatchId (Microsoft.AspNetCore.Http.HttpRequest request, System.Guid batchId)

	[
	ExtensionAttribute(),
	]
	public static void SetODataChangeSetId (Microsoft.AspNetCore.Http.HttpRequest request, System.Guid changeSetId)

	[
	ExtensionAttribute(),
	]
	public static void SetODataContentId (Microsoft.AspNetCore.Http.HttpRequest request, string contentId)

	[
	ExtensionAttribute(),
	]
	public static void SetODataContentIdMapping (Microsoft.AspNetCore.Http.HttpRequest request, System.Collections.Generic.IDictionary`2[[System.String],[System.String]] contentIdMapping)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Batch.ODataBatchReaderExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.Http.HttpContext]] ReadChangeSetOperationRequestAsync (Microsoft.OData.ODataBatchReader reader, Microsoft.AspNetCore.Http.HttpContext context, System.Guid batchId, System.Guid changeSetId, System.Threading.CancellationToken cancellationToken)

	[
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[Microsoft.AspNetCore.Http.HttpContext]]]] ReadChangeSetRequestAsync (Microsoft.OData.ODataBatchReader reader, Microsoft.AspNetCore.Http.HttpContext context, System.Guid batchId, System.Threading.CancellationToken cancellationToken)

	[
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.Http.HttpContext]] ReadOperationRequestAsync (Microsoft.OData.ODataBatchReader reader, Microsoft.AspNetCore.Http.HttpContext context, System.Guid batchId, System.Threading.CancellationToken cancellationToken)
}

public class Microsoft.AspNetCore.OData.Batch.ChangeSetRequestItem : Microsoft.AspNetCore.OData.Batch.ODataBatchRequestItem {
	public ChangeSetRequestItem (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.Http.HttpContext]] contexts)

	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.Http.HttpContext]] Contexts  { public get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (Microsoft.AspNetCore.Http.RequestDelegate handler)
}

public class Microsoft.AspNetCore.OData.Batch.ChangeSetResponseItem : Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem {
	public ChangeSetResponseItem (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.Http.HttpContext]] contexts)

	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.Http.HttpContext]] Contexts  { public get; }

	internal virtual bool IsResponseSuccessful ()
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer)
}

public class Microsoft.AspNetCore.OData.Batch.DefaultODataBatchHandler : Microsoft.AspNetCore.OData.Batch.ODataBatchHandler {
	public DefaultODataBatchHandler ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]]]] ExecuteRequestMessagesAsync (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchRequestItem]] requests, Microsoft.AspNetCore.Http.RequestDelegate handler)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchRequestItem]]]] ParseBatchRequestsAsync (Microsoft.AspNetCore.Http.HttpContext context)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ProcessBatchAsync (Microsoft.AspNetCore.Http.HttpContext context, Microsoft.AspNetCore.Http.RequestDelegate nextHandler)
}

public class Microsoft.AspNetCore.OData.Batch.ODataBatchContent {
	public ODataBatchContent (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] responses, System.IServiceProvider requestContainer)
	public ODataBatchContent (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] responses, System.IServiceProvider requestContainer, string contentType)

	Microsoft.AspNetCore.Http.IHeaderDictionary Headers  { public get; }
	System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] Responses  { public get; }

	public System.Threading.Tasks.Task SerializeToStreamAsync (System.IO.Stream stream)
}

public class Microsoft.AspNetCore.OData.Batch.ODataBatchMiddleware {
	public ODataBatchMiddleware (System.IServiceProvider serviceProvider, Microsoft.AspNetCore.Http.RequestDelegate next)

	[
	AsyncStateMachineAttribute(),
	]
	public System.Threading.Tasks.Task Invoke (Microsoft.AspNetCore.Http.HttpContext context)
}

public class Microsoft.AspNetCore.OData.Batch.OperationRequestItem : Microsoft.AspNetCore.OData.Batch.ODataBatchRequestItem {
	public OperationRequestItem (Microsoft.AspNetCore.Http.HttpContext context)

	Microsoft.AspNetCore.Http.HttpContext Context  { public get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (Microsoft.AspNetCore.Http.RequestDelegate handler)
}

public class Microsoft.AspNetCore.OData.Batch.OperationResponseItem : Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem {
	public OperationResponseItem (Microsoft.AspNetCore.Http.HttpContext context)

	Microsoft.AspNetCore.Http.HttpContext Context  { public get; }

	internal virtual bool IsResponseSuccessful ()
	public virtual System.Threading.Tasks.Task WriteResponseAsync (Microsoft.OData.ODataBatchWriter writer)
}

public class Microsoft.AspNetCore.OData.Batch.UnbufferedODataBatchHandler : Microsoft.AspNetCore.OData.Batch.ODataBatchHandler {
	public UnbufferedODataBatchHandler ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] ExecuteChangeSetAsync (Microsoft.OData.ODataBatchReader batchReader, System.Guid batchId, Microsoft.AspNetCore.Http.HttpRequest originalRequest, Microsoft.AspNetCore.Http.RequestDelegate handler)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] ExecuteOperationAsync (Microsoft.OData.ODataBatchReader batchReader, System.Guid batchId, Microsoft.AspNetCore.Http.HttpRequest originalRequest, Microsoft.AspNetCore.Http.RequestDelegate handler)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ProcessBatchAsync (Microsoft.AspNetCore.Http.HttpContext context, Microsoft.AspNetCore.Http.RequestDelegate nextHandler)
}

public enum Microsoft.AspNetCore.OData.Deltas.DeltaItemKind : int {
	DeletedResource = 1
	DeltaDeletedLink = 2
	DeltaLink = 3
	Resource = 0
	Unknown = 4
}

public interface Microsoft.AspNetCore.OData.Deltas.IDelta : IDeltaSetItem {
	void Clear ()
	System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetDeltaNestedNavigationProperties ()
	System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	bool TryGetPropertyType (string name, out System.Type& type)
	bool TryGetPropertyValue (string name, out System.Object& value)
	bool TrySetPropertyValue (string name, object value)
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaDeletedResource : IDelta, IDeltaSetItem {
	System.Uri Id  { public abstract get; public abstract set; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaSet : IEnumerable, ICollection`1, IEnumerable`1 {
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaSetItem {
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public abstract get; }
}

public interface Microsoft.AspNetCore.OData.Deltas.ITypedDelta {
	System.Type ExpectedClrType  { public abstract get; }
	System.Type StructuredType  { public abstract get; }
}

public abstract class Microsoft.AspNetCore.OData.Deltas.Delta : System.Dynamic.DynamicObject, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem {
	protected Delta ()

	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public abstract get; }

	public abstract void Clear ()
	public abstract System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public abstract System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetDeltaNestedNavigationProperties ()
	public abstract System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public virtual bool TryGetMember (System.Dynamic.GetMemberBinder binder, out System.Object& result)
	public abstract bool TryGetPropertyType (string name, out System.Type& type)
	public abstract bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetMember (System.Dynamic.SetMemberBinder binder, object value)
	public abstract bool TrySetPropertyValue (string name, object value)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Deltas.Delta`1 : Microsoft.AspNetCore.OData.Deltas.Delta, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, ITypedDelta {
	public Delta`1 ()
	public Delta`1 (System.Type structuralType)
	public Delta`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties)
	public Delta`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo)

	System.Type ExpectedClrType  { public virtual get; }
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
	System.Type StructuredType  { public virtual get; }
	System.Collections.Generic.IList`1[[System.String]] UpdatableProperties  { public get; }

	public virtual void Clear ()
	public void CopyChangedValues (T original)
	public void CopyUnchangedValues (T original)
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public virtual System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetDeltaNestedNavigationProperties ()
	public T GetInstance ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public void Patch (T original)
	public void Put (T original)
	public virtual bool TryGetPropertyType (string name, out System.Type& type)
	public virtual bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetPropertyValue (string name, object value)
}

public class Microsoft.AspNetCore.OData.Deltas.DeltaDeletedResource`1 : Delta`1, IDynamicMetaObjectProvider, IDelta, IDeltaDeletedResource, IDeltaSetItem, ITypedDelta {
	public DeltaDeletedResource`1 ()
	public DeltaDeletedResource`1 (System.Type structuralType)
	public DeltaDeletedResource`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties)
	public DeltaDeletedResource`1 (System.Type structuralType, System.Collections.Generic.IEnumerable`1[[System.String]] updatableProperties, System.Reflection.PropertyInfo dynamicDictionaryPropertyInfo)

	System.Uri Id  { public virtual get; public virtual set; }
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Deltas.DeltaSet`1 : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNetCore.OData.Deltas.IDeltaSetItem]], ICollection, IEnumerable, IList, IDeltaSet, ITypedDelta, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public DeltaSet`1 ()

	System.Type ExpectedClrType  { public virtual get; }
	System.Type StructuredType  { public virtual get; }
}

public interface Microsoft.AspNetCore.OData.Edm.IODataTypeMapper {
	System.Type GetClrPrimitiveType (Microsoft.OData.Edm.IEdmPrimitiveType primitiveType, bool nullable)
	System.Type GetClrType (Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmType edmType, bool nullable, Microsoft.OData.ModelBuilder.IAssemblyResolver assembliesResolver)
	Microsoft.OData.Edm.IEdmPrimitiveTypeReference GetEdmPrimitiveType (System.Type clrType)
	Microsoft.OData.Edm.IEdmTypeReference GetEdmTypeReference (Microsoft.OData.Edm.IEdmModel edmModel, System.Type clrType)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Edm.EdmModelAnnotationExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.Edm.IEdmPathExpression]]]] GetAlternateKeys (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmEntityType entityType)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ModelBuilder.ClrEnumMemberAnnotation GetClrEnumMemberAnnotation (Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmEnumType enumType)

	[
	ExtensionAttribute(),
	]
	public static string GetClrPropertyName (Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmProperty edmProperty)

	[
	ExtensionAttribute(),
	]
	public static System.Collections.Generic.IEnumerable`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] GetConcurrencyProperties (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	[
	ExtensionAttribute(),
	]
	public static System.Reflection.PropertyInfo GetDynamicPropertyDictionary (Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmStructuredType edmType)

	[
	ExtensionAttribute(),
	]
	public static string GetModelName (Microsoft.OData.Edm.IEdmModel model)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Edm.IODataTypeMapper GetTypeMapper (Microsoft.OData.Edm.IEdmModel model)

	[
	ExtensionAttribute(),
	]
	public static void SetModelName (Microsoft.OData.Edm.IEdmModel model, string name)

	[
	ExtensionAttribute(),
	]
	public static void SetTypeMapper (Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Edm.IODataTypeMapper mapper)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Edm.EdmModelLinkBuilderExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Edm.NavigationSourceLinkBuilderAnnotation GetNavigationSourceLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Edm.OperationLinkBuilder GetOperationLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmOperation operation)

	[
	ExtensionAttribute(),
	]
	public static void HasEditLink (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1[[System.Uri]] editLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void HasIdLink (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1[[System.Uri]] idLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void HasNavigationPropertyLink (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Edm.NavigationLinkBuilder linkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void HasReadLink (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1[[System.Uri]] readLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void SetNavigationSourceLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.AspNetCore.OData.Edm.NavigationSourceLinkBuilderAnnotation navigationSourceLinkBuilder)

	[
	ExtensionAttribute(),
	]
	public static void SetOperationLinkBuilder (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.OData.Edm.OperationLinkBuilder operationLinkBuilder)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Edm.IODataTypeMapperExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Type GetClrType (Microsoft.AspNetCore.OData.Edm.IODataTypeMapper mapper, Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmTypeReference edmType)

	[
	ExtensionAttribute(),
	]
	public static System.Type GetClrType (Microsoft.AspNetCore.OData.Edm.IODataTypeMapper mapper, Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.OData.ModelBuilder.IAssemblyResolver assembliesResolver)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.AspNetCore.OData.Edm.IODataTypeMapper mapper, Microsoft.OData.Edm.IEdmModel edmModel, System.Type clrType)

	[
	ExtensionAttribute(),
	]
	public static System.Type GetPrimitiveType (Microsoft.AspNetCore.OData.Edm.IODataTypeMapper mapper, Microsoft.OData.Edm.IEdmPrimitiveTypeReference primitiveType)
}

public class Microsoft.AspNetCore.OData.Edm.CustomAggregateMethodAnnotation {
	public CustomAggregateMethodAnnotation ()

	public Microsoft.AspNetCore.OData.Edm.CustomAggregateMethodAnnotation AddMethod (string methodToken, System.Collections.Generic.IDictionary`2[[System.Type],[System.Reflection.MethodInfo]] methods)
	public bool GetMethodInfo (string methodToken, System.Type returnType, out System.Reflection.MethodInfo& methodInfo)
}

public class Microsoft.AspNetCore.OData.Edm.DefaultODataTypeMapper : IODataTypeMapper {
	public DefaultODataTypeMapper ()

	public virtual System.Type GetClrPrimitiveType (Microsoft.OData.Edm.IEdmPrimitiveType primitiveType, bool nullable)
	public virtual System.Type GetClrType (Microsoft.OData.Edm.IEdmModel edmModel, Microsoft.OData.Edm.IEdmType edmType, bool nullable, Microsoft.OData.ModelBuilder.IAssemblyResolver assembliesResolver)
	public virtual Microsoft.OData.Edm.IEdmPrimitiveTypeReference GetEdmPrimitiveType (System.Type clrType)
	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmTypeReference (Microsoft.OData.Edm.IEdmModel edmModel, System.Type clrType)
}

public class Microsoft.AspNetCore.OData.Edm.EntitySelfLinks {
	public EntitySelfLinks ()

	System.Uri EditLink  { public get; public set; }
	System.Uri IdLink  { public get; public set; }
	System.Uri ReadLink  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Edm.ModelNameAnnotation {
	public ModelNameAnnotation (string name)

	string ModelName  { public get; }
}

public class Microsoft.AspNetCore.OData.Edm.NavigationLinkBuilder {
	public NavigationLinkBuilder (System.Func`3[[Microsoft.AspNetCore.OData.Formatter.ResourceContext],[Microsoft.OData.Edm.IEdmNavigationProperty],[System.Uri]] navigationLinkFactory, bool followsConventions)

	System.Func`3[[Microsoft.AspNetCore.OData.Formatter.ResourceContext],[Microsoft.OData.Edm.IEdmNavigationProperty],[System.Uri]] Factory  { public get; }
	bool FollowsConventions  { public get; }
}

public class Microsoft.AspNetCore.OData.Edm.NavigationSourceLinkBuilderAnnotation {
	public NavigationSourceLinkBuilderAnnotation ()
	public NavigationSourceLinkBuilderAnnotation (Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.OData.Edm.IEdmModel model)

	Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1[[System.Uri]] EditLinkBuilder  { public get; public set; }
	Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1[[System.Uri]] IdLinkBuilder  { public get; public set; }
	Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1[[System.Uri]] ReadLinkBuilder  { public get; public set; }

	public void AddNavigationPropertyLinkBuilder (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Edm.NavigationLinkBuilder linkBuilder)
	public virtual System.Uri BuildEditLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext instanceContext, Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel metadataLevel, System.Uri idLink)
	public virtual Microsoft.AspNetCore.OData.Edm.EntitySelfLinks BuildEntitySelfLinks (Microsoft.AspNetCore.OData.Formatter.ResourceContext instanceContext, Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildIdLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext instanceContext, Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildNavigationLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext instanceContext, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel metadataLevel)
	public virtual System.Uri BuildReadLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext instanceContext, Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel metadataLevel, System.Uri editLink)
}

public class Microsoft.AspNetCore.OData.Edm.OperationLinkBuilder {
	public OperationLinkBuilder (System.Func`2[[Microsoft.AspNetCore.OData.Formatter.ResourceContext],[System.Uri]] linkFactory, bool followsConventions)
	public OperationLinkBuilder (System.Func`2[[Microsoft.AspNetCore.OData.Formatter.ResourceSetContext],[System.Uri]] linkFactory, bool followsConventions)

	bool FollowsConventions  { public get; }

	public virtual System.Uri BuildLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext context)
	public virtual System.Uri BuildLink (Microsoft.AspNetCore.OData.Formatter.ResourceSetContext context)
}

public class Microsoft.AspNetCore.OData.Edm.SelfLinkBuilder`1 {
	public SelfLinkBuilder`1 (Func`2 linkFactory, bool followsConventions)

	Func`2 Factory  { public get; }
	bool FollowsConventions  { public get; }
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.ActionModelExtensions {
	[
	ExtensionAttribute(),
	]
	public static void AddSelector (Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action, string httpMethods, string prefix, Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate path, params Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)

	[
	ExtensionAttribute(),
	]
	public static T GetAttribute (Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)

	[
	ExtensionAttribute(),
	]
	public static bool HasODataKeyParameter (Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action, Microsoft.OData.Edm.IEdmEntityType entityType, params bool enablePropertyNameCaseInsensitive, params string keyPrefix)

	[
	ExtensionAttribute(),
	]
	public static bool HasParameter (Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action, string parameterName)

	[
	ExtensionAttribute(),
	]
	public static bool IsODataIgnored (Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.ControllerModelExtensions {
	[
	ExtensionAttribute(),
	]
	public static T GetAttribute (Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)

	[
	ExtensionAttribute(),
	]
	public static bool HasAttribute (Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)

	[
	ExtensionAttribute(),
	]
	public static bool IsODataIgnored (Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.HttpContextExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Abstracts.IODataBatchFeature ODataBatchFeature (Microsoft.AspNetCore.Http.HttpContext httpContext)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Abstracts.IODataFeature ODataFeature (Microsoft.AspNetCore.Http.HttpContext httpContext)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.ODataOptions ODataOptions (Microsoft.AspNetCore.Http.HttpContext httpContext)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.HttpRequestExtensions {
	[
	ExtensionAttribute(),
	]
	public static void ClearRouteServices (Microsoft.AspNetCore.Http.HttpRequest request, params bool dispose)

	[
	ExtensionAttribute(),
	]
	public static string CreateETag (Microsoft.AspNetCore.Http.HttpRequest request, System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] properties, params System.TimeZoneInfo timeZone)

	[
	ExtensionAttribute(),
	]
	public static System.IServiceProvider CreateRouteServices (Microsoft.AspNetCore.Http.HttpRequest request, string routePrefix)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider GetDeserializerProvider (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Abstracts.IETagHandler GetETagHandler (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.Edm.IEdmModel GetModel (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GetNextPageLink (Microsoft.AspNetCore.Http.HttpRequest request, int pageSize, object instance, System.Func`2[[System.Object],[System.String]] objectToSkipTokenValue)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataVersion GetODataVersion (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataMessageReaderSettings GetReaderSettings (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static System.IServiceProvider GetRouteServices (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static System.TimeZoneInfo GetTimeZoneInfo (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataMessageWriterSettings GetWriterSettings (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static bool IsCountRequest (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static bool IsNoDollarQueryEnable (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Abstracts.IODataBatchFeature ODataBatchFeature (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Abstracts.IODataFeature ODataFeature (Microsoft.AspNetCore.Http.HttpRequest request)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.ODataOptions ODataOptions (Microsoft.AspNetCore.Http.HttpRequest request)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.HttpResponseExtensions {
	[
	ExtensionAttribute(),
	]
	public static bool IsSuccessStatusCode (Microsoft.AspNetCore.Http.HttpResponse response)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.LinkGeneratorHelpers {
	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.OData.UriParser.ODataPathSegment[] segments)

	[
	ExtensionAttribute(),
	]
	public static string CreateODataLink (Microsoft.AspNetCore.Http.HttpRequest request, System.Collections.Generic.IList`1[[Microsoft.OData.UriParser.ODataPathSegment]] segments)
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Extensions.SerializableErrorExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.ODataError CreateODataError (Microsoft.AspNetCore.Mvc.SerializableError serializableError)
}

public sealed class Microsoft.AspNetCore.OData.Extensions.SerializableErrorKeys {
	public static readonly string ErrorCodeKey = "ErrorCode"
	public static readonly string ExceptionMessageKey = "ExceptionMessage"
	public static readonly string ExceptionTypeKey = "ExceptionType"
	public static readonly string InnerExceptionKey = "InnerException"
	public static readonly string MessageDetailKey = "MessageDetail"
	public static readonly string MessageKey = "Message"
	public static readonly string MessageLanguageKey = "MessageLanguage"
	public static readonly string ModelStateKey = "ModelState"
	public static readonly string StackTraceKey = "StackTrace"
}

public enum Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel : int {
	Full = 1
	Minimal = 0
	None = 2
}

[
EditorBrowsableAttribute(),
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.LinkGenerationHelpers {
	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateActionLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext, Microsoft.OData.Edm.IEdmOperation action)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateActionLink (Microsoft.AspNetCore.OData.Formatter.ResourceSetContext resourceSetContext, Microsoft.OData.Edm.IEdmOperation action)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateFunctionLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext, Microsoft.OData.Edm.IEdmOperation function)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateFunctionLink (Microsoft.AspNetCore.OData.Formatter.ResourceSetContext resourceSetContext, Microsoft.OData.Edm.IEdmOperation function)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateNavigationPropertyLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, bool includeCast)

	[
	ExtensionAttribute(),
	]
	public static System.Uri GenerateSelfLink (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext, bool includeCast)
}

public sealed class Microsoft.AspNetCore.OData.Formatter.ODataInputFormatterFactory {
	public static System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.ODataInputFormatter]] Create ()
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.ODataOutputFormatterFactory {
	public static System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.ODataOutputFormatter]] Create ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.ODataActionParameters : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ODataActionParameters ()
}

public class Microsoft.AspNetCore.OData.Formatter.ODataInputFormatter : Microsoft.AspNetCore.Mvc.Formatters.TextInputFormatter, IApiRequestFormatMetadataProvider, IInputFormatter {
	public ODataInputFormatter (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.ODataPayloadKind]] payloadKinds)

	System.Func`2[[Microsoft.AspNetCore.Http.HttpRequest],[System.Uri]] BaseAddressFactory  { public get; public set; }

	public virtual bool CanRead (Microsoft.AspNetCore.Mvc.Formatters.InputFormatterContext context)
	public static System.Uri GetDefaultBaseAddress (Microsoft.AspNetCore.Http.HttpRequest request)
	public virtual System.Collections.Generic.IReadOnlyList`1[[System.String]] GetSupportedContentTypes (string contentType, System.Type objectType)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.Mvc.Formatters.InputFormatterResult]] ReadRequestBodyAsync (Microsoft.AspNetCore.Mvc.Formatters.InputFormatterContext context, System.Text.Encoding encoding)
}

public class Microsoft.AspNetCore.OData.Formatter.ODataOutputFormatter : Microsoft.AspNetCore.Mvc.Formatters.TextOutputFormatter, IApiResponseTypeMetadataProvider, IOutputFormatter, IMediaTypeMappingCollection {
	public ODataOutputFormatter (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.ODataPayloadKind]] payloadKinds)

	System.Func`2[[Microsoft.AspNetCore.Http.HttpRequest],[System.Uri]] BaseAddressFactory  { public get; public set; }
	System.Collections.Generic.ICollection`1[[Microsoft.AspNetCore.OData.Formatter.MediaType.MediaTypeMapping]] MediaTypeMappings  { public virtual get; }

	public virtual bool CanWriteResult (Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterCanWriteContext context)
	public static System.Uri GetDefaultBaseAddress (Microsoft.AspNetCore.Http.HttpRequest request)
	public virtual System.Collections.Generic.IReadOnlyList`1[[System.String]] GetSupportedContentTypes (string contentType, System.Type objectType)
	public virtual System.Threading.Tasks.Task WriteResponseBodyAsync (Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext context, System.Text.Encoding selectedEncoding)
	public virtual void WriteResponseHeaders (Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext context)
}

public class Microsoft.AspNetCore.OData.Formatter.ODataParameterValue {
	public static string ParameterValuePrefix = "DF908045-6922-46A0-82F2-2F6E7F43D1B1_"

	public ODataParameterValue (object paramValue, Microsoft.OData.Edm.IEdmTypeReference paramType)

	Microsoft.OData.Edm.IEdmTypeReference EdmType  { public get; }
	object Value  { public get; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.ODataUntypedActionParameters : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public ODataUntypedActionParameters (Microsoft.OData.Edm.IEdmAction action)

	Microsoft.OData.Edm.IEdmAction Action  { public get; }
}

public class Microsoft.AspNetCore.OData.Formatter.ResourceContext {
	public ResourceContext ()
	public ResourceContext (Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext serializerContext, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, object resourceInstance)

	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] DynamicComplexProperties  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; public set; }
	Microsoft.AspNetCore.OData.Formatter.Value.IEdmStructuredObject EdmObject  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	Microsoft.AspNetCore.Http.HttpRequest Request  { public get; public set; }
	object ResourceInstance  { public get; public set; }
	Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext SerializerContext  { public get; public set; }
	bool SkipExpensiveAvailabilityChecks  { public get; public set; }
	Microsoft.OData.Edm.IEdmStructuredType StructuredType  { public get; public set; }

	public object GetPropertyValue (string propertyName)
}

public class Microsoft.AspNetCore.OData.Formatter.ResourceSetContext {
	public ResourceSetContext ()

	Microsoft.OData.Edm.IEdmModel EdmModel  { public get; }
	Microsoft.OData.Edm.IEdmEntitySetBase EntitySetBase  { public get; public set; }
	Microsoft.AspNetCore.Http.HttpRequest Request  { public get; public set; }
	object ResourceSetInstance  { public get; public set; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.FromODataBodyAttribute : Microsoft.AspNetCore.Mvc.ModelBinderAttribute, IBinderTypeProviderMetadata, IBindingSourceMetadata, IModelNameProvider {
	public FromODataBodyAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.FromODataUriAttribute : Microsoft.AspNetCore.Mvc.ModelBinderAttribute, IBinderTypeProviderMetadata, IBindingSourceMetadata, IModelNameProvider {
	public FromODataUriAttribute ()
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNetCore.OData.Query.AllowedArithmeticOperators : int {
	Add = 1
	All = 31
	Divide = 8
	Modulo = 16
	Multiply = 4
	None = 0
	Subtract = 2
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNetCore.OData.Query.AllowedFunctions : int {
	All = 268435456
	AllDateTimeFunctions = 7010304
	AllFunctions = 1072365567
	AllMathFunctions = 58720256
	AllStringFunctions = 536871935
	Any = 134217728
	Cast = 1024
	Ceiling = 33554432
	Concat = 32
	Contains = 4
	Date = 4096
	Day = 32768
	EndsWith = 2
	Floor = 16777216
	FractionalSeconds = 4194304
	Hour = 131072
	IndexOf = 16
	IsOf = 67108864
	Length = 8
	MatchesPattern = 536870912
	Minute = 524288
	Month = 8192
	None = 0
	Round = 8388608
	Second = 2097152
	StartsWith = 1
	Substring = 64
	Time = 16384
	ToLower = 128
	ToUpper = 256
	Trim = 512
	Year = 2048
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNetCore.OData.Query.AllowedLogicalOperators : int {
	All = 1023
	And = 2
	Equal = 4
	GreaterThan = 16
	GreaterThanOrEqual = 32
	Has = 512
	LessThan = 64
	LessThanOrEqual = 128
	None = 0
	Not = 256
	NotEqual = 8
	Or = 1
}

[
FlagsAttribute(),
]
public enum Microsoft.AspNetCore.OData.Query.AllowedQueryOptions : int {
	All = 8191
	Apply = 1024
	Compute = 2048
	Count = 64
	DeltaToken = 512
	Expand = 2
	Filter = 1
	Format = 128
	None = 0
	OrderBy = 8
	Search = 4096
	Select = 4
	Skip = 32
	SkipToken = 256
	Supported = 7679
	Top = 16
}

public enum Microsoft.AspNetCore.OData.Query.HandleNullPropagationOption : int {
	Default = 0
	False = 2
	True = 1
}

public interface Microsoft.AspNetCore.OData.Query.IODataQueryRequestParser {
	bool CanParse (Microsoft.AspNetCore.Http.HttpRequest request)
	System.Threading.Tasks.Task`1[[System.String]] ParseAsync (Microsoft.AspNetCore.Http.HttpRequest request)
}

public abstract class Microsoft.AspNetCore.OData.Query.OrderByNode {
	protected OrderByNode (Microsoft.OData.UriParser.OrderByClause orderByClause)
	protected OrderByNode (Microsoft.OData.UriParser.OrderByDirection direction)

	Microsoft.OData.UriParser.OrderByDirection Direction  { public get; }

	public static System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Query.OrderByNode]] CreateCollection (Microsoft.OData.UriParser.OrderByClause orderByClause)
}

public abstract class Microsoft.AspNetCore.OData.Query.SkipTokenHandler {
	protected SkipTokenHandler ()

	public abstract IQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipTokenQueryOption, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public abstract System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipTokenQueryOption, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public abstract System.Uri GenerateNextPageLink (System.Uri baseUri, int pageSize, object instance, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext context)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Query.HttpRequestODataQueryExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Query.ETag GetETag (Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.Net.Http.Headers.EntityTagHeaderValue entityTagHeaderValue)

	[
	ExtensionAttribute(),
	]
	public static ETag`1 GetETag (Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.Net.Http.Headers.EntityTagHeaderValue entityTagHeaderValue)
}

public class Microsoft.AspNetCore.OData.Query.ApplyQueryOption {
	public ApplyQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.OData.UriParser.Aggregation.ApplyClause ApplyClause  { public get; }
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	System.Type ResultClrType  { public get; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
}

public class Microsoft.AspNetCore.OData.Query.ComputeQueryOption {
	public ComputeQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.OData.UriParser.ComputeClause ComputeClause  { public get; }
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	System.Type ResultClrType  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.IComputeQueryValidator Validator  { public get; public set; }

	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.CountQueryOption {
	public CountQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.ICountQueryValidator Validator  { public get; public set; }
	bool Value  { public get; }

	public System.Nullable`1[[System.Int64]] GetEntityCount (System.Linq.IQueryable query)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.DefaultODataQueryRequestParser : IODataQueryRequestParser {
	public DefaultODataQueryRequestParser ()

	public virtual bool CanParse (Microsoft.AspNetCore.Http.HttpRequest request)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.String]] ParseAsync (Microsoft.AspNetCore.Http.HttpRequest request)
}

public class Microsoft.AspNetCore.OData.Query.DefaultQueryConfigurations : Microsoft.OData.ModelBuilder.Config.DefaultQuerySettings {
	public DefaultQueryConfigurations ()
}

public class Microsoft.AspNetCore.OData.Query.DefaultSkipTokenHandler : Microsoft.AspNetCore.OData.Query.SkipTokenHandler {
	public DefaultSkipTokenHandler ()

	public virtual IQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipTokenQueryOption, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipTokenQueryOption, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public virtual System.Uri GenerateNextPageLink (System.Uri baseUri, int pageSize, object instance, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext context)
}

[
AttributeUsageAttribute(),
]
public class Microsoft.AspNetCore.OData.Query.EnableQueryAttribute : Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute, IActionFilter, IAsyncActionFilter, IAsyncResultFilter, IFilterMetadata, IOrderedFilter, IResultFilter {
	public EnableQueryAttribute ()

	Microsoft.AspNetCore.OData.Query.AllowedArithmeticOperators AllowedArithmeticOperators  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedFunctions AllowedFunctions  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedLogicalOperators AllowedLogicalOperators  { public get; public set; }
	string AllowedOrderByProperties  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedQueryOptions AllowedQueryOptions  { public get; public set; }
	bool EnableConstantParameterization  { public get; public set; }
	bool EnableCorrelatedSubqueryBuffering  { public get; public set; }
	bool EnsureStableOrdering  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.HandleNullPropagationOption HandleNullPropagation  { public get; public set; }
	bool HandleReferenceNavigationPropertyExpandFilter  { public get; public set; }
	int MaxAnyAllExpressionDepth  { public get; public set; }
	int MaxExpansionDepth  { public get; public set; }
	int MaxNodeCount  { public get; public set; }
	int MaxOrderByNodeCount  { public get; public set; }
	int MaxSkip  { public get; public set; }
	int MaxTop  { public get; public set; }
	int PageSize  { public get; public set; }

	public virtual System.Linq.IQueryable ApplyQuery (System.Linq.IQueryable queryable, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public virtual object ApplyQuery (object entity, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	protected virtual Microsoft.AspNetCore.OData.Query.ODataQueryOptions CreateAndValidateQueryOptions (Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.AspNetCore.OData.Query.ODataQueryContext queryContext)
	public static Microsoft.AspNetCore.Mvc.SerializableError CreateErrorResponse (string message, params System.Exception exception)
	protected virtual Microsoft.AspNetCore.OData.Query.ODataQueryOptions CreateQueryOptionsOnExecuting (Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext actionExecutingContext)
	public virtual Microsoft.OData.Edm.IEdmModel GetModel (System.Type elementClrType, Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
	public virtual void OnActionExecuted (Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext actionExecutedContext)
	public virtual void OnActionExecuting (Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext actionExecutingContext)
	public virtual void ValidateQuery (Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
}

[
DefaultMemberAttribute(),
]
public class Microsoft.AspNetCore.OData.Query.ETag : System.Dynamic.DynamicObject, IDynamicMetaObjectProvider {
	public ETag ()

	System.Type EntityType  { public get; public set; }
	bool IsAny  { public get; public set; }
	bool IsIfNoneMatch  { public get; public set; }
	bool IsWellFormed  { public get; public set; }
	object Item [string key] { public get; public set; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual bool TryGetMember (System.Dynamic.GetMemberBinder binder, out System.Object& result)
	public virtual bool TrySetMember (System.Dynamic.SetMemberBinder binder, object value)
}

public class Microsoft.AspNetCore.OData.Query.ETag`1 : Microsoft.AspNetCore.OData.Query.ETag, IDynamicMetaObjectProvider {
	public ETag`1 ()

	public IQueryable`1 ApplyTo (IQueryable`1 query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
}

public class Microsoft.AspNetCore.OData.Query.FilterQueryOption {
	public FilterQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ComputeQueryOption Compute  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.OData.UriParser.FilterClause FilterClause  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.IFilterQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.ODataQueryContext {
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmType elementType, Microsoft.OData.UriParser.ODataPath path)
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, System.Type elementClrType, Microsoft.OData.UriParser.ODataPath path)

	Microsoft.AspNetCore.OData.Query.DefaultQueryConfigurations DefaultQueryConfigurations  { public get; }
	System.Type ElementClrType  { public get; }
	Microsoft.OData.Edm.IEdmType ElementType  { public get; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	Microsoft.OData.UriParser.ODataPath Path  { public get; }
	System.IServiceProvider RequestContainer  { public get; }
}

[
NonValidatingParameterBindingAttribute(),
ODataQueryParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Query.ODataQueryOptions {
	public ODataQueryOptions (Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.AspNetCore.Http.HttpRequest request)

	Microsoft.AspNetCore.OData.Query.ApplyQueryOption Apply  { public get; }
	Microsoft.AspNetCore.OData.Query.ComputeQueryOption Compute  { public get; }
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.AspNetCore.OData.Query.CountQueryOption Count  { public get; }
	Microsoft.AspNetCore.OData.Query.FilterQueryOption Filter  { public get; }
	Microsoft.AspNetCore.OData.Query.ETag IfMatch  { public virtual get; }
	Microsoft.AspNetCore.OData.Query.ETag IfNoneMatch  { public virtual get; }
	Microsoft.AspNetCore.OData.Query.OrderByQueryOption OrderBy  { public get; }
	Microsoft.AspNetCore.OData.Query.ODataRawQueryOptions RawValues  { public get; }
	Microsoft.AspNetCore.Http.HttpRequest Request  { public get; }
	Microsoft.AspNetCore.OData.Query.SearchQueryOption Search  { public get; }
	Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption SelectExpand  { public get; }
	Microsoft.AspNetCore.OData.Query.SkipQueryOption Skip  { public get; }
	Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption SkipToken  { public get; }
	Microsoft.AspNetCore.OData.Query.TopQueryOption Top  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.IODataQueryValidator Validator  { public get; public set; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.AllowedQueryOptions ignoreQueryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public virtual object ApplyTo (object entity, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.AllowedQueryOptions ignoreQueryOptions)
	public virtual object ApplyTo (object entity, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.AllowedQueryOptions ignoreQueryOptions)
	public virtual Microsoft.AspNetCore.OData.Query.OrderByQueryOption GenerateStableOrder ()
	internal virtual Microsoft.AspNetCore.OData.Query.ETag GetETag (Microsoft.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
	public bool IsSupportedQueryOption (string queryOptionName)
	public static bool IsSystemQueryOption (string queryOptionName)
	public static bool IsSystemQueryOption (string queryOptionName, bool isDollarSignOptional)
	public static IQueryable`1 LimitResults (IQueryable`1 queryable, int limit, out System.Boolean& resultsLimited)
	public static IQueryable`1 LimitResults (IQueryable`1 queryable, int limit, bool parameterize, out System.Boolean& resultsLimited)
	public virtual void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

[
ODataQueryParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Query.ODataQueryOptions`1 : Microsoft.AspNetCore.OData.Query.ODataQueryOptions {
	public ODataQueryOptions`1 (Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.AspNetCore.Http.HttpRequest request)

	ETag`1 IfMatch  { public get; }
	ETag`1 IfNoneMatch  { public get; }

	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	internal virtual Microsoft.AspNetCore.OData.Query.ETag GetETag (Microsoft.Net.Http.Headers.EntityTagHeaderValue etagHeaderValue)
}

public class Microsoft.AspNetCore.OData.Query.ODataQueryRequestMiddleware {
	public ODataQueryRequestMiddleware (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Query.IODataQueryRequestParser]] queryRequestParsers, Microsoft.AspNetCore.Http.RequestDelegate next)

	[
	AsyncStateMachineAttribute(),
	]
	public System.Threading.Tasks.Task Invoke (Microsoft.AspNetCore.Http.HttpContext context)
}

public class Microsoft.AspNetCore.OData.Query.ODataQuerySettings {
	public ODataQuerySettings ()

	bool EnableConstantParameterization  { public get; public set; }
	bool EnableCorrelatedSubqueryBuffering  { public get; public set; }
	bool EnsureStableOrdering  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.HandleNullPropagationOption HandleNullPropagation  { public get; public set; }
	bool HandleReferenceNavigationPropertyExpandFilter  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedQueryOptions IgnoredNestedQueryOptions  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedQueryOptions IgnoredQueryOptions  { public get; public set; }
	System.Nullable`1[[System.Int32]] PageSize  { public get; public set; }
	System.TimeZoneInfo TimeZone  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Query.ODataRawQueryOptions {
	public ODataRawQueryOptions ()

	string Apply  { public get; }
	string Compute  { public get; }
	string Count  { public get; }
	string DeltaToken  { public get; }
	string Expand  { public get; }
	string Filter  { public get; }
	string Format  { public get; }
	string OrderBy  { public get; }
	string Search  { public get; }
	string Select  { public get; }
	string Skip  { public get; }
	string SkipToken  { public get; }
	string Top  { public get; }
}

public class Microsoft.AspNetCore.OData.Query.OrderByCountNode : Microsoft.AspNetCore.OData.Query.OrderByNode {
	public OrderByCountNode (Microsoft.OData.UriParser.OrderByClause orderByClause)

	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
}

public class Microsoft.AspNetCore.OData.Query.OrderByItNode : Microsoft.AspNetCore.OData.Query.OrderByNode {
	public OrderByItNode (Microsoft.OData.UriParser.OrderByDirection direction)
}

public class Microsoft.AspNetCore.OData.Query.OrderByOpenPropertyNode : Microsoft.AspNetCore.OData.Query.OrderByNode {
	public OrderByOpenPropertyNode (Microsoft.OData.UriParser.OrderByClause orderByClause)

	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	string PropertyName  { public get; }
}

public class Microsoft.AspNetCore.OData.Query.OrderByPropertyNode : Microsoft.AspNetCore.OData.Query.OrderByNode {
	public OrderByPropertyNode (Microsoft.OData.UriParser.OrderByClause orderByClause)
	public OrderByPropertyNode (Microsoft.OData.Edm.IEdmProperty property, Microsoft.OData.UriParser.OrderByDirection direction)

	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	Microsoft.OData.Edm.IEdmProperty Property  { public get; }
}

public class Microsoft.AspNetCore.OData.Query.OrderByQueryOption {
	public OrderByQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ComputeQueryOption Compute  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Query.OrderByNode]] OrderByNodes  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.IOrderByQueryValidator Validator  { public get; public set; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query)
	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.QueryFilterProvider : IFilterProvider {
	public QueryFilterProvider (Microsoft.AspNetCore.Mvc.Filters.IActionFilter queryFilter)

	int Order  { public virtual get; }
	Microsoft.AspNetCore.Mvc.Filters.IActionFilter QueryFilter  { public get; }

	public virtual void OnProvidersExecuted (Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext context)
	public virtual void OnProvidersExecuting (Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext context)
}

public class Microsoft.AspNetCore.OData.Query.SearchQueryOption {
	public SearchQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	System.Type ResultClrType  { public get; }
	Microsoft.OData.UriParser.SearchClause SearchClause  { public get; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
}

public class Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption {
	public SelectExpandQueryOption (string select, string expand, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ComputeQueryOption Compute  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	int LevelsMaxLiteralExpansionDepth  { public get; public set; }
	string RawExpand  { public get; }
	string RawSelect  { public get; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.ISelectExpandQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable queryable, Microsoft.AspNetCore.OData.Query.ODataQuerySettings settings)
	public object ApplyTo (object entity, Microsoft.AspNetCore.OData.Query.ODataQuerySettings settings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.SkipQueryOption {
	public SkipQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.ISkipQueryValidator Validator  { public get; public set; }
	int Value  { public get; }

	public IQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption {
	public SkipTokenQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.AspNetCore.OData.Query.SkipTokenHandler Handler  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.ISkipTokenQueryValidator Validator  { public get; }

	public virtual IQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.TopQueryOption {
	public TopQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.ITopQueryValidator Validator  { public get; public set; }
	int Value  { public get; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Query.ODataQueryParameterBindingAttribute : Microsoft.AspNetCore.Mvc.ModelBinderAttribute, IBinderTypeProviderMetadata, IBindingSourceMetadata, IModelNameProvider {
	public ODataQueryParameterBindingAttribute ()
}

public interface Microsoft.AspNetCore.OData.Results.IODataErrorResult {
	Microsoft.OData.ODataError Error  { public abstract get; }
}

[
DataContractAttribute(),
]
public abstract class Microsoft.AspNetCore.OData.Results.PageResult {
	protected PageResult (System.Uri nextPageLink, System.Nullable`1[[System.Int64]] count)

	[
	DataMemberAttribute(),
	]
	System.Nullable`1[[System.Int64]] Count  { public get; }

	[
	DataMemberAttribute(),
	]
	System.Uri NextPageLink  { public get; }

	public abstract System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary ()
}

public abstract class Microsoft.AspNetCore.OData.Results.SingleResult {
	protected SingleResult (System.Linq.IQueryable queryable)

	System.Linq.IQueryable Queryable  { public get; }

	public static SingleResult`1 Create (IQueryable`1 queryable)
}

public class Microsoft.AspNetCore.OData.Results.BadRequestODataResult : Microsoft.AspNetCore.Mvc.BadRequestResult, IActionResult, IClientErrorActionResult, IStatusCodeActionResult, IODataErrorResult {
	public BadRequestODataResult (Microsoft.OData.ODataError odataError)
	public BadRequestODataResult (string message)

	Microsoft.OData.ODataError Error  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public class Microsoft.AspNetCore.OData.Results.ConflictODataResult : Microsoft.AspNetCore.Mvc.ConflictResult, IActionResult, IClientErrorActionResult, IStatusCodeActionResult, IODataErrorResult {
	public ConflictODataResult (Microsoft.OData.ODataError odataError)
	public ConflictODataResult (string message)

	Microsoft.OData.ODataError Error  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public class Microsoft.AspNetCore.OData.Results.CreatedODataResult`1 : Microsoft.AspNetCore.Mvc.ObjectResult, IActionResult, IStatusCodeActionResult {
	public CreatedODataResult`1 (T entity)

	T Entity  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public class Microsoft.AspNetCore.OData.Results.NotFoundODataResult : Microsoft.AspNetCore.Mvc.NotFoundResult, IActionResult, IClientErrorActionResult, IStatusCodeActionResult, IODataErrorResult {
	public NotFoundODataResult (Microsoft.OData.ODataError odataError)
	public NotFoundODataResult (string message)

	Microsoft.OData.ODataError Error  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public class Microsoft.AspNetCore.OData.Results.ODataErrorResult : Microsoft.AspNetCore.Mvc.ActionResult, IActionResult, IODataErrorResult {
	public ODataErrorResult (Microsoft.OData.ODataError odataError)
	public ODataErrorResult (string errorCode, string message)

	Microsoft.OData.ODataError Error  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

[
DataContractAttribute(),
]
public class Microsoft.AspNetCore.OData.Results.PageResult`1 : Microsoft.AspNetCore.OData.Results.PageResult, IEnumerable`1, IEnumerable {
	public PageResult`1 (IEnumerable`1 items, System.Uri nextPageLink, System.Nullable`1[[System.Int64]] count)

	[
	DataMemberAttribute(),
	]
	IEnumerable`1 Items  { public get; }

	public virtual IEnumerator`1 GetEnumerator ()
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	public virtual System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary ()
}

public class Microsoft.AspNetCore.OData.Results.UnauthorizedODataResult : Microsoft.AspNetCore.Mvc.UnauthorizedResult, IActionResult, IClientErrorActionResult, IStatusCodeActionResult, IODataErrorResult {
	public UnauthorizedODataResult (Microsoft.OData.ODataError odataError)
	public UnauthorizedODataResult (string message)

	Microsoft.OData.ODataError Error  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public class Microsoft.AspNetCore.OData.Results.UnprocessableEntityODataResult : Microsoft.AspNetCore.Mvc.UnprocessableEntityResult, IActionResult, IClientErrorActionResult, IStatusCodeActionResult, IODataErrorResult {
	public UnprocessableEntityODataResult (Microsoft.OData.ODataError odataError)
	public UnprocessableEntityODataResult (string message)

	Microsoft.OData.ODataError Error  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public class Microsoft.AspNetCore.OData.Results.UpdatedODataResult`1 : Microsoft.AspNetCore.Mvc.ObjectResult, IActionResult, IStatusCodeActionResult {
	public UpdatedODataResult`1 (T entity)

	T Entity  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task ExecuteResultAsync (Microsoft.AspNetCore.Mvc.ActionContext context)
}

public sealed class Microsoft.AspNetCore.OData.Results.SingleResult`1 : Microsoft.AspNetCore.OData.Results.SingleResult {
	public SingleResult`1 (IQueryable`1 queryable)

	IQueryable`1 Queryable  { public get; }
}

public interface Microsoft.AspNetCore.OData.Routing.IODataRoutingMetadata {
	bool IsConventional  { public abstract get; }
	Microsoft.OData.Edm.IEdmModel Model  { public abstract get; }
	string Prefix  { public abstract get; }
	Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate Template  { public abstract get; }
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Routing.ODataPathExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.Edm.IEdmType GetEdmType (Microsoft.OData.UriParser.ODataPath path)

	[
	ExtensionAttribute(),
	]
	public static Microsoft.OData.Edm.IEdmNavigationSource GetNavigationSource (Microsoft.OData.UriParser.ODataPath path)

	[
	ExtensionAttribute(),
	]
	public static string GetPathString (Microsoft.OData.UriParser.ODataPath path)

	[
	ExtensionAttribute(),
	]
	public static string GetPathString (System.Collections.Generic.IList`1[[Microsoft.OData.UriParser.ODataPathSegment]] segments)

	[
	ExtensionAttribute(),
	]
	public static bool IsStreamPropertyPath (Microsoft.OData.UriParser.ODataPath path)
}

public sealed class Microsoft.AspNetCore.OData.Routing.ODataSegmentKinds {
	public static string Action = "action"
	public static string Batch = "$batch"
	public static string Cast = "cast"
	public static string Count = "$count"
	public static string DynamicProperty = "dynamicproperty"
	public static string EntitySet = "entityset"
	public static string Function = "function"
	public static string Key = "key"
	public static string Metadata = "$metadata"
	public static string Navigation = "navigation"
	public static string PathTemplate = "template"
	public static string Property = "property"
	public static string Ref = "$ref"
	public static string ServiceBase = "~"
	public static string Singleton = "singleton"
	public static string UnboundAction = "unboundaction"
	public static string UnboundFunction = "unboundfunction"
	public static string Unresolved = "unresolved"
	public static string Value = "$value"
}

public class Microsoft.AspNetCore.OData.Routing.ODataPathNavigationSourceHandler : Microsoft.OData.UriParser.PathSegmentHandler {
	public ODataPathNavigationSourceHandler ()

	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	string Path  { public get; }

	public virtual void Handle (Microsoft.OData.UriParser.BatchSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.CountSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.DynamicPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.EntitySetSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.KeySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.MetadataSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.ODataPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationImportSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PathTemplateSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.SingletonSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.TypeSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.ValueSegment segment)
}

public class Microsoft.AspNetCore.OData.Routing.ODataPathSegmentHandler : Microsoft.OData.UriParser.PathSegmentHandler {
	public ODataPathSegmentHandler ()

	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	string PathLiteral  { public get; }

	public virtual void Handle (Microsoft.OData.UriParser.BatchSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.CountSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.DynamicPathSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.EntitySetSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.KeySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.MetadataSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationImportSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.OperationSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PathTemplateSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.PropertySegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.SingletonSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.TypeSegment segment)
	public virtual void Handle (Microsoft.OData.UriParser.ValueSegment segment)
}

public class Microsoft.AspNetCore.OData.Routing.ODataPathSegmentTranslator : Microsoft.OData.UriParser.PathSegmentTranslator`1[[Microsoft.OData.UriParser.ODataPathSegment]] {
	public ODataPathSegmentTranslator ()

	public static Microsoft.OData.UriParser.SingleValueNode TranslateParameterAlias (Microsoft.OData.UriParser.SingleValueNode node, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.UriParser.SingleValueNode]] parameterAliasNodes)
}

public class Microsoft.AspNetCore.OData.Routing.ODataRouteOptions {
	public ODataRouteOptions ()

	bool EnableActionNameCaseInsensitive  { public get; public set; }
	bool EnableControllerNameCaseInsensitive  { public get; public set; }
	bool EnableDollarCountRouting  { public get; public set; }
	bool EnableDollarValueRouting  { public get; public set; }
	bool EnableKeyAsSegment  { public get; public set; }
	bool EnableKeyInParenthesis  { public get; public set; }
	bool EnableNonParenthesisForEmptyParameterFunction  { public get; public set; }
	bool EnablePropertyNameCaseInsensitive  { public get; public set; }
	bool EnableQualifiedOperationCall  { public get; public set; }
	bool EnableUnqualifiedOperationCall  { public get; public set; }
}

public sealed class Microsoft.AspNetCore.OData.Routing.ODataRoutingMetadata : IODataRoutingMetadata {
	public ODataRoutingMetadata (string prefix, Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate template)

	bool IsConventional  { public virtual get; public set; }
	Microsoft.OData.Edm.IEdmModel Model  { public virtual get; }
	string Prefix  { public virtual get; }
	Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate Template  { public virtual get; }
}

public interface Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializer {
	Microsoft.OData.ODataPayloadKind ODataPayloadKind  { public abstract get; }

	System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public interface Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider {
	Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataEdmTypeDeserializer GetEdmTypeDeserializer (Microsoft.OData.Edm.IEdmTypeReference edmType, params bool isDelta)
	Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializer GetODataDeserializer (System.Type type, Microsoft.AspNetCore.Http.HttpRequest request)
}

public interface Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataEdmTypeDeserializer : IODataDeserializer {
	object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializer : IODataDeserializer {
	protected ODataDeserializer (Microsoft.OData.ODataPayloadKind payloadKind)

	Microsoft.OData.ODataPayloadKind ODataPayloadKind  { public virtual get; }

	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	protected ODataEdmTypeDeserializer (Microsoft.OData.ODataPayloadKind payloadKind)
	protected ODataEdmTypeDeserializer (Microsoft.OData.ODataPayloadKind payloadKind, Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider deserializerProvider)

	Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider DeserializerProvider  { public get; }

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataActionPayloadDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializer, IODataDeserializer {
	public ODataActionPayloadDeserializer (Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider deserializerProvider)

	Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider DeserializerProvider  { public get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataCollectionDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	public ODataCollectionDeserializer (Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider deserializerProvider)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)

	public virtual System.Collections.IEnumerable ReadCollectionValue (Microsoft.OData.ODataCollectionValue collectionValue, Microsoft.OData.Edm.IEdmTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeltaResourceSetDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	public ODataDeltaResourceSetDeserializer (Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider deserializerProvider)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)

	internal virtual object ReadDeltaDeletedLink (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaDeletedLinkWrapper deletedLink, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	internal virtual object ReadDeltaLink (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaLinkWrapper link, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadDeltaResource (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper resource, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual System.Collections.IEnumerable ReadDeltaResourceSet (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaResourceSetWrapper deltaResourceSet, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext {
	public ODataDeserializerContext ()

	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	Microsoft.OData.UriParser.ODataPath Path  { public get; public set; }
	Microsoft.AspNetCore.Http.HttpRequest Request  { public get; public set; }
	Microsoft.OData.Edm.IEdmTypeReference ResourceEdmType  { public get; public set; }
	System.Type ResourceType  { public get; public set; }
	System.TimeZoneInfo TimeZone  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerProvider : IODataDeserializerProvider {
	public ODataDeserializerProvider (System.IServiceProvider serviceProvider)

	public virtual Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataEdmTypeDeserializer GetEdmTypeDeserializer (Microsoft.OData.Edm.IEdmTypeReference edmType, params bool isDelta)
	public virtual Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializer GetODataDeserializer (System.Type type, Microsoft.AspNetCore.Http.HttpRequest request)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEntityReferenceLinkDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializer, IODataDeserializer {
	public ODataEntityReferenceLinkDeserializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEnumDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	public ODataEnumDeserializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataPrimitiveDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	public ODataPrimitiveDeserializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadPrimitive (Microsoft.OData.ODataProperty primitiveProperty, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	public ODataResourceDeserializer (Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider deserializerProvider)

	public virtual void ApplyDeletedResource (object resource, Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper resourceWrapper, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual void ApplyNestedProperties (object resource, Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual void ApplyNestedProperty (object resource, Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataNestedResourceInfoWrapper resourceInfoWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual void ApplyStructuralProperties (object resource, Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual void ApplyStructuralProperty (object resource, Microsoft.OData.ODataProperty structuralProperty, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object CreateResourceInstance (Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadResource (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmStructuredTypeReference structuredType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceSetDeserializer : Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataEdmTypeDeserializer, IODataDeserializer, IODataEdmTypeDeserializer {
	public ODataResourceSetDeserializer (Microsoft.AspNetCore.OData.Formatter.Deserialization.IODataDeserializerProvider deserializerProvider)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task`1[[System.Object]] ReadAsync (Microsoft.OData.ODataMessageReader messageReader, System.Type type, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)

	public virtual object ReadInline (object item, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadPrimitiveItem (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataPrimitiveWrapper primitiveWrapper, Microsoft.OData.Edm.IEdmTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadResourceItem (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper resourceWrapper, Microsoft.OData.Edm.IEdmTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual System.Collections.IEnumerable ReadResourceSet (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetWrapper resourceSet, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadResourceSetItem (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetWrapper resourceSetWrapper, Microsoft.OData.Edm.IEdmTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.MediaType.MediaTypeMapping {
	protected MediaTypeMapping (string mediaType)

	System.Net.Http.Headers.MediaTypeHeaderValue MediaType  { public get; protected set; }

	public abstract double TryMatchMediaType (Microsoft.AspNetCore.Http.HttpRequest request)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.MediaType.ODataRawValueMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.MediaTypeMapping {
	protected ODataRawValueMediaTypeMapping (string mediaType)

	protected abstract bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
	public virtual double TryMatchMediaType (Microsoft.AspNetCore.Http.HttpRequest request)
}

public class Microsoft.AspNetCore.OData.Formatter.MediaType.ODataBinaryValueMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.ODataRawValueMediaTypeMapping {
	public ODataBinaryValueMediaTypeMapping ()

	protected virtual bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
}

public class Microsoft.AspNetCore.OData.Formatter.MediaType.ODataCountMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.MediaTypeMapping {
	public ODataCountMediaTypeMapping ()

	public virtual double TryMatchMediaType (Microsoft.AspNetCore.Http.HttpRequest request)
}

public class Microsoft.AspNetCore.OData.Formatter.MediaType.ODataEnumValueMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.ODataRawValueMediaTypeMapping {
	public ODataEnumValueMediaTypeMapping ()

	protected virtual bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
}

public class Microsoft.AspNetCore.OData.Formatter.MediaType.ODataPrimitiveValueMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.ODataRawValueMediaTypeMapping {
	public ODataPrimitiveValueMediaTypeMapping ()

	protected virtual bool IsMatch (Microsoft.OData.UriParser.PropertySegment propertySegment)
}

public class Microsoft.AspNetCore.OData.Formatter.MediaType.ODataStreamMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.MediaTypeMapping {
	public ODataStreamMediaTypeMapping ()

	public virtual double TryMatchMediaType (Microsoft.AspNetCore.Http.HttpRequest request)
}

public class Microsoft.AspNetCore.OData.Formatter.MediaType.QueryStringMediaTypeMapping : Microsoft.AspNetCore.OData.Formatter.MediaType.MediaTypeMapping {
	public QueryStringMediaTypeMapping (string queryStringParameterName, string mediaType)
	public QueryStringMediaTypeMapping (string queryStringParameterName, string queryStringParameterValue, string mediaType)

	string QueryStringParameterName  { public get; }
	string QueryStringParameterValue  { public get; }

	public virtual double TryMatchMediaType (Microsoft.AspNetCore.Http.HttpRequest request)
}

public interface Microsoft.AspNetCore.OData.Formatter.Serialization.IODataEdmTypeSerializer : IODataSerializer {
	Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public interface Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializer {
	Microsoft.OData.ODataPayloadKind ODataPayloadKind  { public abstract get; }

	System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public interface Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider {
	Microsoft.AspNetCore.OData.Formatter.Serialization.IODataEdmTypeSerializer GetEdmTypeSerializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializer GetODataPayloadSerializer (System.Type type, Microsoft.AspNetCore.Http.HttpRequest request)
}

public interface Microsoft.AspNetCore.OData.Formatter.Serialization.IUntypedResourceMapper {
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] Map (object resource, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext context)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataEdmTypeSerializer, IODataSerializer {
	protected ODataEdmTypeSerializer (Microsoft.OData.ODataPayloadKind payloadKind)
	protected ODataEdmTypeSerializer (Microsoft.OData.ODataPayloadKind payloadKind, Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider serializerProvider)

	Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider SerializerProvider  { public get; }

	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer : IODataSerializer {
	protected ODataSerializer (Microsoft.OData.ODataPayloadKind payloadKind)

	Microsoft.OData.ODataPayloadKind ODataPayloadKind  { public virtual get; }

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.DefaultUntypedResourceMapper : IUntypedResourceMapper {
	public static Microsoft.AspNetCore.OData.Formatter.Serialization.IUntypedResourceMapper Instance = Microsoft.AspNetCore.OData.Formatter.Serialization.DefaultUntypedResourceMapper

	public DefaultUntypedResourceMapper ()

	public virtual System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] Map (object resource, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext context)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataCollectionSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer, IODataEdmTypeSerializer, IODataSerializer {
	public ODataCollectionSerializer (Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider serializerProvider)

	protected static void AddTypeNameAnnotationAsNeeded (Microsoft.OData.ODataCollectionValue value, Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel metadataLevel)
	public virtual Microsoft.OData.ODataCollectionValue CreateODataCollectionValue (System.Collections.IEnumerable enumerable, Microsoft.OData.Edm.IEdmTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteCollectionAsync (Microsoft.OData.ODataCollectionWriter writer, object graph, Microsoft.OData.Edm.IEdmTypeReference collectionType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataDeltaResourceSetSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer, IODataEdmTypeSerializer, IODataSerializer {
	public ODataDeltaResourceSetSerializer (Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataDeltaResourceSet CreateODataDeltaResourceSet (System.Collections.IEnumerable feedInstance, Microsoft.OData.Edm.IEdmCollectionTypeReference feedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaDeletedLinkAsync (object value, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaDeletedResourceAsync (object value, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaLinkAsync (object value, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEntityReferenceLinkSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataSerializer {
	public ODataEntityReferenceLinkSerializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEntityReferenceLinksSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataSerializer {
	public ODataEntityReferenceLinksSerializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEnumSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer, IODataEdmTypeSerializer, IODataSerializer {
	public ODataEnumSerializer (Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataEnumValue CreateODataEnumValue (object graph, Microsoft.OData.Edm.IEdmEnumTypeReference enumType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataErrorSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataSerializer {
	public ODataErrorSerializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataMetadataSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataSerializer {
	public ODataMetadataSerializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataPrimitiveSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer, IODataEdmTypeSerializer, IODataSerializer {
	public ODataPrimitiveSerializer ()

	public virtual Microsoft.OData.ODataPrimitiveValue CreateODataPrimitiveValue (object graph, Microsoft.OData.Edm.IEdmPrimitiveTypeReference primitiveType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataValue CreateODataValue (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataRawValueSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataSerializer {
	public ODataRawValueSerializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataResourceSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer, IODataEdmTypeSerializer, IODataSerializer {
	public ODataResourceSerializer (Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider serializerProvider)

	public virtual void AppendDynamicProperties (Microsoft.OData.ODataResource resource, Microsoft.AspNetCore.OData.Formatter.Serialization.SelectExpandNode selectExpandNode, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataNestedResourceInfo CreateComplexNestedResourceInfo (Microsoft.OData.Edm.IEdmStructuralProperty complexProperty, Microsoft.OData.UriParser.PathSelectItem pathSelectItem, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataProperty CreateComputedProperty (string propertyName, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataNestedResourceInfo CreateDynamicComplexNestedResourceInfo (string propertyName, object propertyValue, Microsoft.OData.Edm.IEdmTypeReference edmType, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual string CreateETag (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataNestedResourceInfo CreateNavigationLink (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataAction CreateODataAction (Microsoft.OData.Edm.IEdmAction action, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataFunction CreateODataFunction (Microsoft.OData.Edm.IEdmFunction function, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataResource CreateResource (Microsoft.AspNetCore.OData.Formatter.Serialization.SelectExpandNode selectExpandNode, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.AspNetCore.OData.Formatter.Serialization.SelectExpandNode CreateSelectExpandNode (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataStreamPropertyInfo CreateStreamProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataProperty CreateStructuralProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataNestedResourceInfo CreateUntypedNestedResourceInfo (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, object propertyValue, Microsoft.OData.Edm.IEdmTypeReference valueType, Microsoft.OData.UriParser.PathSelectItem pathSelectItem, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual object CreateUntypedPropertyValue (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext, out Microsoft.OData.Edm.IEdmTypeReference& actualType)
	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteDeltaObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataResourceSetSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataEdmTypeSerializer, IODataEdmTypeSerializer, IODataSerializer {
	public ODataResourceSetSerializer (Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider serializerProvider)

	public virtual Microsoft.OData.ODataOperation CreateODataOperation (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.OData.Formatter.ResourceSetContext resourceSetContext, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	public virtual Microsoft.OData.ODataResourceSet CreateResourceSet (System.Collections.IEnumerable resourceSetInstance, Microsoft.OData.Edm.IEdmCollectionTypeReference resourceSetType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	[
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task WriteEnumItemAsync (object enumValue, Microsoft.OData.Edm.IEdmTypeReference enumType, Microsoft.OData.Edm.IEdmTypeReference parentSetType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task WritePrimitiveItemAsync (object primitiveValue, Microsoft.OData.Edm.IEdmTypeReference primitiveType, Microsoft.OData.Edm.IEdmTypeReference parentSetType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task WriteResourceItemAsync (object resourceValue, Microsoft.OData.Edm.IEdmTypeReference resourceType, Microsoft.OData.Edm.IEdmTypeReference parentSetType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	protected virtual System.Threading.Tasks.Task WriteResourceSetItemAsync (object itemSetValue, Microsoft.OData.Edm.IEdmTypeReference itemSetType, Microsoft.OData.Edm.IEdmTypeReference parentSetType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext {
	public ODataSerializerContext ()
	public ODataSerializerContext (Microsoft.AspNetCore.OData.Formatter.ResourceContext resource, Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmProperty edmProperty)

	System.Collections.Generic.ISet`1[[System.String]] ComputedProperties  { public get; }
	Microsoft.OData.Edm.IEdmProperty EdmProperty  { public get; public set; }
	Microsoft.AspNetCore.OData.Formatter.ResourceContext ExpandedResource  { public get; public set; }
	bool ExpandReference  { public get; public set; }
	System.Collections.Generic.IDictionary`2[[System.Object],[System.Object]] Items  { public get; }
	Microsoft.AspNetCore.OData.Formatter.ODataMetadataLevel MetadataLevel  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationProperty NavigationProperty  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	Microsoft.OData.UriParser.ODataPath Path  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.ODataQueryOptions QueryOptions  { public get; }
	Microsoft.AspNetCore.Http.HttpRequest Request  { public get; public set; }
	string RootElementName  { public get; public set; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public get; public set; }
	bool SkipExpensiveAvailabilityChecks  { public get; public set; }
	System.TimeZoneInfo TimeZone  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerProvider : IODataSerializerProvider {
	public ODataSerializerProvider (System.IServiceProvider serviceProvider)

	public virtual Microsoft.AspNetCore.OData.Formatter.Serialization.IODataEdmTypeSerializer GetEdmTypeSerializer (Microsoft.OData.Edm.IEdmTypeReference edmType)
	public virtual Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializer GetODataPayloadSerializer (System.Type type, Microsoft.AspNetCore.Http.HttpRequest request)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataServiceDocumentSerializer : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializer, IODataSerializer {
	public ODataServiceDocumentSerializer ()

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.SelectExpandNode {
	public SelectExpandNode ()
	public SelectExpandNode (Microsoft.OData.Edm.IEdmStructuredType structuredType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
	public SelectExpandNode (Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmStructuredType structuredType, Microsoft.OData.Edm.IEdmModel model)

	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmNavigationProperty],[Microsoft.OData.UriParser.ExpandedNavigationSelectItem]] ExpandedProperties  { public get; }
	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmNavigationProperty],[Microsoft.OData.UriParser.ExpandedReferenceSelectItem]] ReferencedProperties  { public get; }
	bool SelectAllDynamicProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmAction]] SelectedActions  { public get; }
	System.Collections.Generic.IDictionary`2[[Microsoft.OData.Edm.IEdmStructuralProperty],[Microsoft.OData.UriParser.PathSelectItem]] SelectedComplexProperties  { public get; }
	System.Collections.Generic.ISet`1[[System.String]] SelectedComputedProperties  { public get; }
	System.Collections.Generic.ISet`1[[System.String]] SelectedDynamicProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmFunction]] SelectedFunctions  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmNavigationProperty]] SelectedNavigationProperties  { public get; }
	System.Collections.Generic.ISet`1[[Microsoft.OData.Edm.IEdmStructuralProperty]] SelectedStructuralProperties  { public get; }
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmChangedObject : IEdmObject {
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public abstract get; }
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmComplexObject : IEdmObject, IEdmStructuredObject {
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmDeltaDeletedLink : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject {
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmDeltaDeletedResourceObject : IEdmChangedObject, IEdmObject {
	System.Uri Id  { public abstract get; public abstract set; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmDeltaLink : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject {
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmDeltaLinkBase : IEdmChangedObject, IEdmObject {
	string Relationship  { public abstract get; public abstract set; }
	System.Uri Source  { public abstract get; public abstract set; }
	System.Uri Target  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmEntityObject : IEdmObject, IEdmStructuredObject {
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmEnumObject : IEdmObject {
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmObject {
	Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmStructuredObject : IEdmObject {
	bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

public interface Microsoft.AspNetCore.OData.Formatter.Value.IEdmUntypedObject : IEdmObject, IEdmStructuredObject {
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaLinkBase : IEdmChangedObject, IEdmDeltaLinkBase, IEdmObject {
	protected EdmDeltaLinkBase (Microsoft.OData.Edm.IEdmEntityTypeReference typeReference)
	protected EdmDeltaLinkBase (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	Microsoft.OData.Edm.IEdmEntityType EntityType  { public get; }
	bool IsNullable  { public get; }
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public abstract get; }
	string Relationship  { public virtual get; public virtual set; }
	System.Uri Source  { public virtual get; public virtual set; }
	System.Uri Target  { public virtual get; public virtual set; }

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public abstract class Microsoft.AspNetCore.OData.Formatter.Value.EdmStructuredObject : Microsoft.AspNetCore.OData.Deltas.Delta, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, IEdmChangedObject, IEdmObject, IEdmStructuredObject {
	protected EdmStructuredObject (Microsoft.OData.Edm.IEdmStructuredType edmType)
	protected EdmStructuredObject (Microsoft.OData.Edm.IEdmStructuredTypeReference edmType)
	protected EdmStructuredObject (Microsoft.OData.Edm.IEdmStructuredType edmType, bool isNullable)

	Microsoft.OData.Edm.IEdmStructuredType ActualEdmType  { public get; public set; }
	Microsoft.OData.Edm.IEdmStructuredType ExpectedEdmType  { public get; public set; }
	bool IsNullable  { public get; public set; }
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }

	public virtual void Clear ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
	public virtual System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] GetDeltaNestedNavigationProperties ()
	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public System.Collections.Generic.Dictionary`2[[System.String],[System.Object]] TryGetDynamicProperties ()
	public virtual bool TryGetPropertyType (string name, out System.Type& type)
	public virtual bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetPropertyValue (string name, object value)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.Value.EdmTypeExtensions {
	[
	ExtensionAttribute(),
	]
	public static bool IsDeltaResource (Microsoft.AspNetCore.OData.Formatter.Value.IEdmObject resource)

	[
	ExtensionAttribute(),
	]
	public static bool IsDeltaResourceSet (Microsoft.OData.Edm.IEdmType type)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmChangedObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmChangedObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmChangedObjectCollection (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmChangedObjectCollection (Microsoft.OData.Edm.IEdmEntityType entityType, System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmChangedObject]] changedObjectList)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmComplexObject : Microsoft.AspNetCore.OData.Formatter.Value.EdmStructuredObject, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, IEdmChangedObject, IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType)
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)
	public EdmComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmComplexObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmComplexObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmComplexObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmComplexObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmComplexObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaComplexObject : Microsoft.AspNetCore.OData.Formatter.Value.EdmComplexObject, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, IEdmChangedObject, IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType)
	public EdmDeltaComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)
	public EdmDeltaComplexObject (Microsoft.OData.Edm.IEdmComplexType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaDeletedLink : Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaLinkBase, IEdmChangedObject, IEdmDeltaDeletedLink, IEdmDeltaLinkBase, IEdmObject {
	public EdmDeltaDeletedLink (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaDeletedLink (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaDeletedLink (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaDeletedResourceObject : Microsoft.AspNetCore.OData.Formatter.Value.EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, IEdmChangedObject, IEdmDeltaDeletedResourceObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaDeletedResourceObject (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaDeletedResourceObject (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaDeletedResourceObject (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	System.Uri Id  { public virtual get; public virtual set; }
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public virtual get; public virtual set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaLink : Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaLinkBase, IEdmChangedObject, IEdmDeltaLink, IEdmDeltaLinkBase, IEdmObject {
	public EdmDeltaLink (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaLink (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaLink (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmDeltaResourceObject : Microsoft.AspNetCore.OData.Formatter.Value.EdmEntityObject, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, IEdmChangedObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmDeltaResourceObject (Microsoft.OData.Edm.IEdmEntityType entityType)
	public EdmDeltaResourceObject (Microsoft.OData.Edm.IEdmEntityTypeReference entityTypeReference)
	public EdmDeltaResourceObject (Microsoft.OData.Edm.IEdmEntityType entityType, bool isNullable)

	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind DeltaKind  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmEntityObject : Microsoft.AspNetCore.OData.Formatter.Value.EdmStructuredObject, IDynamicMetaObjectProvider, IDelta, IDeltaSetItem, IEdmChangedObject, IEdmEntityObject, IEdmObject, IEdmStructuredObject {
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityType edmType)
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityTypeReference edmType)
	public EdmEntityObject (Microsoft.OData.Edm.IEdmEntityType edmType, bool isNullable)
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmEntityObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmEntityObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmEntityObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmEntityObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmEntityObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmEnumObject : IEdmEnumObject, IEdmObject {
	public EdmEnumObject (Microsoft.OData.Edm.IEdmEnumType edmType, string value)
	public EdmEnumObject (Microsoft.OData.Edm.IEdmEnumTypeReference edmType, string value)
	public EdmEnumObject (Microsoft.OData.Edm.IEdmEnumType edmType, string value, bool isNullable)

	bool IsNullable  { public get; public set; }
	string Value  { public get; public set; }

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public class Microsoft.AspNetCore.OData.Formatter.Value.EdmEnumObjectCollection : System.Collections.ObjectModel.Collection`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmEnumObject]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmEnumObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType)
	public EdmEnumObjectCollection (Microsoft.OData.Edm.IEdmCollectionTypeReference edmType, System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Value.IEdmEnumObject]] list)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

public class Microsoft.AspNetCore.OData.Formatter.Value.NullEdmComplexObject : IEdmComplexObject, IEdmObject, IEdmStructuredObject {
	public NullEdmComplexObject (Microsoft.OData.Edm.IEdmComplexTypeReference edmType)

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
	public virtual bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

[
NonValidatingParameterBindingAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.Value.EdmUntypedCollection : System.Collections.Generic.List`1[[System.Object]], ICollection, IEnumerable, IList, IEdmObject, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public EdmUntypedCollection ()

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
}

[
NonValidatingParameterBindingAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.Value.EdmUntypedObject : System.Collections.Generic.Dictionary`2[[System.String],[System.Object]], ICollection, IDictionary, IEnumerable, IDeserializationCallback, ISerializable, IEdmObject, IEdmStructuredObject, IEdmUntypedObject, IDictionary`2, IReadOnlyDictionary`2, ICollection`1, IEnumerable`1, IReadOnlyCollection`1 {
	public EdmUntypedObject ()

	public virtual Microsoft.OData.Edm.IEdmTypeReference GetEdmType ()
	public virtual bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaLinkBaseWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	protected ODataDeltaLinkBaseWrapper ()
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	protected ODataItemWrapper ()
}

public abstract class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetBaseWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	protected ODataResourceSetBaseWrapper ()
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataReaderExtensions {
	[
	ExtensionAttribute(),
	]
	public static Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper ReadResourceOrResourceSet (Microsoft.OData.ODataReader reader)

	[
	AsyncStateMachineAttribute(),
	ExtensionAttribute(),
	]
	public static System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper]] ReadResourceOrResourceSetAsync (Microsoft.OData.ODataReader reader)
}

public class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataEntityReferenceLinkWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	public ODataEntityReferenceLinkWrapper (Microsoft.OData.ODataEntityReferenceLink link)

	Microsoft.OData.ODataEntityReferenceLink EntityReferenceLink  { public get; }
}

public class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataPrimitiveWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	public ODataPrimitiveWrapper (Microsoft.OData.ODataPrimitiveValue value)

	Microsoft.OData.ODataPrimitiveValue Value  { public get; }
}

public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaDeletedLinkWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaLinkBaseWrapper {
	public ODataDeltaDeletedLinkWrapper (Microsoft.OData.ODataDeltaDeletedLink deltaDeletedLink)

	Microsoft.OData.ODataDeltaDeletedLink DeltaDeletedLink  { public get; }
}

public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaLinkWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaLinkBaseWrapper {
	public ODataDeltaLinkWrapper (Microsoft.OData.ODataDeltaLink deltaLink)

	Microsoft.OData.ODataDeltaLink DeltaLink  { public get; }
}

public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaResourceSetWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetBaseWrapper {
	public ODataDeltaResourceSetWrapper (Microsoft.OData.ODataDeltaResourceSet deltaResourceSet)

	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper]] DeltaItems  { public get; }
	Microsoft.OData.ODataDeltaResourceSet DeltaResourceSet  { public get; }
}

public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataNestedResourceInfoWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	public ODataNestedResourceInfoWrapper (Microsoft.OData.ODataNestedResourceInfo nestedInfo)

	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper]] NestedItems  { public get; }
	Microsoft.OData.ODataNestedResourceInfo NestedResourceInfo  { public get; }
}

public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetBaseWrapper {
	public ODataResourceSetWrapper (Microsoft.OData.ODataResourceSet resourceSet)

	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper]] Items  { public get; }
	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper]] Resources  { public get; }
	Microsoft.OData.ODataResourceSet ResourceSet  { public get; }
}

public sealed class Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceWrapper : Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataItemWrapper {
	public ODataResourceWrapper (Microsoft.OData.ODataResourceBase resource)

	bool IsDeletedResource  { public get; }
	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataNestedResourceInfoWrapper]] NestedResourceInfos  { public get; }
	Microsoft.OData.ODataResourceBase Resource  { public get; }
}

public interface Microsoft.AspNetCore.OData.Query.Container.IPropertyMapper {
	string MapProperty (string propertyName)
}

public interface Microsoft.AspNetCore.OData.Query.Container.ITruncatedCollection : IEnumerable {
	bool IsTruncated  { public abstract get; }
	int PageSize  { public abstract get; }
}

public class Microsoft.AspNetCore.OData.Query.Container.NamedPropertyExpression {
	public NamedPropertyExpression (System.Linq.Expressions.Expression name, System.Linq.Expressions.Expression value)

	bool AutoSelected  { public get; public set; }
	System.Nullable`1[[System.Boolean]] CountOption  { public get; public set; }
	System.Linq.Expressions.Expression Name  { public get; }
	System.Linq.Expressions.Expression NullCheck  { public get; public set; }
	System.Nullable`1[[System.Int32]] PageSize  { public get; public set; }
	System.Linq.Expressions.Expression TotalCount  { public get; public set; }
	System.Linq.Expressions.Expression Value  { public get; }
}

public class Microsoft.AspNetCore.OData.Query.Container.TruncatedCollection`1 : List`1, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1, ICollection, IEnumerable, IList, ICountOptionCollection, ITruncatedCollection {
	public TruncatedCollection`1 (IEnumerable`1 source, int pageSize)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize)
	public TruncatedCollection`1 (IEnumerable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, bool parameterize)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount)
	public TruncatedCollection`1 (IQueryable`1 source, int pageSize, System.Nullable`1[[System.Int64]] totalCount, bool parameterize)

	bool IsTruncated  { public virtual get; }
	int PageSize  { public virtual get; }
	System.Nullable`1[[System.Int64]] TotalCount  { public virtual get; }
}

public interface Microsoft.AspNetCore.OData.Query.Expressions.IFilterBinder {
	System.Linq.Expressions.Expression BindFilter (Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

public interface Microsoft.AspNetCore.OData.Query.Expressions.IOrderByBinder {
	Microsoft.AspNetCore.OData.Query.Expressions.OrderByBinderResult BindOrderBy (Microsoft.OData.UriParser.OrderByClause orderByClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

public interface Microsoft.AspNetCore.OData.Query.Expressions.ISearchBinder {
	System.Linq.Expressions.Expression BindSearch (Microsoft.OData.UriParser.SearchClause searchClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

public interface Microsoft.AspNetCore.OData.Query.Expressions.ISelectExpandBinder {
	System.Linq.Expressions.Expression BindSelectExpand (Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

public abstract class Microsoft.AspNetCore.OData.Query.Expressions.ExpressionBinderBase {
	protected ExpressionBinderBase (System.IServiceProvider requestContainer)

	System.Linq.Expressions.ParameterExpression Parameter  { protected abstract get; }

	public abstract System.Linq.Expressions.Expression Bind (Microsoft.OData.UriParser.QueryNode node)
	protected System.Linq.Expressions.Expression[] BindArguments (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.UriParser.QueryNode]] nodes)
	public virtual System.Linq.Expressions.Expression BindCollectionConstantNode (Microsoft.OData.UriParser.CollectionConstantNode node)
	public virtual System.Linq.Expressions.Expression BindConstantNode (Microsoft.OData.UriParser.ConstantNode constantNode)
	public virtual System.Linq.Expressions.Expression BindSingleValueFunctionCallNode (Microsoft.OData.UriParser.SingleValueFunctionCallNode node)
	protected void EnsureFlattenedPropertyContainer (System.Linq.Expressions.ParameterExpression source)
	protected System.Reflection.PropertyInfo GetDynamicPropertyContainer (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode openNode)
	protected System.Linq.Expressions.Expression GetFlattenedPropertyExpression (string propertyPath)
}

public abstract class Microsoft.AspNetCore.OData.Query.Expressions.QueryBinder {
	protected QueryBinder ()

	protected static System.Linq.Expressions.Expression ApplyNullPropagationForFilterBody (System.Linq.Expressions.Expression body, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression Bind (Microsoft.OData.UriParser.QueryNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindAllNode (Microsoft.OData.UriParser.AllNode allNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindAnyNode (Microsoft.OData.UriParser.AnyNode anyNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected System.Linq.Expressions.Expression[] BindArguments (System.Collections.Generic.IEnumerable`1[[Microsoft.OData.UriParser.QueryNode]] nodes, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindBinaryOperatorNode (Microsoft.OData.UriParser.BinaryOperatorNode binaryOperatorNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindCastSingleValue (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindCeiling (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindCollectionComplexNode (Microsoft.OData.UriParser.CollectionComplexNode collectionComplexNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindCollectionConstantNode (Microsoft.OData.UriParser.CollectionConstantNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindCollectionNode (Microsoft.OData.UriParser.CollectionNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindCollectionPropertyAccessNode (Microsoft.OData.UriParser.CollectionPropertyAccessNode propertyAccessNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindCollectionResourceCastNode (Microsoft.OData.UriParser.CollectionResourceCastNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindConcat (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindConstantNode (Microsoft.OData.UriParser.ConstantNode constantNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindContains (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindConvertNode (Microsoft.OData.UriParser.ConvertNode convertNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindCountNode (Microsoft.OData.UriParser.CountNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindCustomMethodExpressionOrNull (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindDate (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindDateRelatedProperty (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindDynamicPropertyAccessQueryNode (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode openNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindEndsWith (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindFloor (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindFractionalSeconds (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindIndexOf (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindInNode (Microsoft.OData.UriParser.InNode inNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindIsOf (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindLength (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindMatchesPattern (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, string propertyPath, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindNow (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindPropertyAccessQueryNode (Microsoft.OData.UriParser.SingleValuePropertyAccessNode propertyAccessNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindRangeVariable (Microsoft.OData.UriParser.RangeVariable rangeVariable, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindRound (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindSingleComplexNode (Microsoft.OData.UriParser.SingleComplexNode singleComplexNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindSingleResourceCastFunctionCall (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindSingleResourceCastNode (Microsoft.OData.UriParser.SingleResourceCastNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindSingleResourceFunctionCallNode (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindSingleValueFunctionCallNode (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindSingleValueNode (Microsoft.OData.UriParser.SingleValueNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindStartsWith (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindSubstring (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindTime (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindTimeRelatedProperty (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindToLower (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindToUpper (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected virtual System.Linq.Expressions.Expression BindTrim (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual System.Linq.Expressions.Expression BindUnaryOperatorNode (Microsoft.OData.UriParser.UnaryOperatorNode unaryOperatorNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected static System.Reflection.PropertyInfo GetDynamicPropertyContainer (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode openNode, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	protected System.Linq.Expressions.Expression GetFlattenedPropertyExpression (string propertyPath, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

[
ExtensionAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Query.Expressions.BinderExtensions {
	[
	ExtensionAttribute(),
	]
	public static System.Collections.IEnumerable ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.IFilterBinder binder, System.Collections.IEnumerable query, Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)

	[
	ExtensionAttribute(),
	]
	public static System.Linq.Expressions.Expression ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.IFilterBinder binder, System.Linq.Expressions.Expression source, Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)

	[
	ExtensionAttribute(),
	]
	public static System.Linq.IQueryable ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.IFilterBinder binder, System.Linq.IQueryable query, Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)

	[
	ExtensionAttribute(),
	]
	public static System.Linq.IQueryable ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.ISearchBinder binder, System.Linq.IQueryable source, Microsoft.OData.UriParser.SearchClause searchClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)

	[
	ExtensionAttribute(),
	]
	public static System.Linq.IQueryable ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.ISelectExpandBinder binder, System.Linq.IQueryable source, Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)

	[
	ExtensionAttribute(),
	]
	public static object ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.ISelectExpandBinder binder, object source, Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)

	[
	ExtensionAttribute(),
	]
	public static System.Linq.Expressions.Expression ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.IOrderByBinder binder, System.Linq.Expressions.Expression source, Microsoft.OData.UriParser.OrderByClause orderByClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, bool alreadyOrdered)

	[
	ExtensionAttribute(),
	]
	public static System.Linq.IQueryable ApplyBind (Microsoft.AspNetCore.OData.Query.Expressions.IOrderByBinder binder, System.Linq.IQueryable query, Microsoft.OData.UriParser.OrderByClause orderByClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, bool alreadyOrdered)
}

public class Microsoft.AspNetCore.OData.Query.Expressions.FilterBinder : Microsoft.AspNetCore.OData.Query.Expressions.QueryBinder, IFilterBinder {
	public FilterBinder ()

	public virtual System.Linq.Expressions.Expression BindFilter (Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

public class Microsoft.AspNetCore.OData.Query.Expressions.OrderByBinder : Microsoft.AspNetCore.OData.Query.Expressions.QueryBinder, IOrderByBinder {
	public OrderByBinder ()

	public virtual Microsoft.AspNetCore.OData.Query.Expressions.OrderByBinderResult BindOrderBy (Microsoft.OData.UriParser.OrderByClause orderByClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
}

public class Microsoft.AspNetCore.OData.Query.Expressions.OrderByBinderResult {
	public OrderByBinderResult (System.Linq.Expressions.Expression orderByExpression, Microsoft.OData.UriParser.OrderByDirection direction)

	Microsoft.OData.UriParser.OrderByDirection Direction  { public get; }
	System.Linq.Expressions.Expression OrderByExpression  { public get; }
	Microsoft.AspNetCore.OData.Query.Expressions.OrderByBinderResult ThenBy  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext {
	public QueryBinderContext (Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, System.Type clrType)
	public QueryBinderContext (Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, System.Type clrType)

	Microsoft.OData.ModelBuilder.IAssemblyResolver AssembliesResolver  { public get; public set; }
	System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.UriParser.ComputeExpression]] ComputedProperties  { public get; }
	System.Linq.Expressions.ParameterExpression CurrentParameter  { public get; }
	System.Type ElementClrType  { public get; }
	Microsoft.OData.Edm.IEdmType ElementType  { public get; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.ODataQuerySettings QuerySettings  { public get; }
	System.Linq.Expressions.Expression Source  { public get; public set; }

	public System.Linq.Expressions.ParameterExpression GetParameter (string name)
	public void RemoveParameter (string name)
}

public class Microsoft.AspNetCore.OData.Query.Expressions.SelectExpandBinder : Microsoft.AspNetCore.OData.Query.Expressions.QueryBinder, ISelectExpandBinder {
	public SelectExpandBinder (Microsoft.AspNetCore.OData.Query.Expressions.IFilterBinder filterBinder, Microsoft.AspNetCore.OData.Query.Expressions.IOrderByBinder orderByBinder)

	Microsoft.AspNetCore.OData.Query.Expressions.IFilterBinder FilterBinder  { public get; }
	Microsoft.AspNetCore.OData.Query.Expressions.IOrderByBinder OrderByBinder  { public get; }

	public virtual void BindComputedProperty (System.Linq.Expressions.Expression source, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, string computedProperty, System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Query.Container.NamedPropertyExpression]] includedProperties)
	public virtual System.Linq.Expressions.Expression BindSelectExpand (Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context)
	public virtual void BuildDynamicProperty (Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, System.Linq.Expressions.Expression source, Microsoft.OData.Edm.IEdmStructuredType structuredType, System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Query.Container.NamedPropertyExpression]] includedProperties)
	public virtual System.Linq.Expressions.Expression CreatePropertyNameExpression (Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, Microsoft.OData.Edm.IEdmStructuredType elementType, Microsoft.OData.Edm.IEdmProperty edmProperty, System.Linq.Expressions.Expression source)
	public virtual System.Linq.Expressions.Expression CreatePropertyValueExpression (Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, Microsoft.OData.Edm.IEdmStructuredType elementType, Microsoft.OData.Edm.IEdmProperty edmProperty, System.Linq.Expressions.Expression source, Microsoft.OData.UriParser.FilterClause filterClause, params Microsoft.OData.UriParser.ComputeClause computeClause)
	public virtual System.Linq.Expressions.Expression CreateTotalCountExpression (Microsoft.AspNetCore.OData.Query.Expressions.QueryBinderContext context, System.Linq.Expressions.Expression source, System.Nullable`1[[System.Boolean]] countOption)
	public virtual System.Linq.Expressions.Expression CreateTypeNameExpression (System.Linq.Expressions.Expression source, Microsoft.OData.Edm.IEdmStructuredType elementType, Microsoft.OData.Edm.IEdmModel model)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.IComputeQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.ComputeQueryOption computeQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.ICountQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.CountQueryOption countQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.IFilterQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.FilterQueryOption filterQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.IODataQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.ODataQueryOptions options, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.IOrderByQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.OrderByQueryOption orderByOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.ISelectExpandQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption selectExpandQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.ISkipQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.SkipQueryOption skipQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.ISkipTokenQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipToken, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Validator.ITopQueryValidator {
	void Validate (Microsoft.AspNetCore.OData.Query.TopQueryOption topQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public abstract class Microsoft.AspNetCore.OData.Query.Validator.QueryValidatorContext {
	protected QueryValidatorContext ()

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; public set; }
	int CurrentDepth  { public get; public set; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.OData.Edm.IEdmProperty Property  { public get; public set; }
	Microsoft.OData.Edm.IEdmStructuredType StructuredType  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings ValidationSettings  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Query.Validator.ComputeQueryValidator : IComputeQueryValidator {
	public ComputeQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.ComputeQueryOption computeQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.CountQueryValidator : ICountQueryValidator {
	public CountQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.CountQueryOption countQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.FilterQueryValidator : IFilterQueryValidator {
	public FilterQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.FilterQueryOption filterQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateAllNode (Microsoft.OData.UriParser.AllNode allNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateAnyNode (Microsoft.OData.UriParser.AnyNode anyNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateArithmeticOperator (Microsoft.OData.UriParser.BinaryOperatorNode binaryNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateBinaryOperatorNode (Microsoft.OData.UriParser.BinaryOperatorNode binaryOperatorNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateCollectionComplexNode (Microsoft.OData.UriParser.CollectionComplexNode collectionComplexNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateCollectionNode (Microsoft.OData.UriParser.CollectionNode node, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateCollectionPropertyAccessNode (Microsoft.OData.UriParser.CollectionPropertyAccessNode propertyAccessNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateCollectionResourceCastNode (Microsoft.OData.UriParser.CollectionResourceCastNode collectionResourceCastNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateConstantNode (Microsoft.OData.UriParser.ConstantNode constantNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateConvertNode (Microsoft.OData.UriParser.ConvertNode convertNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateCountNode (Microsoft.OData.UriParser.CountNode countNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateFilter (Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateLogicalOperator (Microsoft.OData.UriParser.BinaryOperatorNode binaryNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateQueryNode (Microsoft.OData.UriParser.QueryNode node, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateRangeVariable (Microsoft.OData.UriParser.RangeVariable rangeVariable, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateSingleComplexNode (Microsoft.OData.UriParser.SingleComplexNode singleComplexNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateSingleResourceCastNode (Microsoft.OData.UriParser.SingleResourceCastNode singleResourceCastNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateSingleResourceFunctionCallNode (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateSingleValueFunctionCallNode (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateSingleValueNode (Microsoft.OData.UriParser.SingleValueNode node, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateSingleValuePropertyAccessNode (Microsoft.OData.UriParser.SingleValuePropertyAccessNode propertyAccessNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
	protected virtual void ValidateUnaryOperatorNode (Microsoft.OData.UriParser.UnaryOperatorNode unaryOperatorNode, Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext validatorContext)
}

public class Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext : Microsoft.AspNetCore.OData.Query.Validator.QueryValidatorContext {
	public FilterValidatorContext ()

	int CurrentAnyAllExpressionDepth  { public get; }
	int CurrentNodeCount  { public get; }
	Microsoft.AspNetCore.OData.Query.FilterQueryOption Filter  { public get; public set; }

	public Microsoft.AspNetCore.OData.Query.Validator.FilterValidatorContext Clone ()
	public void EnterLambda ()
	public void ExitLambda ()
	public void IncrementNodeCount ()
}

public class Microsoft.AspNetCore.OData.Query.Validator.ODataQueryValidator : IODataQueryValidator {
	public ODataQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.ODataQueryOptions options, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings {
	public ODataValidationSettings ()

	Microsoft.AspNetCore.OData.Query.AllowedArithmeticOperators AllowedArithmeticOperators  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedFunctions AllowedFunctions  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.AllowedLogicalOperators AllowedLogicalOperators  { public get; public set; }
	System.Collections.Generic.ISet`1[[System.String]] AllowedOrderByProperties  { public get; }
	Microsoft.AspNetCore.OData.Query.AllowedQueryOptions AllowedQueryOptions  { public get; public set; }
	int MaxAnyAllExpressionDepth  { public get; public set; }
	int MaxExpansionDepth  { public get; public set; }
	int MaxNodeCount  { public get; public set; }
	int MaxOrderByNodeCount  { public get; public set; }
	System.Nullable`1[[System.Int32]] MaxSkip  { public get; public set; }
	System.Nullable`1[[System.Int32]] MaxTop  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Query.Validator.OrderByQueryValidator : IOrderByQueryValidator {
	public OrderByQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.OrderByQueryOption orderByOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.SelectExpandQueryValidator : ISelectExpandQueryValidator {
	public SelectExpandQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption selectExpandQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
	protected virtual void ValidateExpandedCountSelectItem (Microsoft.OData.UriParser.ExpandedCountSelectItem expandCountItem, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateExpandedNavigationSelectItem (Microsoft.OData.UriParser.ExpandedNavigationSelectItem expandItem, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateExpandedReferenceSelectItem (Microsoft.OData.UriParser.ExpandedReferenceSelectItem expandReferItem, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNamespaceQualifiedWildcardSelectItem (Microsoft.OData.UriParser.NamespaceQualifiedWildcardSelectItem namespaceQualifiedWildcardSelectItem, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedApply (Microsoft.OData.UriParser.Aggregation.ApplyClause applyClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedCompute (Microsoft.OData.UriParser.ComputeClause computeClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedCount (System.Nullable`1[[System.Boolean]] countOption, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedFilter (Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedLevels (Microsoft.OData.UriParser.LevelsClause levelsClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedOrderby (Microsoft.OData.UriParser.OrderByClause orderByClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedSearch (Microsoft.OData.UriParser.SearchClause searchClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedSkip (System.Nullable`1[[System.Int64]] skipOption, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateNestedTop (System.Nullable`1[[System.Int64]] topOption, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidatePathSelectItem (Microsoft.OData.UriParser.PathSelectItem pathSelectItem, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateSelectExpand (Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
	protected virtual void ValidateWildcardSelectItem (Microsoft.OData.UriParser.WildcardSelectItem wildCardSelectItem, Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext validatorContext)
}

public class Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext : Microsoft.AspNetCore.OData.Query.Validator.QueryValidatorContext {
	public SelectExpandValidatorContext ()

	System.Nullable`1[[System.Int32]] RemainingDepth  { public get; public set; }
	Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption SelectExpand  { public get; public set; }

	public Microsoft.AspNetCore.OData.Query.Validator.SelectExpandValidatorContext Clone ()
}

public class Microsoft.AspNetCore.OData.Query.Validator.SkipQueryValidator : ISkipQueryValidator {
	public SkipQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.SkipQueryOption skipQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.SkipTokenQueryValidator : ISkipTokenQueryValidator {
	public SkipTokenQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipToken, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.TopQueryValidator : ITopQueryValidator {
	public TopQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.TopQueryOption topQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public interface Microsoft.AspNetCore.OData.Query.Wrapper.ISelectExpandWrapper {
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary ()
	System.Collections.Generic.IDictionary`2[[System.String],[System.Object]] ToDictionary (System.Func`3[[Microsoft.OData.Edm.IEdmModel],[Microsoft.OData.Edm.IEdmStructuredType],[Microsoft.AspNetCore.OData.Query.Container.IPropertyMapper]] propertyMapperProvider)
}

public abstract class Microsoft.AspNetCore.OData.Query.Wrapper.DynamicTypeWrapper {
	protected DynamicTypeWrapper ()

	System.Collections.Generic.Dictionary`2[[System.String],[System.Object]] Values  { public abstract get; }

	public virtual bool TryGetPropertyValue (string propertyName, out System.Object& value)
}

[
AttributeUsageAttribute(),
]
public class Microsoft.AspNetCore.OData.Routing.Attributes.ODataRouteComponentAttribute : System.Attribute {
	public ODataRouteComponentAttribute ()
	public ODataRouteComponentAttribute (string routePrefix)

	string RoutePrefix  { public get; }
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Routing.Attributes.ODataAttributeRoutingAttribute : System.Attribute {
	public ODataAttributeRoutingAttribute ()
}

[
AttributeUsageAttribute(),
]
public sealed class Microsoft.AspNetCore.OData.Routing.Attributes.ODataIgnoredAttribute : System.Attribute {
	public ODataIgnoredAttribute ()
}

[
ODataAttributeRoutingAttribute(),
]
public abstract class Microsoft.AspNetCore.OData.Routing.Controllers.ODataController : Microsoft.AspNetCore.Mvc.ControllerBase {
	protected ODataController ()

	protected virtual Microsoft.AspNetCore.OData.Results.BadRequestODataResult BadRequest (Microsoft.OData.ODataError odataError)
	protected virtual Microsoft.AspNetCore.OData.Results.BadRequestODataResult BadRequest (string message)
	protected virtual Microsoft.AspNetCore.OData.Results.ConflictODataResult Conflict (Microsoft.OData.ODataError odataError)
	protected virtual Microsoft.AspNetCore.OData.Results.ConflictODataResult Conflict (string message)
	protected virtual CreatedODataResult`1 Created (TEntity entity)
	protected virtual Microsoft.AspNetCore.OData.Results.NotFoundODataResult NotFound (Microsoft.OData.ODataError odataError)
	protected virtual Microsoft.AspNetCore.OData.Results.NotFoundODataResult NotFound (string message)
	protected virtual Microsoft.AspNetCore.OData.Results.ODataErrorResult ODataErrorResult (Microsoft.OData.ODataError odataError)
	protected virtual Microsoft.AspNetCore.OData.Results.ODataErrorResult ODataErrorResult (string errorCode, string message)
	protected virtual Microsoft.AspNetCore.OData.Results.UnauthorizedODataResult Unauthorized (Microsoft.OData.ODataError odataError)
	protected virtual Microsoft.AspNetCore.OData.Results.UnauthorizedODataResult Unauthorized (string message)
	protected virtual Microsoft.AspNetCore.OData.Results.UnprocessableEntityODataResult UnprocessableEntity (Microsoft.OData.ODataError odataError)
	protected virtual Microsoft.AspNetCore.OData.Results.UnprocessableEntityODataResult UnprocessableEntity (string message)
	protected virtual UpdatedODataResult`1 Updated (TEntity entity)
}

public class Microsoft.AspNetCore.OData.Routing.Controllers.MetadataController : Microsoft.AspNetCore.Mvc.ControllerBase {
	public MetadataController ()

	[
	HttpGetAttribute(),
	]
	public Microsoft.OData.Edm.IEdmModel GetMetadata ()

	[
	HttpGetAttribute(),
	]
	public Microsoft.OData.ODataServiceDocument GetServiceDocument ()
}

public interface Microsoft.AspNetCore.OData.Routing.Conventions.IODataControllerActionConvention {
	int Order  { public abstract get; }

	bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public abstract class Microsoft.AspNetCore.OData.Routing.Conventions.OperationRoutingConvention : IODataControllerActionConvention {
	protected OperationRoutingConvention ()

	int Order  { public abstract get; }

	protected static void AddSelector (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context, Microsoft.OData.Edm.IEdmOperation edmOperation, bool hasKeyParameter, Microsoft.OData.Edm.IEdmEntityType entityType, Microsoft.OData.Edm.IEdmNavigationSource navigationSource, Microsoft.OData.Edm.IEdmEntityType castType)
	public abstract bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected abstract bool IsOperationParameterMatched (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
	protected void ProcessOperations (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context, Microsoft.OData.Edm.IEdmEntityType entityType, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.ActionRoutingConvention : Microsoft.AspNetCore.OData.Routing.Conventions.OperationRoutingConvention, IODataControllerActionConvention {
	public ActionRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool IsOperationParameterMatched (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.AttributeRoutingConvention : IODataControllerActionConvention {
	public AttributeRoutingConvention (Microsoft.Extensions.Logging.ILogger`1[[Microsoft.AspNetCore.OData.Routing.Conventions.AttributeRoutingConvention]] logger, Microsoft.AspNetCore.OData.Routing.Parser.IODataPathTemplateParser parser)

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.EntityRoutingConvention : IODataControllerActionConvention {
	public EntityRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.EntitySetRoutingConvention : IODataControllerActionConvention {
	public EntitySetRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool CanApplyDollarCount (Microsoft.OData.Edm.IEdmEntitySet entitySet, Microsoft.AspNetCore.OData.Routing.ODataRouteOptions routeOptions)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.FunctionRoutingConvention : Microsoft.AspNetCore.OData.Routing.Conventions.OperationRoutingConvention, IODataControllerActionConvention {
	public FunctionRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool IsOperationParameterMatched (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.MetadataRoutingConvention : IODataControllerActionConvention {
	public MetadataRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.NavigationRoutingConvention : IODataControllerActionConvention {
	public NavigationRoutingConvention (Microsoft.Extensions.Logging.ILogger`1[[Microsoft.AspNetCore.OData.Routing.Conventions.NavigationRoutingConvention]] logger)

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool CanApplyDollarCount (Microsoft.OData.Edm.IEdmNavigationProperty edmProperty, string method, Microsoft.AspNetCore.OData.Routing.ODataRouteOptions routeOptions)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext {
	public ODataControllerActionContext (string prefix, Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)

	Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel Action  { public get; public set; }
	Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel Controller  { public get; }
	Microsoft.OData.Edm.IEdmEntitySet EntitySet  { public get; }
	Microsoft.OData.Edm.IEdmEntityType EntityType  { public get; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; public set; }
	Microsoft.AspNetCore.OData.ODataOptions Options  { public get; public set; }
	string Prefix  { public get; }
	Microsoft.OData.Edm.IEdmSingleton Singleton  { public get; }
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.OperationImportRoutingConvention : IODataControllerActionConvention {
	public OperationImportRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.PropertyRoutingConvention : IODataControllerActionConvention {
	public PropertyRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool CanApply (Microsoft.OData.Edm.IEdmProperty edmProperty, string method, Microsoft.AspNetCore.OData.Routing.ODataRouteOptions routeOptions)
	protected virtual bool CanApplyDollarCount (Microsoft.OData.Edm.IEdmProperty edmProperty, string method, Microsoft.AspNetCore.OData.Routing.ODataRouteOptions routeOptions)
	protected virtual bool CanApplyDollarValue (Microsoft.OData.Edm.IEdmProperty edmProperty, string method, Microsoft.AspNetCore.OData.Routing.ODataRouteOptions routeOptions)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.RefRoutingConvention : IODataControllerActionConvention {
	public RefRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.SingletonRoutingConvention : IODataControllerActionConvention {
	public SingletonRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	public virtual bool AppliesToController (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
}

public interface Microsoft.AspNetCore.OData.Routing.Parser.IODataPathTemplateParser {
	Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate Parse (Microsoft.OData.Edm.IEdmModel model, string odataPath, System.IServiceProvider requestProvider)
}

public class Microsoft.AspNetCore.OData.Routing.Parser.DefaultODataPathTemplateParser : IODataPathTemplateParser {
	public DefaultODataPathTemplateParser ()

	public virtual Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate Parse (Microsoft.OData.Edm.IEdmModel model, string odataPath, System.IServiceProvider requestProvider)
}

public interface Microsoft.AspNetCore.OData.Routing.Template.IODataTemplateTranslator {
	Microsoft.OData.UriParser.ODataPath Translate (Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate path, Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public abstract class Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	protected ODataSegmentTemplate ()

	public abstract System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public abstract bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.ActionImportSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public ActionImportSegmentTemplate (Microsoft.OData.UriParser.OperationImportSegment segment)
	public ActionImportSegmentTemplate (Microsoft.OData.Edm.IEdmActionImport actionImport, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmActionImport ActionImport  { public get; }
	Microsoft.OData.UriParser.OperationImportSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.ActionSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public ActionSegmentTemplate (Microsoft.OData.UriParser.OperationSegment segment)
	public ActionSegmentTemplate (Microsoft.OData.Edm.IEdmAction action, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmAction Action  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	Microsoft.OData.UriParser.OperationSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.CastSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public CastSegmentTemplate (Microsoft.OData.UriParser.TypeSegment typeSegment)
	public CastSegmentTemplate (Microsoft.OData.Edm.IEdmType castType, Microsoft.OData.Edm.IEdmType expectedType, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmType CastType  { public get; }
	Microsoft.OData.Edm.IEdmType ExpectedType  { public get; }
	Microsoft.OData.UriParser.TypeSegment TypeSegment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.CountSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	Microsoft.AspNetCore.OData.Routing.Template.CountSegmentTemplate Instance  { public static get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.DynamicSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public DynamicSegmentTemplate (Microsoft.OData.UriParser.DynamicPathSegment segment)

	Microsoft.OData.UriParser.DynamicPathSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.EntitySetSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public EntitySetSegmentTemplate (Microsoft.OData.Edm.IEdmEntitySet entitySet)
	public EntitySetSegmentTemplate (Microsoft.OData.UriParser.EntitySetSegment segment)

	Microsoft.OData.Edm.IEdmEntitySet EntitySet  { public get; }
	Microsoft.OData.UriParser.EntitySetSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.FunctionImportSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public FunctionImportSegmentTemplate (Microsoft.OData.UriParser.OperationImportSegment segment)
	public FunctionImportSegmentTemplate (Microsoft.OData.Edm.IEdmFunctionImport functionImport, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)
	public FunctionImportSegmentTemplate (System.Collections.Generic.IDictionary`2[[System.String],[System.String]] parameters, Microsoft.OData.Edm.IEdmFunctionImport functionImport, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmFunctionImport FunctionImport  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.FunctionSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public FunctionSegmentTemplate (Microsoft.OData.UriParser.OperationSegment operationSegment)
	public FunctionSegmentTemplate (Microsoft.OData.Edm.IEdmFunction function, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)
	public FunctionSegmentTemplate (System.Collections.Generic.IDictionary`2[[System.String],[System.String]] parameters, Microsoft.OData.Edm.IEdmFunction function, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmFunction Function  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] ParameterMappings  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.KeySegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public KeySegmentTemplate (Microsoft.OData.UriParser.KeySegment segment)
	public KeySegmentTemplate (Microsoft.OData.UriParser.KeySegment segment, System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.Edm.IEdmProperty]] keyProperties)
	public KeySegmentTemplate (System.Collections.Generic.IDictionary`2[[System.String],[System.String]] keys, Microsoft.OData.Edm.IEdmEntityType entityType, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	int Count  { public get; }
	Microsoft.OData.Edm.IEdmEntityType EntityType  { public get; }
	System.Collections.Generic.IDictionary`2[[System.String],[System.String]] KeyMappings  { public get; }
	System.Collections.Generic.IDictionary`2[[System.String],[Microsoft.OData.Edm.IEdmProperty]] KeyProperties  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.MetadataSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	Microsoft.AspNetCore.OData.Routing.Template.MetadataSegmentTemplate Instance  { public static get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.NavigationLinkSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public NavigationLinkSegmentTemplate (Microsoft.OData.UriParser.NavigationPropertyLinkSegment segment)
	public NavigationLinkSegmentTemplate (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.AspNetCore.OData.Routing.Template.KeySegmentTemplate Key  { public get; public set; }
	Microsoft.OData.Edm.IEdmNavigationProperty NavigationProperty  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	Microsoft.OData.UriParser.NavigationPropertyLinkSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.NavigationLinkTemplateSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public NavigationLinkTemplateSegmentTemplate (Microsoft.OData.Edm.IEdmStructuredType declaringType, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmStructuredType DeclaringType  { public get; }
	Microsoft.OData.Edm.IEdmNavigationSource NavigationSource  { public get; }
	string RelatedKey  { public get; public set; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.NavigationSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public NavigationSegmentTemplate (Microsoft.OData.UriParser.NavigationPropertySegment segment)
	public NavigationSegmentTemplate (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)

	Microsoft.OData.Edm.IEdmNavigationProperty NavigationProperty  { public get; }
	Microsoft.OData.UriParser.NavigationPropertySegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate : System.Collections.Generic.List`1[[Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate]], ICollection, IEnumerable, IList, ICollection`1, IEnumerable`1, IList`1, IReadOnlyCollection`1, IReadOnlyList`1 {
	public ODataPathTemplate (Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate[] segments)
	public ODataPathTemplate (System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate]] segments)
	public ODataPathTemplate (System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate]] segments)

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (params Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
}

public class Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext {
	public ODataTemplateTranslateContext (Microsoft.AspNetCore.Http.HttpContext context, Microsoft.AspNetCore.Http.Endpoint endpoint, Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues, Microsoft.OData.Edm.IEdmModel model)

	Microsoft.AspNetCore.Http.Endpoint Endpoint  { public get; }
	Microsoft.AspNetCore.Http.HttpContext HttpContext  { public get; }
	Microsoft.OData.Edm.IEdmModel Model  { public get; }
	Microsoft.AspNetCore.Routing.RouteValueDictionary RouteValues  { public get; }
	System.Collections.Generic.IList`1[[Microsoft.OData.UriParser.ODataPathSegment]] Segments  { public get; }
	Microsoft.AspNetCore.Routing.RouteValueDictionary UpdatedValues  { public get; }

	public string GetParameterAliasOrSelf (string alias)
}

public class Microsoft.AspNetCore.OData.Routing.Template.PathTemplateSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public PathTemplateSegmentTemplate (Microsoft.OData.UriParser.PathTemplateSegment segment)

	string ParameterName  { public get; }
	Microsoft.OData.UriParser.PathTemplateSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.PropertyCatchAllSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public PropertyCatchAllSegmentTemplate (Microsoft.OData.Edm.IEdmStructuredType declaredType)

	Microsoft.OData.Edm.IEdmStructuredType StructuredType  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.PropertySegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public PropertySegmentTemplate (Microsoft.OData.Edm.IEdmStructuralProperty property)
	public PropertySegmentTemplate (Microsoft.OData.UriParser.PropertySegment segment)

	Microsoft.OData.Edm.IEdmStructuralProperty Property  { public get; }
	Microsoft.OData.UriParser.PropertySegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.SingletonSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public SingletonSegmentTemplate (Microsoft.OData.Edm.IEdmSingleton singleton)
	public SingletonSegmentTemplate (Microsoft.OData.UriParser.SingletonSegment segment)

	Microsoft.OData.UriParser.SingletonSegment Segment  { public get; }
	Microsoft.OData.Edm.IEdmSingleton Singleton  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

public class Microsoft.AspNetCore.OData.Routing.Template.ValueSegmentTemplate : Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentTemplate {
	public ValueSegmentTemplate (Microsoft.OData.Edm.IEdmType previousType)
	public ValueSegmentTemplate (Microsoft.OData.UriParser.ValueSegment segment)

	Microsoft.OData.UriParser.ValueSegment Segment  { public get; }

	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetTemplates (Microsoft.AspNetCore.OData.Routing.ODataRouteOptions options)
	public virtual bool TryTranslate (Microsoft.AspNetCore.OData.Routing.Template.ODataTemplateTranslateContext context)
}

