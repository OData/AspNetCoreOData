namespace OData2Linq.Helpers
{
    using Microsoft.AspNetCore.OData;
    using Microsoft.AspNetCore.OData.Query;
    using Microsoft.OData;
    using OData2Linq.Settings;
    using System.Linq;

    internal static class TopSkipHelper
    {
        public static IQueryable<T> ApplyTopWithValidation<T>(IQueryable<T> query, long? top, ODataSettings settings)
        {
            if (top.HasValue)
            {
                if (top.Value > int.MaxValue)
                {
                    throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, int.MaxValue, AllowedQueryOptions.Top, top.Value));
                }

                if (top.Value > settings.ValidationSettings.MaxTop)
                {
                    throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, settings.ValidationSettings.MaxTop, AllowedQueryOptions.Top, top.Value));
                }

                return (IQueryable<T>)ExpressionHelpers.Take(query, (int)top.Value, typeof(T), settings.QuerySettings.EnableConstantParameterization);
            }

            return query;
        }

        public static IQueryable<T> ApplySkipWithValidation<T>(IQueryable<T> query, long? skip, ODataSettings settings)
        {
            if (skip.HasValue)
            {
                if (skip.Value > int.MaxValue)
                {
                    throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, int.MaxValue, AllowedQueryOptions.Skip, skip.Value));
                }

                if (skip.Value > settings.ValidationSettings.MaxSkip)
                {
                    throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, settings.ValidationSettings.MaxSkip, AllowedQueryOptions.Skip, skip.Value));
                }

                return (IQueryable<T>)ExpressionHelpers.Skip(query, (int)skip.Value, typeof(T), settings.QuerySettings.EnableConstantParameterization);
            }

            return query;
        }
    }
}