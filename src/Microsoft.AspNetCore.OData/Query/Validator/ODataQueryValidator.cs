//-----------------------------------------------------------------------------
// <copyright file="ODataQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

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
    public virtual void Validate(ODataQueryOptions options, ODataValidationSettings validationSettings)
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

        if (options.Apply?.ApplyClause != null)
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
            options.Search.Validate(validationSettings);
        }

        if (options.Count != null || options.Request.IsCountRequest())
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

    /// <summary>
    /// Try validates the OData query.
    /// </summary>
    /// <param name="options">The OData query to validate.</param>
    /// <param name="validationSettings">The settings used for validation.</param>
    /// <param name="validationErrors">The collection of validation errors.</param>
    /// <returns>True if validation is successful; otherwise, false.</returns>
    public virtual bool TryValidate(ODataQueryOptions options, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors)
    {
        List<ODataException> errors = new List<ODataException>();

        // Validate input parameters
        if (options == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(options)).Message));
        }

        if (validationSettings == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(validationSettings)).Message));
        }

        // If there are parameter errors, return early
        if (errors.Count > 0)
        {
            validationErrors = errors;
            return false;
        }

        // Helper function to aggregate errors
        void AddValidationErrors(IEnumerable<ODataException> queryOptionErrors)
        {
            if (queryOptionErrors != null)
            {
                errors.AddRange(queryOptionErrors);
            }
        }

        // Validate each query option
        if (options.Compute != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Compute, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> computeErrors);
            AddValidationErrors(computeErrors);

            if (!options.Compute.TryValidate(validationSettings, out IEnumerable<ODataException> computeValidationErrors))
            {
                AddValidationErrors(computeValidationErrors);
            }
        }

        if (options.Apply?.ApplyClause != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Apply, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> applyErrors);
            AddValidationErrors(applyErrors);
        }

        if (options.Skip != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Skip, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> skipErrors);
            AddValidationErrors(skipErrors);

            if (!options.Skip.TryValidate(validationSettings, out IEnumerable<ODataException> skipValidationErrors))
            {
                AddValidationErrors(skipValidationErrors);
            }
        }

        if (options.Top != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Top, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> topErrors);
            AddValidationErrors(topErrors);

            if (!options.Top.TryValidate(validationSettings, out IEnumerable<ODataException> topValidationErrors))
            {
                AddValidationErrors(topValidationErrors);
            }
        }

        if (options.OrderBy != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.OrderBy, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> orderByErrors);
            AddValidationErrors(orderByErrors);

            if (!options.OrderBy.TryValidate(validationSettings, out IEnumerable<ODataException> orderByValidationErrors))
            {
                AddValidationErrors(orderByValidationErrors);
            }
        }

        if (options.Filter != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Filter, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> filterErrors);
            AddValidationErrors(filterErrors);

            if (!options.Filter.TryValidate(validationSettings, out IEnumerable<ODataException> filterValidationErrors))
            {
                AddValidationErrors(filterValidationErrors);
            }
        }

        if (options.Search != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Search, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> searchErrors);
            AddValidationErrors(searchErrors);

            if (!options.Search.TryValidate(validationSettings, out IEnumerable<ODataException> searchValidationErrors))
            {
                AddValidationErrors(searchValidationErrors);
            }
        }

        if (options.Count != null || options.Request.IsCountRequest())
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Count, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> countErrors);
            AddValidationErrors(countErrors);

            if (options.Count?.TryValidate(validationSettings, out IEnumerable<ODataException> countValidationErrors) == false)
            {
                AddValidationErrors(countValidationErrors);
            }
        }

        if (options.SkipToken != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.SkipToken, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> skipTokenErrors);
            AddValidationErrors(skipTokenErrors);

            if (!options.SkipToken.TryValidate(validationSettings, out IEnumerable<ODataException> skipTokenValidationErrors))
            {
                AddValidationErrors(skipTokenValidationErrors);
            }
        }

        if (options.RawValues.Expand != null)
        {
            TryValidateNotEmptyOrWhitespace(options.RawValues.Expand, errors);
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Expand, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> expandErrors);
            AddValidationErrors(expandErrors);
        }

        if (options.RawValues.Select != null)
        {
            TryValidateNotEmptyOrWhitespace(options.RawValues.Select, errors);
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Select, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> selectErrors);
            AddValidationErrors(selectErrors);
        }

        if (options.SelectExpand != null)
        {
            if (!options.SelectExpand.TryValidate(validationSettings, out IEnumerable<ODataException> selectExpandValidationErrors))
            {
                AddValidationErrors(selectExpandValidationErrors);
            }
        }

        if (options.RawValues.Format != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Format, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> formatErrors);
            AddValidationErrors(formatErrors);
        }

        if (options.RawValues.DeltaToken != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.DeltaToken, validationSettings.AllowedQueryOptions, out IEnumerable<ODataException> deltaTokenErrors);
            AddValidationErrors(deltaTokenErrors);
        }

        validationErrors = errors;
        return errors.Count == 0;
    }


    private static void ValidateQueryOptionAllowed(AllowedQueryOptions queryOption, AllowedQueryOptions allowed)
    {
        if ((queryOption & allowed) == AllowedQueryOptions.None)
        {
            throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, queryOption, nameof(AllowedQueryOptions)));
        }
    }

    private static void TryValidateQueryOptionAllowed(AllowedQueryOptions queryOption, AllowedQueryOptions allowed, out IEnumerable<ODataException> validationErrors)
    {
        List<ODataException> errors = new List<ODataException>();
        if ((queryOption & allowed) == AllowedQueryOptions.None)
        {
            errors.Add(new ODataException(Error.Format(SRResources.NotAllowedQueryOption, queryOption, nameof(AllowedQueryOptions))));
        }

        validationErrors = errors;
    }

    private static void ValidateNotEmptyOrWhitespace(string rawValue)
    {
        if (rawValue != null && string.IsNullOrWhiteSpace(rawValue))
        {
            throw new ODataException(SRResources.SelectExpandEmptyOrWhitespace);
        }
    }

    private static void TryValidateNotEmptyOrWhitespace(string rawValue, List<ODataException> validationErrors)
    {
        if (rawValue != null && string.IsNullOrWhiteSpace(rawValue))
        {
            validationErrors.Add(new ODataException(SRResources.SelectExpandEmptyOrWhitespace));
        }
    }
}
