//-----------------------------------------------------------------------------
// <copyright file="DollarApplyEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    public class DollarApplyEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            
            builder.EntitySet<Category>("Categories");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Sale>("Sales");
            builder.EntitySet<Employee>("Employees");
            builder.ComplexType<Address>();
            builder.Singleton<Company>("Company");

            var model = builder.GetEdmModel();

            var stdevMethodAnnotation = new CustomAggregateMethodAnnotation();
            var stdevMethod = new Dictionary<Type, MethodInfo>
            {
                {
                    typeof(decimal),
                    typeof(DollarApplyCustomMethods).GetMethod("StdDev", BindingFlags.Static | BindingFlags.Public)
                }
            };

            stdevMethodAnnotation.AddMethod("custom.stdev", stdevMethod);
            model.SetAnnotationValue(model, stdevMethodAnnotation);

            return model;
        }
    }
}
