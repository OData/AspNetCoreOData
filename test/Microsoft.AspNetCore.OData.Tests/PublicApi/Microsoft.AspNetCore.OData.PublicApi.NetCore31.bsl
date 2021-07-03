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

	public abstract System.Threading.Tasks.Task`1[[Microsoft.AspNetCore.OData.Batch.ODataBatchResponseItem]] SendRequestAsync (Microsoft.AspNetCore.Http.RequestDelegate handler)
	[
	AsyncStateMachineAttribute(),
	]
	public static System.Threading.Tasks.Task SendRequestAsync (Microsoft.AspNetCore.Http.RequestDelegate handler, Microsoft.AspNetCore.Http.HttpContext context, System.Collections.Generic.Dictionary`2[[System.String],[System.String]] contentIdToLocationMapping)
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
	System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	bool TryGetPropertyType (string name, out System.Type& type)
	bool TryGetPropertyValue (string name, out System.Object& value)
	bool TrySetPropertyValue (string name, object value)
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaDeletedLink : IDeltaLinkBase, IDeltaSetItem {
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaDeletedResource : IDelta, IDeltaSetItem {
	System.Uri Id  { public abstract get; public abstract set; }
	System.Nullable`1[[Microsoft.OData.DeltaDeletedEntryReason]] Reason  { public abstract get; public abstract set; }
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaLink : IDeltaLinkBase, IDeltaSetItem {
}

public interface Microsoft.AspNetCore.OData.Deltas.IDeltaLinkBase : IDeltaSetItem {
	string Relationship  { public abstract get; public abstract set; }
	System.Uri Source  { public abstract get; public abstract set; }
	System.Uri Target  { public abstract get; public abstract set; }
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
	public abstract System.Collections.Generic.IEnumerable`1[[System.String]] GetUnchangedPropertyNames ()
	public virtual bool TryGetMember (System.Dynamic.GetMemberBinder binder, out System.Object& result)
	public abstract bool TryGetPropertyType (string name, out System.Type& type)
	public abstract bool TryGetPropertyValue (string name, out System.Object& value)
	public virtual bool TrySetMember (System.Dynamic.SetMemberBinder binder, object value)
	public abstract bool TrySetPropertyValue (string name, object value)
}

public abstract class Microsoft.AspNetCore.OData.Deltas.DeltaLinkBase`1 : IDeltaLinkBase, IDeltaSetItem, ITypedDelta {
	protected DeltaLinkBase`1 ()
	protected DeltaLinkBase`1 (System.Type structuralType)

	System.Type ExpectedClrType  { public virtual get; }
	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public abstract get; }
	string Relationship  { public virtual get; public virtual set; }
	System.Uri Source  { public virtual get; public virtual set; }
	System.Type StructuredType  { public virtual get; }
	System.Uri Target  { public virtual get; public virtual set; }
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

	public virtual void Clear ()
	public void CopyChangedValues (T original)
	public void CopyUnchangedValues (T original)
	public virtual System.Collections.Generic.IEnumerable`1[[System.String]] GetChangedPropertyNames ()
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

public sealed class Microsoft.AspNetCore.OData.Deltas.DeltaDeletedLink`1 : DeltaLinkBase`1, IDeltaDeletedLink, IDeltaLinkBase, IDeltaSetItem, ITypedDelta {
	public DeltaDeletedLink`1 ()
	public DeltaDeletedLink`1 (System.Type structuralType)

	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
}

public sealed class Microsoft.AspNetCore.OData.Deltas.DeltaLink`1 : DeltaLinkBase`1, IDeltaLink, IDeltaLinkBase, IDeltaSetItem, ITypedDelta {
	public DeltaLink`1 ()
	public DeltaLink`1 (System.Type structuralType)

	Microsoft.AspNetCore.OData.Deltas.DeltaItemKind Kind  { public virtual get; }
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
	public static void SetModelName (Microsoft.OData.Edm.IEdmModel model, string name)
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

public class Microsoft.AspNetCore.OData.Edm.CustomAggregateMethodAnnotation {
	public CustomAggregateMethodAnnotation ()

	public Microsoft.AspNetCore.OData.Edm.CustomAggregateMethodAnnotation AddMethod (string methodToken, System.Collections.Generic.IDictionary`2[[System.Type],[System.Reflection.MethodInfo]] methods)
	public bool GetMethodInfo (string methodToken, System.Type returnType, out System.Reflection.MethodInfo& methodInfo)
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
	public static bool HasODataKeyParameter (Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action, Microsoft.OData.Edm.IEdmEntityType entityType, params string keyPrefix)

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
	AllFunctions = 535494655
	AllMathFunctions = 58720256
	AllStringFunctions = 1023
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
	All = 2047
	Apply = 1024
	Count = 64
	DeltaToken = 512
	Expand = 2
	Filter = 1
	Format = 128
	None = 0
	OrderBy = 8
	Select = 4
	Skip = 32
	SkipToken = 256
	Supported = 1535
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

public class Microsoft.AspNetCore.OData.Query.CountQueryOption {
	public CountQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.CountQueryValidator Validator  { public get; public set; }
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
	public static Microsoft.AspNetCore.Mvc.SerializableError CreateErrorResponse (string message, params System.Exception exception)
	public static Microsoft.OData.Edm.IEdmModel GetModel (System.Type elementClrType, Microsoft.AspNetCore.Http.HttpRequest request, Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
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

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.OData.UriParser.FilterClause FilterClause  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.FilterQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.ODataQueryContext {
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.Edm.IEdmType elementType, Microsoft.OData.UriParser.ODataPath path)
	public ODataQueryContext (Microsoft.OData.Edm.IEdmModel model, System.Type elementClrType, Microsoft.OData.UriParser.ODataPath path)

	Microsoft.OData.ModelBuilder.Config.DefaultQuerySettings DefaultQuerySettings  { public get; }
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
	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.AspNetCore.OData.Query.CountQueryOption Count  { public get; }
	Microsoft.AspNetCore.OData.Query.FilterQueryOption Filter  { public get; }
	Microsoft.AspNetCore.OData.Query.ETag IfMatch  { public virtual get; }
	Microsoft.AspNetCore.OData.Query.ETag IfNoneMatch  { public virtual get; }
	Microsoft.AspNetCore.OData.Query.OrderByQueryOption OrderBy  { public get; }
	Microsoft.AspNetCore.OData.Query.ODataRawQueryOptions RawValues  { public get; }
	Microsoft.AspNetCore.Http.HttpRequest Request  { public get; }
	Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption SelectExpand  { public get; }
	Microsoft.AspNetCore.OData.Query.SkipQueryOption Skip  { public get; }
	Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption SkipToken  { public get; }
	Microsoft.AspNetCore.OData.Query.TopQueryOption Top  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.ODataQueryValidator Validator  { public get; public set; }

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
	System.Nullable`1[[System.Int32]] PageSize  { public get; public set; }
}

public class Microsoft.AspNetCore.OData.Query.ODataRawQueryOptions {
	public ODataRawQueryOptions ()

	string Apply  { public get; }
	string Count  { public get; }
	string DeltaToken  { public get; }
	string Expand  { public get; }
	string Filter  { public get; }
	string Format  { public get; }
	string OrderBy  { public get; }
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

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	Microsoft.OData.UriParser.OrderByClause OrderByClause  { public get; }
	System.Collections.Generic.IList`1[[Microsoft.AspNetCore.OData.Query.OrderByNode]] OrderByNodes  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.OrderByQueryValidator Validator  { public get; public set; }

	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query)
	public IOrderedQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public System.Linq.IOrderedQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption {
	public SelectExpandQueryOption (string select, string expand, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	int LevelsMaxLiteralExpansionDepth  { public get; public set; }
	string RawExpand  { public get; }
	string RawSelect  { public get; }
	Microsoft.OData.UriParser.SelectExpandClause SelectExpandClause  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.SelectExpandQueryValidator Validator  { public get; public set; }

	public System.Linq.IQueryable ApplyTo (System.Linq.IQueryable queryable, Microsoft.AspNetCore.OData.Query.ODataQuerySettings settings)
	public object ApplyTo (object entity, Microsoft.AspNetCore.OData.Query.ODataQuerySettings settings)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.SkipQueryOption {
	public SkipQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.SkipQueryValidator Validator  { public get; public set; }
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
	Microsoft.AspNetCore.OData.Query.Validator.SkipTokenQueryValidator Validator  { public get; }

	public virtual IQueryable`1 ApplyTo (IQueryable`1 query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public virtual System.Linq.IQueryable ApplyTo (System.Linq.IQueryable query, Microsoft.AspNetCore.OData.Query.ODataQuerySettings querySettings, Microsoft.AspNetCore.OData.Query.ODataQueryOptions queryOptions)
	public void Validate (Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.TopQueryOption {
	public TopQueryOption (string rawValue, Microsoft.AspNetCore.OData.Query.ODataQueryContext context, Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser)

	Microsoft.AspNetCore.OData.Query.ODataQueryContext Context  { public get; }
	string RawValue  { public get; }
	Microsoft.AspNetCore.OData.Query.Validator.TopQueryValidator Validator  { public get; public set; }
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

public class Microsoft.AspNetCore.OData.Results.CreatedODataResult`1 : Microsoft.AspNetCore.Mvc.ActionResult, IActionResult {
	public CreatedODataResult`1 (T entity)

	T Entity  { public virtual get; }

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

public class Microsoft.AspNetCore.OData.Results.UpdatedODataResult`1 : Microsoft.AspNetCore.Mvc.ActionResult, IActionResult {
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

	bool EnableControllerNameCaseInsensitive  { public get; public set; }
	bool EnableKeyAsSegment  { public get; public set; }
	bool EnableKeyInParenthesis  { public get; public set; }
	bool EnableNonParenthesisForEmptyParameterFunction  { public get; public set; }
	bool EnableQualifiedOperationCall  { public get; public set; }
	bool EnableUnqualifiedOperationCall  { public get; public set; }
}

public sealed class Microsoft.AspNetCore.OData.Routing.ODataRoutingMetadata : IODataRoutingMetadata {
	public ODataRoutingMetadata (string prefix, Microsoft.OData.Edm.IEdmModel model, Microsoft.AspNetCore.OData.Routing.Template.ODataPathTemplate template)

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

	public virtual object ReadDeltaDeletedLink (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaDeletedLinkWrapper deletedLink, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
	public virtual object ReadDeltaLink (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataDeltaLinkWrapper link, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
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
	public virtual System.Collections.IEnumerable ReadResourceSet (Microsoft.AspNetCore.OData.Formatter.Wrapper.ODataResourceSetWrapper resourceSet, Microsoft.OData.Edm.IEdmStructuredTypeReference elementType, Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataDeserializerContext readContext)
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
	public virtual string CreateETag (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataNestedResourceInfo CreateNavigationLink (Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataAction CreateODataAction (Microsoft.OData.Edm.IEdmAction action, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataFunction CreateODataFunction (Microsoft.OData.Edm.IEdmFunction function, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataResource CreateResource (Microsoft.AspNetCore.OData.Formatter.Serialization.SelectExpandNode selectExpandNode, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.AspNetCore.OData.Formatter.Serialization.SelectExpandNode CreateSelectExpandNode (Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataStreamPropertyInfo CreateStreamProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
	public virtual Microsoft.OData.ODataProperty CreateStructuralProperty (Microsoft.OData.Edm.IEdmStructuralProperty structuralProperty, Microsoft.AspNetCore.OData.Formatter.ResourceContext resourceContext)
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
	public virtual System.Threading.Tasks.Task WriteObjectAsync (object graph, System.Type type, Microsoft.OData.ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)

	[
	AsyncStateMachineAttribute(),
	]
	public virtual System.Threading.Tasks.Task WriteObjectInlineAsync (object graph, Microsoft.OData.Edm.IEdmTypeReference expectedType, Microsoft.OData.ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
}

public class Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext {
	public ODataSerializerContext ()
	public ODataSerializerContext (Microsoft.AspNetCore.OData.Formatter.ResourceContext resource, Microsoft.OData.UriParser.SelectExpandClause selectExpandClause, Microsoft.OData.Edm.IEdmProperty edmProperty)

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

public class Microsoft.AspNetCore.OData.Query.Expressions.FilterBinder : Microsoft.AspNetCore.OData.Query.Expressions.ExpressionBinderBase {
	public FilterBinder (System.IServiceProvider requestContainer)

	System.Linq.Expressions.ParameterExpression Parameter  { protected virtual get; }

	public virtual System.Linq.Expressions.Expression Bind (Microsoft.OData.UriParser.QueryNode node)
	public virtual System.Linq.Expressions.Expression BindAllNode (Microsoft.OData.UriParser.AllNode allNode)
	public virtual System.Linq.Expressions.Expression BindAnyNode (Microsoft.OData.UriParser.AnyNode anyNode)
	public virtual System.Linq.Expressions.Expression BindBinaryOperatorNode (Microsoft.OData.UriParser.BinaryOperatorNode binaryOperatorNode)
	public virtual System.Linq.Expressions.Expression BindCollectionComplexNode (Microsoft.OData.UriParser.CollectionComplexNode collectionComplexNode)
	public virtual System.Linq.Expressions.Expression BindCollectionPropertyAccessNode (Microsoft.OData.UriParser.CollectionPropertyAccessNode propertyAccessNode)
	public virtual System.Linq.Expressions.Expression BindCollectionResourceCastNode (Microsoft.OData.UriParser.CollectionResourceCastNode node)
	public virtual System.Linq.Expressions.Expression BindConvertNode (Microsoft.OData.UriParser.ConvertNode convertNode)
	public virtual System.Linq.Expressions.Expression BindDynamicPropertyAccessQueryNode (Microsoft.OData.UriParser.SingleValueOpenPropertyAccessNode openNode)
	public virtual System.Linq.Expressions.Expression BindInNode (Microsoft.OData.UriParser.InNode inNode)
	public virtual System.Linq.Expressions.Expression BindNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty)
	public virtual System.Linq.Expressions.Expression BindNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, string propertyPath)
	public virtual System.Linq.Expressions.Expression BindPropertyAccessQueryNode (Microsoft.OData.UriParser.SingleValuePropertyAccessNode propertyAccessNode)
	public virtual System.Linq.Expressions.Expression BindRangeVariable (Microsoft.OData.UriParser.RangeVariable rangeVariable)
	public virtual System.Linq.Expressions.Expression BindSingleComplexNode (Microsoft.OData.UriParser.SingleComplexNode singleComplexNode)
	public virtual System.Linq.Expressions.Expression BindSingleResourceCastNode (Microsoft.OData.UriParser.SingleResourceCastNode node)
	public virtual System.Linq.Expressions.Expression BindSingleResourceFunctionCallNode (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node)
	public virtual System.Linq.Expressions.Expression BindUnaryOperatorNode (Microsoft.OData.UriParser.UnaryOperatorNode unaryOperatorNode)
}

public class Microsoft.AspNetCore.OData.Query.Validator.CountQueryValidator {
	public CountQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.CountQueryOption countQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.FilterQueryValidator {
	public FilterQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.FilterQueryOption filterQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	public virtual void Validate (Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings, Microsoft.OData.Edm.IEdmModel model)
	internal virtual void Validate (Microsoft.OData.Edm.IEdmProperty property, Microsoft.OData.Edm.IEdmStructuredType structuredType, Microsoft.OData.UriParser.FilterClause filterClause, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings, Microsoft.OData.Edm.IEdmModel model, Microsoft.OData.ModelBuilder.Config.DefaultQuerySettings querySettings)
	protected virtual void ValidateAllNode (Microsoft.OData.UriParser.AllNode allNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateAnyNode (Microsoft.OData.UriParser.AnyNode anyNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateArithmeticOperator (Microsoft.OData.UriParser.BinaryOperatorNode binaryNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateBinaryOperatorNode (Microsoft.OData.UriParser.BinaryOperatorNode binaryOperatorNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateCollectionComplexNode (Microsoft.OData.UriParser.CollectionComplexNode collectionComplexNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateCollectionPropertyAccessNode (Microsoft.OData.UriParser.CollectionPropertyAccessNode propertyAccessNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateCollectionResourceCastNode (Microsoft.OData.UriParser.CollectionResourceCastNode collectionResourceCastNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateConstantNode (Microsoft.OData.UriParser.ConstantNode constantNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateConvertNode (Microsoft.OData.UriParser.ConvertNode convertNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateLogicalOperator (Microsoft.OData.UriParser.BinaryOperatorNode binaryNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateNavigationPropertyNode (Microsoft.OData.UriParser.QueryNode sourceNode, Microsoft.OData.Edm.IEdmNavigationProperty navigationProperty, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateQueryNode (Microsoft.OData.UriParser.QueryNode node, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateRangeVariable (Microsoft.OData.UriParser.RangeVariable rangeVariable, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateSingleComplexNode (Microsoft.OData.UriParser.SingleComplexNode singleComplexNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateSingleResourceCastNode (Microsoft.OData.UriParser.SingleResourceCastNode singleResourceCastNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateSingleResourceFunctionCallNode (Microsoft.OData.UriParser.SingleResourceFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateSingleValueFunctionCallNode (Microsoft.OData.UriParser.SingleValueFunctionCallNode node, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateSingleValuePropertyAccessNode (Microsoft.OData.UriParser.SingleValuePropertyAccessNode propertyAccessNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
	protected virtual void ValidateUnaryOperatorNode (Microsoft.OData.UriParser.UnaryOperatorNode unaryOperatorNode, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings settings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.ODataQueryValidator {
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

public class Microsoft.AspNetCore.OData.Query.Validator.OrderByQueryValidator {
	public OrderByQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.OrderByQueryOption orderByOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.SelectExpandQueryValidator {
	public SelectExpandQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.SelectExpandQueryOption selectExpandQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.SkipQueryValidator {
	public SkipQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.SkipQueryOption skipQueryOption, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.SkipTokenQueryValidator {
	public SkipTokenQueryValidator ()

	public virtual void Validate (Microsoft.AspNetCore.OData.Query.SkipTokenQueryOption skipToken, Microsoft.AspNetCore.OData.Query.Validator.ODataValidationSettings validationSettings)
}

public class Microsoft.AspNetCore.OData.Query.Validator.TopQueryValidator {
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

	protected virtual CreatedODataResult`1 Created (TEntity entity)
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
	protected abstract bool IsOperationParameterMeet (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
	protected void ProcessOperations (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context, Microsoft.OData.Edm.IEdmEntityType entityType, Microsoft.OData.Edm.IEdmNavigationSource navigationSource)
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.ActionRoutingConvention : Microsoft.AspNetCore.OData.Routing.Conventions.OperationRoutingConvention, IODataControllerActionConvention {
	public ActionRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool IsOperationParameterMeet (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
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
}

public class Microsoft.AspNetCore.OData.Routing.Conventions.FunctionRoutingConvention : Microsoft.AspNetCore.OData.Routing.Conventions.OperationRoutingConvention, IODataControllerActionConvention {
	public FunctionRoutingConvention ()

	int Order  { public virtual get; }

	public virtual bool AppliesToAction (Microsoft.AspNetCore.OData.Routing.Conventions.ODataControllerActionContext context)
	protected virtual bool IsOperationParameterMeet (Microsoft.OData.Edm.IEdmOperation operation, Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel action)
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

