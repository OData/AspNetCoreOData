//-----------------------------------------------------------------------------
// <copyright file="PeopleController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;

namespace microsoft.graph;

public class recoveryPreviewJobsController : ODataController
{
    private recoveryPreviewJob[] recoveryPreviewJobs = 
    {
        new recoveryPreviewJob()
    };

    [HttpGet]
    public recoveryPreviewJob Get(string id)
    {
        return recoveryPreviewJobs[0];
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(recoveryPreviewJobs);
    }

    [HttpGet]
    public IActionResult getChanges([FromRoute] string key, ODataQueryOptions<recoveryChangeObject> queryOptions)
    {
        var result = new EdmChangedObjectCollection(GraphModel.recoveryChangeObjectType);
        //could alternatively use DeltaSet<T>
        //var result = new DeltaSet<recoveryChangeObject>();

        // Get IQueryable<recoveryChangeObject> of the changes and apply filter, orderby, etc. to that queryable
        // IQueryable<recoveryChangeObject> changeObjects = getChangeObjects();
        // var filteredChanges = queryOptions.ApplyTo(changeObjects);
        // Loop through the recoveryChangeObject instances and generate the delta payload
        // foreach(recoveryChangeObject changeObject in filteredChanges)...

        // example: create recoveryChangeObject for changed user
        var recoveryChangeObject = new EdmEntityObject(GraphModel.recoveryChangeObjectType);
        // could alternatively do Delta<recoveryChangeObject>
        // var recoveryChangeObject = new Delta<recoveryChangeObject>();
        // Set properties
        // recoveryChangeObject.TrySetPropertyValue("id", "1");
        recoveryChangeObject.TrySetPropertyValue("currentState", getUser());
        recoveryChangeObject.TrySetPropertyValue("deltaFromCurrent", getChangedUser());
        // Add to result
        result.Add(recoveryChangeObject);

        // Create recoveryChangeObject for group w/members
        recoveryChangeObject = new EdmEntityObject(GraphModel.recoveryChangeObjectType);
        // alternatively:
        // recoveryChangeObject = new Delta<recoveryChangeObject>();
        // Set Properties
        recoveryChangeObject.TrySetPropertyValue("id", "2");
        recoveryChangeObject.TrySetPropertyValue("currentState", getGroup());
        recoveryChangeObject.TrySetPropertyValue("deltaFromCurrent", getChangedGroup());
        // Add to result
        result.Add(recoveryChangeObject);

        return Ok(result);
    }
    
    //TODO: getChanges isn't found as a valid segment
    //[HttpGet("recoveryChangeObjects/{id}/microsoft.graph.getChanges()/{key}/deltaFromCurrent")]
    [EnableQuery]
    public IActionResult getChangesFromRecoveryChangeObject([FromRoute] string id, [FromRoute] string key)
    {
        return Ok(getChangedUser());
    }

    // Example function to get a user
    private EdmEntityObject getUser()
    {
        EdmEntityObject user = new EdmEntityObject(GraphModel.userType);
        user.TrySetPropertyValue("id", "user1");
        user.TrySetPropertyValue("displayName", "William");
        return user;
    }

    // Example function to get a group
    private EdmEntityObject getGroup()
    {
        EdmEntityObject user = new EdmEntityObject(GraphModel.groupType);
        user.TrySetPropertyValue("id", "group1");
        var members = new EdmEntityObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(EdmCoreModel.Instance.GetEntityType(),false))));
        var member1 = new EdmEntityObject(GraphModel.userType);
        member1.TrySetPropertyValue("id", "user7");
        member1.TrySetPropertyValue("email", "user7@users.com");
        members.Add(member1);
        user.TrySetPropertyValue("members",members);
        return user;
    }

    // Example function showing creating a changed user
    private EdmDeltaResourceObject getChangedUser()
    {
        var changedUser = new EdmDeltaResourceObject(GraphModel.userType);
        changedUser.TrySetPropertyValue("id", "user1");
        changedUser.TrySetPropertyValue("displayName", "Bill");
        return changedUser;
    }

    // Example function showing creating a changed group
    private EdmDeltaResourceObject getChangedGroup() {
        var members = new EdmChangedObjectCollection(EdmCoreModel.Instance.GetEntityType());

        var addedMember = new EdmDeltaResourceObject(GraphModel.userType);
        addedMember.TrySetPropertyValue("id", "user3");
        members.Add(addedMember);

        var deletedMember = new EdmDeltaDeletedResourceObject(GraphModel.userType);        
        deletedMember.Id = new Uri("https://graph.microsoft.com/v1.0/users/4");
        deletedMember.TrySetPropertyValue("id", "user4");
        members.Add(deletedMember);

        var group = new EdmDeltaResourceObject(GraphModel.groupType);
        group.TrySetPropertyValue("id", "group1");
        group.TrySetPropertyValue("members", members);
        return group;
    }
}
