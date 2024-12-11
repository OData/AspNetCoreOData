//-----------------------------------------------------------------------------
// <copyright file="TenantsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v3;

public class TestEntitiesController : Controller
{
    /* HttpPatch http://localhost:5000/v3/TestEntities/1/Query
     * 
     * Request Body is: 

"results": [
    {
        "EmailClusterId@odata.type": "#Int64",
        "EmailClusterId": 2629759514
    },
    {
        "EmailClusterId@odata.type": "#Int64",
        "EmailClusterId": 2629759515
    }
]
}
     */
    [HttpPatch]
    public IActionResult PatchToQuery(int key, Delta<HuntingQueryResults> delta)
    {
        var changedPropertyNames = delta.GetChangedPropertyNames();

        HuntingQueryResults original = new HuntingQueryResults();
        delta.Patch(original);

        return Ok(key);
    }
}

public class User
{
    public int Id { get; set; }


    public int Id2 { get; set; }
    public int Id3{ get; set; }
    public int Id4 { get; set; }
    public int Id5 { get; set; }
    public int Id6 { get; set; }
    public int Id7 { get; set; }
    public int Id8 { get; set; }
    public int Id9 { get; set; }
    public int Id10 { get; set; }
    public int Id11 { get; set; }
    public int Id12 { get; set; }
    public int Id13 { get; set; }
    public int Id14 { get; set; }
    public int Id15{ get; set; }
    public int Id16 { get; set; }
    public int Id17 { get; set; }
    public int Id18 { get; set; }
    public int Id19 { get; set; }
    public int Id20 { get; set; }
    public int Id21 { get; set; }
    public int Id22 { get; set; }
    public int Id23 { get; set; }
    public int Id24 { get; set; }
    public int Property0 { get; set; }
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }
    public int Property4 { get; set; }
    public int Property5 { get; set; }
    public int Property6 { get; set; }
    public int Property7 { get; set; }
    public int Property8 { get; set; }
    public int Property9 { get; set; }
    public int Property10 { get; set; }
    public int Property11 { get; set; }
    public int Property12 { get; set; }
    public int Property13 { get; set; }
    public int Property14 { get; set; }
    public int Property15 { get; set; }
    public int Property16 { get; set; }
    public int Property17 { get; set; }
    public int Property18 { get; set; }
    public int Property19 { get; set; }
    public int Property20 { get; set; }
    public int Property21 { get; set; }
    public int Property22 { get; set; }
    public int Property23 { get; set; }
    public int Property24 { get; set; }
    public int Property25 { get; set; }
    public int Property26 { get; set; }
    public int Property27 { get; set; }
    public int Property28 { get; set; }
    public int Property29 { get; set; }
    public int Property30 { get; set; }
    public int Property31 { get; set; }
    public int Property32 { get; set; }
    public int Property33 { get; set; }
    public int Property34 { get; set; }
    public int Property35 { get; set; }
    public int Property36 { get; set; }
    public int Property37 { get; set; }
    public int Property38 { get; set; }
    public int Property39 { get; set; }
    public int Property40 { get; set; }
    public int Property41 { get; set; }
    public int Property42 { get; set; }
    public int Property43 { get; set; }
    public int Property44 { get; set; }
    public int Property45 { get; set; }
    public int Property46 { get; set; }
    public int Property47 { get; set; }
    public int Property48 { get; set; }
    public int Property49 { get; set; }
    public int Property50 { get; set; }
    public int Property51 { get; set; }
    public int Property52 { get; set; }
    public int Property53 { get; set; }
    public int Property54 { get; set; }
    public int Property55 { get; set; }
    public int Property56 { get; set; }
    public int Property57 { get; set; }
    public int Property58 { get; set; }
    public int Property59 { get; set; }
    public int Property60 { get; set; }
    public int Property61 { get; set; }
    public int Property62 { get; set; }
    public int Property63 { get; set; }
    public int Property64 { get; set; }
    public int Property65 { get; set; }
    public int Property66 { get; set; }
    public int Property67 { get; set; }
    public int Property68 { get; set; }
    public int Property69 { get; set; }
    public int Property70 { get; set; }
    public int Property71 { get; set; }
    public int Property72 { get; set; }
    public int Property73 { get; set; }
    public int Property74 { get; set; }
    public int Property75 { get; set; }
    public int Property76 { get; set; }
    public int Property77 { get; set; }
    public int Property78 { get; set; }
    public int Property79 { get; set; }
    public int Property80 { get; set; }
    public int Property81 { get; set; }
    public int Property82 { get; set; }
    public int Property83 { get; set; }
    public int Property84 { get; set; }
    public int Property85 { get; set; }
    public int Property86 { get; set; }
    public int Property87 { get; set; }
    public int Property88 { get; set; }
    public int Property89 { get; set; }
    public int Property90 { get; set; }
    public int Property91 { get; set; }
    public int Property92 { get; set; }
    public int Property93 { get; set; }
    public int Property94 { get; set; }
    public int Property95 { get; set; }
    public int Property96 { get; set; }
    public int Property97 { get; set; }
    public int Property98 { get; set; }
    public int Property99 { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class DemoController : ControllerBase
{
    private readonly IUserService userService;

    public DemoController(IUserService userService)
    {
        this.userService = userService;
    }

    [Authorize]
    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = false)]
    public ActionResult<System.Collections.Generic.IEnumerable<User>> GetUsers(ODataQueryOptions<User> options)
    {
        return Ok(options.ApplyTo(userService.AllUsers()));
    }
}

public interface IUserService
{
    IQueryable<User> AllUsers();
}

public class UserService : IUserService
{
    public IQueryable<User> AllUsers()
    {
        var users = new User[]
        {
            new User(),
        };
        return users.AsQueryable();
    }
}

public class Auth : AuthorizationHandler<RolesPolicy>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private List<string> _unauthorizedEntities;

    private static readonly ConcurrentDictionary<Type, IEdmModel> TypeToModelCache = new();

    public Auth(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesPolicy requirement)
    {
        _unauthorizedEntities = new List<string>();
        var isAuthorizedResults = new List<bool>
        {
            IsAuthorizedToController(context)
        };

        var expandedPropertyTypes = GetExpandProperties(context);
        isAuthorizedResults.Add(IsAuthorizedToExpandedProperty(context, expandedPropertyTypes));
        context.Succeed(requirement);
    }

    private bool IsAuthorizedToExpandedProperty(AuthorizationHandlerContext context, IEnumerable<Type> expandedPropertyTypes)
    {
        foreach (var expandedPropertyType in expandedPropertyTypes)
        {
            
        }

        return true;
    }

    private static string GetParentPropertyAssemblyFullName(AuthorizationHandlerContext context)
    {
        return ((context.Resource as DefaultHttpContext)?.GetEndpoint()?.Metadata.AsEnumerable().FirstOrDefault(item => item is User) as string) ?? "ODataRoutingSample.Controllers.v3.User";
    }

    private IEnumerable<Type> GetExpandProperties(AuthorizationHandlerContext context)
    {
        var parentTypeName = GetParentPropertyAssemblyFullName(context);
        var parentType = GetTypeByClassFullName(parentTypeName);
        var request = httpContextAccessor.HttpContext.Request;

        var builder = new ODataConventionModelBuilder(
            request.HttpContext.RequestServices.GetRequiredService<IAssemblyResolver>(),
            isQueryCompositionMode: true);

        // This code was the source of the memory leak since each model
        // instance is cached by OData to map CLR types to EDM types.
        // Creating a new model instance each time this method is called
        // causes the cache to grow indefinitely.
        //var entityTypeConfiguration = builder.AddEntityType(parentType);
        //builder.AddEntitySet(parentType.Name, entityTypeConfiguration);
        //var model = builder.GetEdmModel();

        // Fix the issue by caching the model instance based on the parent type.
        var model = TypeToModelCache.GetOrAdd(parentType, _ =>
        {
            var entityTypeConfiguration = builder.AddEntityType(parentType);
            builder.AddEntitySet(parentType.Name, entityTypeConfiguration);
            return builder.GetEdmModel();
        });


        var path = request.ODataFeature().Path;
        var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, parentType, path), request);

        if (queryOptions.SelectExpand?.RawExpand is null)
            return Enumerable.Empty<Type>();

        return GetExpandNamesFromSelectedItems(queryOptions.SelectExpand.SelectExpandClause.SelectedItems);
    }

    private IEnumerable<Type> GetExpandNamesFromSelectedItems(IEnumerable<SelectItem> selectedItems)
    {
        return selectedItems
            .Where(item => item is ExpandedNavigationSelectItem)
            .Cast<ExpandedNavigationSelectItem>()
            .Select(ExtractExpandNames)
            .SelectMany(item => item)
            .Distinct()
            .ToList();
    }
    private static string GetEdmType(IEdmType edmType)
    {
        if (edmType.TypeKind == EdmTypeKind.Collection)
            return edmType.AsElementType().FullTypeName();

        return edmType.FullTypeName();
    }

    private IEnumerable<Type> ExtractExpandNames(ExpandedNavigationSelectItem selectItem)
    {
        var list = new List<Type>();
        var type = GetTypeByClassFullName(GetEdmType(selectItem.PathToNavigationProperty.FirstSegment.EdmType));

        var selectedItems = selectItem.SelectAndExpand.SelectedItems;
        var entityTypes = GetExpandNamesFromSelectedItems(selectedItems);
        list.AddRange(entityTypes);

        list.Add(type);

        return list;
    }

    private static object GetRolesAttribute(AuthorizationHandlerContext context)
    {
        return (context.Resource as DefaultHttpContext)?.GetEndpoint()?.Metadata.AsEnumerable().FirstOrDefault(item => item is User);
    }

    private bool IsAuthorizedToController(AuthorizationHandlerContext context)
    {
        var roleAttribute = GetRolesAttribute(context);
        var parentTypeName = GetParentPropertyAssemblyFullName(context);
        var parentType = GetTypeByClassFullName(parentTypeName);

        return true;
    }

    public static Type GetTypeByClassFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
        {
            var type = assembly.GetType(fullName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
}

public class RolesPolicy : IAuthorizationRequirement
{
}

[ODataAttributeRouting]
[Route("v3")]
public class tenantsController : Controller
{
    [HttpPut("tenants/{tenantId}/devices/{deviceId}")]
    [HttpPut("tenants({tenantId})/devices({deviceId})")]
    public IActionResult PutToDevices(string tenantId, string deviceId)
    {
        return Ok($"PutTo Devices - tenantId={tenantId}: deviceId={deviceId}");
    }

    [HttpGet("tenants/{tenantId}/folders/{folderId}")]
    [HttpGet("tenants({tenantId})/folders({folderId})")]
    public IActionResult GetFolders(string tenantId, Guid folderId)
    {
        return Ok($"GetFolders - tenantId={tenantId}: folderId={folderId}");
    }

    [HttpGet("tenants/{tenantId}/pages/{pageId}")]
    [HttpGet("tenants({tenantId})/pages({pageId})")]
    public IActionResult GetDriverPages(string tenantId, int pageId)
    {
        // Example:
        // 1) ~/v3/tenants/23281137-7a37-4c2f-ad57-a511f38dea09/pages/2  ==> works
        // 2) ~/v3/tenants/'23281137-7a37-4c2f-ad57-a511f38dea09'/pages/2  ==> works
        // 3) ~/v3/tenants/'23281137-7a37-4c2f-ad57-a511f38dea09'/pages/ab  ==> throw exception on 'ab'
        return Ok($"GetDriverPages - tenantId={tenantId}: pageId={pageId}");
    }
}
