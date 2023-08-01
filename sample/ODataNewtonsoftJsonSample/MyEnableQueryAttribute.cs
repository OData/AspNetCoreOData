//-----------------------------------------------------------------------------
// <copyright file="WeatherForecast.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using System;

namespace ODataNewtonsoftJsonSample
{
    public class MyEnableQueryAttribute : EnableQueryAttribute
    {
        public override  IEdmModel GetModel(Type elementClrType,
            HttpRequest request,
            ActionDescriptor actionDescriptor)
        {
            IEdmModel model = request.GetModel();
            if (model != null)
            {
                return model;
            }

            if (elementClrType == typeof(WeatherForecast))
            {
                return EdmModelBuilder.GetModel();
            }

            return base.GetModel(elementClrType, request, actionDescriptor);
        }
    }
}
