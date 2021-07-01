// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Expressions;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Query
{
    internal static class ODataQueryContextExtensions
    {
        public static ODataQuerySettings UpdateQuerySettings(this ODataQueryContext context, ODataQuerySettings querySettings, IQueryable query)
        {
            ODataQuerySettings updatedSettings = context?.Request?.GetService<ODataQuerySettings>() ?? new ODataQuerySettings();

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
            return context?.Request?.GetService<SkipTokenHandler>() ?? DefaultSkipTokenHandler.Instance;
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
                binder = context?.Request?.GetService<FilterBinder>();
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