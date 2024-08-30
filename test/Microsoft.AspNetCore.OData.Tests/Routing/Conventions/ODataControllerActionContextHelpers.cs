//-----------------------------------------------------------------------------
// <copyright file="ODataControllerActionContextHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions;

internal class ODataControllerActionContextHelpers
{
    public static ODataControllerActionContext BuildContext(string modelPrefix, IEdmModel model, ControllerModel controller)
    {
        Assert.NotNull(model);

        // The reason why to create a context is that:
        // We don't need to call te FindEntitySet or FindSingleton before every convention.
        // So, for a controller, we try to call "FindEntitySet" or "FindSingleton" once.
        string controllerName = controller.ControllerName;
        ODataControllerActionContext context = new ODataControllerActionContext(modelPrefix, model, controller);

        IEdmEntitySet entitySet = model.EntityContainer?.FindEntitySet(controllerName);
        if (entitySet != null)
        {
            context.NavigationSource = entitySet;
        }

        IEdmSingleton singleton = model.EntityContainer?.FindSingleton(controllerName);
        if (singleton != null)
        {
            context.NavigationSource = singleton;
        }

        return context;
    }
}
