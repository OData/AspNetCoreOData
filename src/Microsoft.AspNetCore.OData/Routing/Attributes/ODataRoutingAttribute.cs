// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;

namespace Microsoft.AspNetCore.OData.Routing.Attributes
{
    /// <summary>
    /// Represents an attribute that can be placed on Controller or action to specify that's OData controller or OData action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ODataRoutingAttribute : Attribute
    {
        // The design:
        // ODataController has this attribute, all controllers derived from ODataController will be considered as OData attribute routing.

        // If any other controller decorated with this attribute, the route template of all actions will be considered as OData attribute routing.
        // If any action decorated with this attribute, the route template of this action will be considered as OData attribute routing.

        // If you want to mix asp.net core attribute and odata attribute routing, consider to create two methods in the controller.
        // If you want to opt one action out OData attribute routing, using [NonODataAction] attribute.
    }

    public class ODataModelContext
    {
        public IServiceProvider ServiceProvider { get; set; } = default!;
        public ControllerModel ControllerModel { get; set; } = default!;

        public ActionModel ActionModel { get; set; } = default!;

        //   public ActionDescriptor ActionDescriptor { get; set; } = default!;

        //   public HttpContext HttpContext { get; set; } = default!;
    }

    public interface IODataModelProvider
    {
        string Prefix { get; }

        IEdmModel GetEdmModel(ODataModelContext context);

        /// <summary>
        /// The Service provider for OData
        /// </summary>
        IServiceProvider SeviceProvider { get; }

        Func<string, ODataPathTemplate> TemplateParser { get; }
    }

    public abstract class ODataModelProviderAttribute : Attribute, IODataModelProvider
    {
        protected ODataModelProviderAttribute()
            : this(string.Empty)
        { }

        protected ODataModelProviderAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }

        public IServiceProvider SeviceProvider { get; set; }

        public abstract IEdmModel GetEdmModel(ODataModelContext context);

        public Func<string, ODataPathTemplate> TemplateParser { get; set; }

    }

    public class ODataRoutingConvention : Attribute, IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            // Maybe we can use this method?
            throw new NotImplementedException();
        }
    }
}
