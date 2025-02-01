//-----------------------------------------------------------------------------
// <copyright file="ODataQueryContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Query;

internal static class ODataQueryContextExtensions
{
    public static ODataQuerySettings GetODataQuerySettings(this ODataQueryContext context)
    {
        ODataQuerySettings returnSettings = new ODataQuerySettings();
        ODataQuerySettings settings = context?.RequestContainer?.GetRequiredService<ODataQuerySettings>();
        if (settings != null)
        {
            returnSettings.CopyFrom(settings);
        }

        returnSettings.TimeZone = context.Request.GetTimeZoneInfo();

        return returnSettings;
    }

    public static ODataQuerySettings UpdateQuerySettings(this ODataQueryContext context, ODataQuerySettings querySettings, IQueryable query)
    {
        ODataQuerySettings updatedSettings = new ODataQuerySettings();
        ODataQuerySettings settings = context?.RequestContainer?.GetRequiredService<ODataQuerySettings>();
        if (settings != null)
        {
            updatedSettings.CopyFrom(settings);
        }

        updatedSettings.CopyFrom(querySettings);

        if (updatedSettings.HandleNullPropagation == HandleNullPropagationOption.Default)
        {
            updatedSettings.HandleNullPropagation = query != null
                ? HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query)
                : HandleNullPropagationOption.True;
        }

        return updatedSettings;
    }

    public static SkipTokenHandler GetSkipTokenHandler(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetRequiredService<SkipTokenHandler>() ?? DefaultSkipTokenHandler.Instance;
    }

    /// <summary>
    /// Gets the <see cref="IFilterBinder"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IFilterBinder"/>.</returns>
    public static IFilterBinder GetFilterBinder(this ODataQueryContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        IFilterBinder binder = context.RequestContainer?.GetService<IFilterBinder>();
        return binder ?? new FilterBinder();
    }

    /// <summary>
    /// Gets the <see cref="ISearchBinder"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ISearchBinder"/>.</returns>
    public static ISearchBinder GetSearchBinder(this ODataQueryContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        // We don't provide the default implementation of ISearchBinder,
        // Actually, how to match is dependent upon the implementation.
        return context.RequestContainer?.GetService<ISearchBinder>();
    }

    /// <summary>
    /// Gets the <see cref="ISelectExpandBinder"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ISelectExpandBinder"/>.</returns>
    public static ISelectExpandBinder GetSelectExpandBinder(this ODataQueryContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        ISelectExpandBinder binder = context.RequestContainer?.GetService<ISelectExpandBinder>();

        return binder ?? new SelectExpandBinder(context.GetFilterBinder(), context.GetOrderByBinder());
    }

    /// <summary>
    /// Gets the <see cref="IOrderByBinder"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IOrderByBinder"/>.</returns>
    public static IOrderByBinder GetOrderByBinder(this ODataQueryContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        IOrderByBinder binder = context.RequestContainer?.GetService<IOrderByBinder>();

        return binder ?? new OrderByBinder();
    }

    /// <summary>
    /// Gets the <see cref="IAssemblyResolver"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IAssemblyResolver"/>.</returns>
    public static IAssemblyResolver GetAssemblyResolver(this ODataQueryContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        IAssemblyResolver resolver = context.RequestContainer?.GetService<IAssemblyResolver>();

        return resolver ?? AssemblyResolverHelper.Default;
    }

    /// <summary>
    /// Gets the <see cref="IODataQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IODataQueryValidator"/>.</returns>
    public static IODataQueryValidator GetODataQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<IODataQueryValidator>()
            ?? new ODataQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="IComputeQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IComputeQueryValidator"/>.</returns>
    public static IComputeQueryValidator GetComputeQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<IComputeQueryValidator>()
            ?? new ComputeQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="ICountQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ICountQueryValidator"/>.</returns>
    public static ICountQueryValidator GetCountQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<ICountQueryValidator>()
            ?? new CountQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="IFilterQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IFilterQueryValidator"/>.</returns>
    public static IFilterQueryValidator GetFilterQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<IFilterQueryValidator>()
            ?? new FilterQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="IOrderByQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="IOrderByQueryValidator"/>.</returns>
    public static IOrderByQueryValidator GetOrderByQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<IOrderByQueryValidator>()
            ?? new OrderByQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="ISkipQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ISkipQueryValidator"/>.</returns>
    public static ISkipQueryValidator GetSkipQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<ISkipQueryValidator>()
            ?? new SkipQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="ISkipTokenQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ISkipTokenQueryValidator"/>.</returns>
    public static ISkipTokenQueryValidator GetSkipTokenQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<ISkipTokenQueryValidator>()
            ?? new SkipTokenQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="ITopQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ITopQueryValidator"/>.</returns>
    public static ITopQueryValidator GetTopQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<ITopQueryValidator>()
            ?? new TopQueryValidator();
    }

    /// <summary>
    /// Gets the <see cref="ISelectExpandQueryValidator"/>.
    /// </summary>
    /// <param name="context">The query context.</param>
    /// <returns>The built <see cref="ISelectExpandQueryValidator"/>.</returns>
    public static ISelectExpandQueryValidator GetSelectExpandQueryValidator(this ODataQueryContext context)
    {
        return context?.RequestContainer?.GetService<ISelectExpandQueryValidator>()
            ?? new SelectExpandQueryValidator();
    }
}
