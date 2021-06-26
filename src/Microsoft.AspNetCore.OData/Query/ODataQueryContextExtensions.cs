// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Query
{
    internal static class ODataQueryContextExtensions
    {
        public static ODataQuerySettings UpdateQuerySettings(this ODataQueryContext context, ODataQuerySettings querySettings, IQueryable query)
        {
            ODataQuerySettings updatedSettings = (context == null || context.RequestContainer == null)
                ? new ODataQuerySettings()
                : context.RequestContainer.GetRequiredService<ODataQuerySettings>();

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
            if (context == null || context.RequestContainer == null)
            {
                return DefaultSkipTokenHandler.Instance;
            }

            return context.RequestContainer.GetRequiredService<SkipTokenHandler>();
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
    }
}