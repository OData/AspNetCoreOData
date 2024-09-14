//-----------------------------------------------------------------------------
// <copyright file="ODataNewtonsoftJsonMvcBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson;

/// <summary>
/// Extension methods for adding OData Json converter to Newtonsoft.Json to <see cref="IMvcBuilder"/> and <see cref="IMvcCoreBuilder"/>.
/// </summary>
public static class ODataNewtonsoftJsonMvcBuilderExtensions
{
    #region IMvcBuilder
    /// <summary>
    /// Configures Newtonsoft.Json using OData Json converter.
    /// </summary>
    /// <param name="builder">The Mvc builder.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddODataNewtonsoftJson(this IMvcBuilder builder)
    {
        return builder.AddODataNewtonsoftJson(null);
    }

    /// <summary>
    /// Configures Newtonsoft.Json using OData Json converter.
    /// </summary>
    /// <param name="builder">The Mvc builder.</param>
    /// <param name="mapperProvider">The mapper provider.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddODataNewtonsoftJson(this IMvcBuilder builder,
        Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AddNewtonsoftJson(BuildSetupAction(mapperProvider));
    }
    #endregion

    #region IMvcCoreBuilder
    /// <summary>
    /// Configures Newtonsoft.Json using OData Json converter.
    /// </summary>
    /// <param name="builder">The Mvc core builder.</param>
    /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
    public static IMvcCoreBuilder AddODataNewtonsoftJson(this IMvcCoreBuilder builder)
    {
        return builder.AddODataNewtonsoftJson(null);
    }

    /// <summary>
    /// Configures Newtonsoft.Json using OData Json converter.
    /// </summary>
    /// <param name="builder">The Mvc core builder.</param>
    /// <param name="mapperProvider">The mapper provider.</param>
    /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
    public static IMvcCoreBuilder AddODataNewtonsoftJson(this IMvcCoreBuilder builder,
        Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AddNewtonsoftJson(BuildSetupAction(mapperProvider));
    }
    #endregion

    private static Action<MvcNewtonsoftJsonOptions> BuildSetupAction(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
    {
        Action<MvcNewtonsoftJsonOptions> odataSetupAction = opt =>
        {
            if (mapperProvider is null)
            {
                opt.SerializerSettings.Converters.Add(new JSelectExpandWrapperConverter());
            }
            else
            {
                opt.SerializerSettings.Converters.Add(new JSelectExpandWrapperConverter(mapperProvider));
            }

            opt.SerializerSettings.Converters.Add(new JDynamicTypeWrapperConverter());
            opt.SerializerSettings.Converters.Add(new JPageResultValueConverter());
            opt.SerializerSettings.Converters.Add(new JSingleResultValueConverter());
        };

        return odataSetupAction;
    }
}
