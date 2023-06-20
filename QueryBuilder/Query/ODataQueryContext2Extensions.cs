using System.Linq;
using QueryBuilder.Abstracts;
using QueryBuilder.Query.Expressions;
using QueryBuilder.Query.Validator;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;

namespace QueryBuilder.Query
{
    internal static class ODataQueryContext2Extensions
    {
        //public static ODataQuerySettings GetODataQuerySettings(this ODataQueryContext2 context)
        //{
        //    ODataQuerySettings returnSettings = new ODataQuerySettings();
        //    ODataQuerySettings settings = context?.RequestContainer?.GetRequiredService<ODataQuerySettings>();
        //    if (settings != null)
        //    {
        //        returnSettings.CopyFrom(settings);
        //    }

        //    return returnSettings;
        //}


        // QUESTION: To remove unused parameter, should this no longer be a Context extension method? If so, where should it go?
        public static ODataQuerySettings UpdateQuerySettings(this ODataQueryFundamentalsContext context, ODataQuerySettings querySettings, IQueryable query)
        {
            ODataQuerySettings updatedSettings = new ODataQuerySettings();

            // QUESTION: Doesn't this always get overriden? What's the point then?
            // Look at history for this file/method to see if i can figure out reasoning
            // - Commit: https://github.com/OData/AspNetCoreOData/commit/c48a776b5818b36d983abe61b4aaf30761a3216b#diff-163dd061aeec926a287d9ea683e9be25f263c02989731faa52b7f1b6bd540dbd
            // - Seems like there's no corresponding PR
            //ODataQuerySettings settings = context?.RequestContainer?.GetRequiredService<ODataQuerySettings>();
            //if (settings != null)
            //{
            //    updatedSettings.CopyFrom(settings);
            //}

            updatedSettings.CopyFrom(querySettings);

            if (updatedSettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings.HandleNullPropagation = query != null
                    ? HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query)
                    : HandleNullPropagationOption.True;
            }

            return updatedSettings;
        }

        ////public static SkipTokenHandler GetSkipTokenHandler(this ODataQueryContext2 context)
        ////{
        ////    return context?.RequestContainer?.GetRequiredService<SkipTokenHandler>() ?? DefaultSkipTokenHandler.Instance;
        ////}

        ///// <summary>
        ///// Gets the <see cref="IFilterBinder"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IFilterBinder"/>.</returns>
        //public static IFilterBinder GetFilterBinder(this ODataQueryContext2 context)
        //{
        //    if (context == null)
        //    {
        //        throw Error.ArgumentNull(nameof(context));
        //    }

        //    IFilterBinder binder = context.RequestContainer?.GetService<IFilterBinder>();
        //    return binder ?? new FilterBinder();
        //}

        ///// <summary>
        ///// Gets the <see cref="ISearchBinder"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ISearchBinder"/>.</returns>
        //public static ISearchBinder GetSearchBinder(this ODataQueryContext2 context)
        //{
        //    if (context == null)
        //    {
        //        throw Error.ArgumentNull(nameof(context));
        //    }

        //    // We don't provide the default implementation of ISearchBinder,
        //    // Actually, how to match is dependent upon the implementation.
        //    return context.RequestContainer?.GetService<ISearchBinder>();
        //}

        ///// <summary>
        ///// Gets the <see cref="ISelectExpandBinder"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ISelectExpandBinder"/>.</returns>
        //public static ISelectExpandBinder GetSelectExpandBinder(this ODataQueryContext2 context)
        //{
        //    if (context == null)
        //    {
        //        throw Error.ArgumentNull(nameof(context));
        //    }

        //    ISelectExpandBinder binder = context.RequestContainer?.GetService<ISelectExpandBinder>();

        //    return binder ?? new SelectExpandBinder(context.GetFilterBinder(), context.GetOrderByBinder());
        //}

        ///// <summary>
        ///// Gets the <see cref="IOrderByBinder"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IOrderByBinder"/>.</returns>
        //public static IOrderByBinder GetOrderByBinder(this ODataQueryContext2 context)
        //{
        //    if (context == null)
        //    {
        //        throw Error.ArgumentNull(nameof(context));
        //    }

        //    IOrderByBinder binder = context.RequestContainer?.GetService<IOrderByBinder>();

        //    return binder ?? new OrderByBinder();
        //}

        ///// <summary>
        ///// Gets the <see cref="IAssemblyResolver"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IAssemblyResolver"/>.</returns>
        //public static IAssemblyResolver GetAssemblyResolver(this ODataQueryContext2 context)
        //{
        //    if (context == null)
        //    {
        //        throw Error.ArgumentNull(nameof(context));
        //    }

        //    IAssemblyResolver resolver = context.RequestContainer?.GetService<IAssemblyResolver>();

        //    return resolver ?? AssemblyResolverHelper.Default;
        //}

        ///// <summary>
        ///// Gets the <see cref="IODataQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IODataQueryValidator"/>.</returns>
        //public static IODataQueryValidator GetODataQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<IODataQueryValidator>()
        //        ?? new ODataQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="IComputeQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IComputeQueryValidator"/>.</returns>
        //public static IComputeQueryValidator GetComputeQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<IComputeQueryValidator>()
        //        ?? new ComputeQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="ICountQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ICountQueryValidator"/>.</returns>
        //public static ICountQueryValidator GetCountQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<ICountQueryValidator>()
        //        ?? new CountQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="IFilterQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IFilterQueryValidator"/>.</returns>
        //public static IFilterQueryValidator GetFilterQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<IFilterQueryValidator>()
        //        ?? new FilterQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="IOrderByQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="IOrderByQueryValidator"/>.</returns>
        //public static IOrderByQueryValidator GetOrderByQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<IOrderByQueryValidator>()
        //        ?? new OrderByQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="ISkipQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ISkipQueryValidator"/>.</returns>
        //public static ISkipQueryValidator GetSkipQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<ISkipQueryValidator>()
        //        ?? new SkipQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="ISkipTokenQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ISkipTokenQueryValidator"/>.</returns>
        //public static ISkipTokenQueryValidator GetSkipTokenQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<ISkipTokenQueryValidator>()
        //        ?? new SkipTokenQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="ITopQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ITopQueryValidator"/>.</returns>
        //public static ITopQueryValidator GetTopQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<ITopQueryValidator>()
        //        ?? new TopQueryValidator();
        //}

        ///// <summary>
        ///// Gets the <see cref="ISelectExpandQueryValidator"/>.
        ///// </summary>
        ///// <param name="context">The query context.</param>
        ///// <returns>The built <see cref="ISelectExpandQueryValidator"/>.</returns>
        //public static ISelectExpandQueryValidator GetSelectExpandQueryValidator(this ODataQueryContext2 context)
        //{
        //    return context?.RequestContainer?.GetService<ISelectExpandQueryValidator>()
        //        ?? new SelectExpandQueryValidator();
        //}
    }
}
