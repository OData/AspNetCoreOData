
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace ODataRoutingSample.Extensions
{
    public class SetBaseRoutingConvention : IODataControllerActionConvention
    {
        public virtual int Order => 0;

        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            return context.Controller.ControllerName == "People";
        }

        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context.Prefix != string.Empty)
            {
                return false;
            }

            if (context.Action.ActionName != "FindPerson")
            {
                return false;
            }

            IEdmEntitySet entitySet = context.Model.EntityContainer.FindEntitySet("People");

            ODataPathTemplate path = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                new FilterSegmentTemplate()
                );

            context.Action.AddSelector("Get", context.Prefix, context.Model, path);
            return true;
        }
    }
}
