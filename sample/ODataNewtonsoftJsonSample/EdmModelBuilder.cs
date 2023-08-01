//-----------------------------------------------------------------------------
// <copyright file="WeatherForecast.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataNewtonsoftJsonSample
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _model;

        public static IEdmModel GetModel()
        {
            if (_model == null)
            {
                var builder = new ODataConventionModelBuilder();
                builder.EntitySet<WeatherForecast>(nameof(WeatherForecast));
                builder.EntityType<WeatherForecast>().Ignore(c => c.Mac); // remove it from OData side
                builder.EntityType<WeatherForecast>().Property(c => c.ODataMac).Name = "Mac"; // use this line to make the property name as "Mac" at OData side
                _model = builder.GetEdmModel();
            }

            return _model;
        }
    }
}
