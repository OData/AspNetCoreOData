// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
{
    public class AttributeRoutingConventionTests
    {
        private static IEdmModel EdmModel = GetEdmModel();
        private static RefRoutingConvention _ref = new RefRoutingConvention();

        [Fact]
        public void ConstructorThrows()
        {
            AttributeRoutingConvention convention = CreateConvention();
            Assert.NotNull(convention);
        }

        [Fact]
        public void Tests()
        {
            string actionMethodName = "CreateRef";
            string method = RefRoutingConvention.SplitRefActionName(actionMethodName, out string prop, out string declaring);
            Assert.Equal("CreateRef", method);
            Assert.Null(prop);
            Assert.Null(declaring);

            actionMethodName = "GetRefToPropertyFromAbc";
            method = RefRoutingConvention.SplitRefActionName(actionMethodName, out prop, out declaring);
            Assert.Equal("GetRef", method);
            Assert.Equal("Property", prop);
            Assert.Equal("Abc", declaring);

            actionMethodName = "CreateRefFromAbcToProperty";
            method = RefRoutingConvention.SplitRefActionName(actionMethodName, out prop, out declaring);
            Assert.Null(method);
        }

        [Fact]
        public void AppliesToControllerWithoutRoutePrefixWorksAsExpected()
        {
            Type controllerType = typeof(AttributeRoutingConvention1Controller);

            MethodInfo methodInfo = controllerType.GetMethod("MyAction");
            var attributes = methodInfo.GetCustomAttributes(inherit: true);
            ActionModel action = new ActionModel(methodInfo, attributes);

            ODataControllerActionContext context = BuildContext<AttributeRoutingConvention1Controller>(EdmModel);
            context.Controller.Actions.Add(action);

            AttributeRoutingConvention attributeConvention = CreateConvention();

            bool ok = attributeConvention.AppliesToController(context);
            Assert.Equal(4, action.Selectors.Count); 
            Assert.False(ok);
        }

        [Fact]
        public void AppliesToControllerWithoutRoutePrefixWorksAsExpected()
        {
            Type controllerType = typeof(AttributeRoutingConvention1Controller);

            MethodInfo methodInfo = controllerType.GetMethod("MyAction");
            var attributes = methodInfo.GetCustomAttributes(inherit: true);
            ActionModel action = new ActionModel(methodInfo, attributes);

            ODataControllerActionContext context = BuildContext<AttributeRoutingConvention1Controller>(EdmModel);
            context.Controller.Actions.Add(action);

            AttributeRoutingConvention attributeConvention = CreateConvention();

            bool ok = attributeConvention.AppliesToController(context);
            Assert.Equal(4, action.Selectors.Count);
            Assert.False(ok);
        }

        private static ODataControllerActionContext BuildContext<T>(IEdmModel model)
        {
            ControllerModel controller = new ControllerModel(typeof(T).GetTypeInfo(), new List<object>());
            ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, model, controller);
            return context;
        }

        private AttributeRoutingConvention CreateConvention()
        {
            var services = new ServiceCollection()
                .AddLogging();

            services.AddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();
            services.AddSingleton<AttributeRoutingConvention>();

            return services.BuildServiceProvider().GetRequiredService<AttributeRoutingConvention>();
        }

        private AttributeRoutingConvention CreateConvention(Action<IServiceCollection> configAction)
        {
            var services = new ServiceCollection()
                .AddLogging();

            services.AddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();
            services.AddSingleton<AttributeRoutingConvention>();

            configAction?.Invoke(services);

            return services.BuildServiceProvider().GetRequiredService<AttributeRoutingConvention>();
        }

        private static IEdmModel GetEdmModel()
        {
            var model = new EdmModel();
            var customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.String));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);

            var container = new EdmEntityContainer("NS", "Default");
            container.AddEntitySet("Customers", customer);
            model.AddElement(container);
            return model;
        }

        private class AttributeRoutingConvention1Controller
        {
            [ODataRoute("Customers({key})")]
            [ODataRoute("Customers/{key}/Name")]
            public void MyAction(int key)
            {
            }
        }

        [ODataRoutePrefix("Customers")]
        [ODataRoutePrefix("Orders")]
        private class AttributeRoutingConvention2Controller
        {
            [ODataRoute("({key})")]
            [ODataRoute("")]
            public void List(int key)
            {
            }
        }

        private class AttributeRoutingConventionTestPathTemplateParser : IODataPathTemplateParser
        {
            public ODataPathTemplate Parse(IEdmModel model, string odataPath)
            {
                return new ODataPathTemplate();
            }
        }
    }
}