//using QueryBuilder.Extensions;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate OData queries based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class ODataQueryValidator : IODataQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="options">The OData query to validate.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(ODataQueryOptionsFundamentals options, ODataValidationSettings validationSettings)
        {
            if (options == null)
            {
                throw Error.ArgumentNull("options");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            // Validate each query options
            if (options.Compute != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Compute, validationSettings.AllowedQueryOptions);
                options.Compute.Validate(validationSettings);
            }

            if (options.Apply != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Apply, validationSettings.AllowedQueryOptions);
            }

            if (options.Skip != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Skip, validationSettings.AllowedQueryOptions);
                options.Skip.Validate(validationSettings);
            }

            if (options.Top != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Top, validationSettings.AllowedQueryOptions);
                options.Top.Validate(validationSettings);
            }

            if (options.OrderBy != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.OrderBy, validationSettings.AllowedQueryOptions);
                options.OrderBy.Validate(validationSettings);
            }

            if (options.Filter != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Filter, validationSettings.AllowedQueryOptions);
                options.Filter.Validate(validationSettings);
            }

            if (options.Search != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Search, validationSettings.AllowedQueryOptions);
            }

            ODataPath path = options.QueryContext.Path;
            bool isCountRequest = path != null && path.LastSegment is CountSegment;
            if (options.Count != null || isCountRequest)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Count, validationSettings.AllowedQueryOptions);

                if (options.Count != null)
                {
                    options.Count.Validate(validationSettings);
                }
            }

            if (options.SkipToken != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.SkipToken, validationSettings.AllowedQueryOptions);
                options.SkipToken.Validate(validationSettings);
            }

            if (options.RawValues.Expand != null)
            {
                ValidateNotEmptyOrWhitespace(options.RawValues.Expand);
                ValidateQueryOptionAllowed(AllowedQueryOptions.Expand, validationSettings.AllowedQueryOptions);
            }

            if (options.RawValues.Select != null)
            {
                ValidateNotEmptyOrWhitespace(options.RawValues.Select);
                ValidateQueryOptionAllowed(AllowedQueryOptions.Select, validationSettings.AllowedQueryOptions);
            }

            if (options.SelectExpand != null)
            {
                options.SelectExpand.Validate(validationSettings);
            }

            if (options.RawValues.Format != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Format, validationSettings.AllowedQueryOptions);
            }

            if (options.RawValues.SkipToken != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.SkipToken, validationSettings.AllowedQueryOptions);
            }

            if (options.RawValues.DeltaToken != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.DeltaToken, validationSettings.AllowedQueryOptions);
            }
        }

        private static void ValidateQueryOptionAllowed(AllowedQueryOptions queryOption, AllowedQueryOptions allowed)
        {
            if ((queryOption & allowed) == AllowedQueryOptions.None)
            {
                throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, queryOption, "AllowedQueryOptions"));
            }
        }

        private static void ValidateNotEmptyOrWhitespace(string rawValue)
        {
            if (rawValue != null && string.IsNullOrWhiteSpace(rawValue))
            {
                throw new ODataException(SRResources.SelectExpandEmptyOrWhitespace);
            }
        }
    }
}
