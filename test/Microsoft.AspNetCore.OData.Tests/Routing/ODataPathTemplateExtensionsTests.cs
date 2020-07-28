// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing
{
    public class ODataPathTemplateExtensionsTests
    {

        [Fact]
        public void TestPathTemplateFunction()
        {
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            // VipCustomer
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet("Customers", customer);

            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmEntityTypeReference(vipCustomer, false));
            getSalaray.AddParameter("salary", intType);
            getSalaray.AddOptionalParameter("minSalary", intType);
            getSalaray.AddOptionalParameter("maxSalary", intType);
            getSalaray.AddOptionalParameter("aveSalary", intType, "129");

            ODataPathTemplate template = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                new KeySegmentTemplate(customer, entitySet),
                new CastSegmentTemplate(vipCustomer, customer, entitySet),
                new FunctionSegmentTemplate(getSalaray, null));

            var templates = template.GetAllTemplates();
            Assert.Equal(32, templates.Count());
        }

        [Fact]
        public void TestFunction()
        {
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            getSalaray.AddParameter("salary", intType);
            getSalaray.AddParameter("minSalary", intType);
            getSalaray.AddParameter("maxSalary", intType);
            getSalaray.AddParameter("aveSalary", intType);

            var items = getSalaray.GenerateFunctionPath();

            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void TestFunction2()
        {
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            getSalaray.AddParameter("salary", intType);
            getSalaray.AddOptionalParameter("minSalary", intType);
            getSalaray.AddOptionalParameter("maxSalary", intType);
            getSalaray.AddOptionalParameter("aveSalary", intType, "129");

            var items = getSalaray.GenerateFunctionPath();

            Assert.Equal(16, items.Count);
        }
    }
}
