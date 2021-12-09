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
        public static ODataQuerySettings GetODataQuerySettings(this ODataQueryContext context)
        {
            ODataQuerySettings returnSettings = new ODataQuerySettings();
            ODataQuerySettings settings = context?.RequestContainer?.GetRequiredService<ODataQuerySettings>();
            if (settings != null)
            {
                returnSettings.CopyFrom(settings);
            }

            if (returnSettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                returnSettings.HandleNullPropagation = HandleNullPropagationOption.True;
            }

            return returnSettings;
        }

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
    }
}
