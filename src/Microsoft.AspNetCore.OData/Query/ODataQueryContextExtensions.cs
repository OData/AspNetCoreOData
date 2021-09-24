//-----------------------------------------------------------------------------
// <copyright file="ODataQueryContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Query
{
    internal static class ODataQueryContextExtensions
    {
        public static ODataQuerySettings UpdateQuerySettings(this ODataQueryContext context, ODataQuerySettings querySettings, IQueryable query)
        {
            ODataQuerySettings updatedSettings =
                context?.RequestContainer?.GetRequiredService<ODataQuerySettings>() ?? new ODataQuerySettings();

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
        /// <param name="querySettings">The query setting.</param>
        /// <returns>The <see cref="IFilterBinder"/>.</returns>
        public static IFilterBinder GetFilterBinder(this ODataQueryContext context, ODataQuerySettings querySettings)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull(nameof(querySettings));
            }

            IFilterBinder binder = null;
            if (context.RequestContainer != null)
            {
                binder = context.RequestContainer.GetService<IFilterBinder>();

                FilterBinder filterBinder = binder as FilterBinder;
                if (filterBinder != null)
                {
                    if (filterBinder.Model != context.Model)
                    {
                        // TODO: Wtf, Need refactor these codes?
                        filterBinder.Model = context.Model;
                    }

                    return filterBinder;
                }
            }

            return binder ?? new FilterBinder(querySettings, AssemblyResolverHelper.Default, context.Model);
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

            return binder ?? new SelectExpandBinder();
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
    }
}
