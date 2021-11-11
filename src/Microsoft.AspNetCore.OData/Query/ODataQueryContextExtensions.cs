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
        /// Gets the <see cref="FilterBinder"/>.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="querySettings">The query setting.</param>
        /// <returns>The built <see cref="FilterBinder"/>.</returns>
        public static FilterBinder GetFilterBinder(this ODataQueryContext context, ODataQuerySettings querySettings)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull(nameof(querySettings));
            }

            FilterBinder binder = null;
            if (context.RequestContainer != null)
            {
                binder = context.RequestContainer.GetService<FilterBinder>();
                if (binder != null && binder.Model != context.Model)
                {
                    // TODO: Wtf, Need refactor these codes?
                    binder.Model = context.Model;
                }
            }

            return binder ?? new FilterBinder(querySettings, AssemblyResolverHelper.Default, context.Model);
        }


        /// <summary>
        /// Gets the <see cref="FilterBinder"/>.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The built <see cref="FilterBinder"/>.</returns>
        public static IFilterBinder GetFilterBinder2(this ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IFilterBinder binder = context.RequestContainer?.GetService<IFilterBinder>();
            return binder ?? new FilterBinder2();
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
        /// Gets the <see cref="IOrderByBinder"/>.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="querySettings">The query settings.</param>
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
    }
}
