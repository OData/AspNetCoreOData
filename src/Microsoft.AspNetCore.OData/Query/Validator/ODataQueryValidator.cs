//-----------------------------------------------------------------------------
// <copyright file="ODataQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
    /// <param name="validationErrors">When this method returns, contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns>True if validation is successful; otherwise, false.</returns>
    public virtual bool TryValidate(ODataQueryOptions options, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        List<string> errors = null;

        if (options == null || validationSettings == null)
        {
            // Pre-allocate with a reasonable default capacity.
            errors = new List<string>(2);

            // Validate input parameters
            if (options == null)
            {
                errors.Add(Error.ArgumentNull(nameof(options)).Message);
            }

            if (validationSettings == null)
            {
                errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
            }

            validationErrors = errors;
            return false;
        }

        // To prevent duplicates in the `errors` list, ensure that each error is unique before adding it.
        // Modify the `AddValidationErrors` helper function to check for duplicates.
        void AddValidationErrors(IEnumerable<string> queryOptionErrors)
        {
            if (queryOptionErrors == null)
            {
                return;
            }

            // Pre-allocate with a reasonable default capacity.
            errors ??= new List<string>(4);

            // If errors list is not empty, we need to ensure uniqueness.
            var uniqueErrors = queryOptionErrors
                .Where(error => !errors.Any(e => e == error));

            errors.AddRange(uniqueErrors);
        }

        // Validate each query option and add errors if any.
        if (options.Compute != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Compute, validationSettings.AllowedQueryOptions, out var computeErrors);
            AddValidationErrors(computeErrors);

            if (!options.Compute.TryValidate(validationSettings, out var computeValidationErrors))
            {
                AddValidationErrors(computeValidationErrors);
            }
        }

        if (options.Apply?.ApplyClause != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Apply, validationSettings.AllowedQueryOptions, out var applyErrors);
            AddValidationErrors(applyErrors);
        }

        if (options.Skip != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Skip, validationSettings.AllowedQueryOptions, out var skipErrors);
            AddValidationErrors(skipErrors);

            if (!options.Skip.TryValidate(validationSettings, out var skipValidationErrors))
            {
                AddValidationErrors(skipValidationErrors);
            }
        }

        if (options.Top != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Top, validationSettings.AllowedQueryOptions, out var topErrors);
            AddValidationErrors(topErrors);

            if (!options.Top.TryValidate(validationSettings, out var topValidationErrors))
            {
                AddValidationErrors(topValidationErrors);
            }
        }

        if (options.OrderBy != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.OrderBy, validationSettings.AllowedQueryOptions, out var orderByErrors);
            AddValidationErrors(orderByErrors);

            if (!options.OrderBy.TryValidate(validationSettings, out var orderByValidationErrors))
            {
                AddValidationErrors(orderByValidationErrors);
            }
        }

        if (options.Filter != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Filter, validationSettings.AllowedQueryOptions, out var filterErrors);
            AddValidationErrors(filterErrors);

            if (!options.Filter.TryValidate(validationSettings, out var filterValidationErrors))
            {
                AddValidationErrors(filterValidationErrors);
            }
        }

        if (options.Search != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Search, validationSettings.AllowedQueryOptions, out var searchErrors);
            AddValidationErrors(searchErrors);

            if (!options.Search.TryValidate(validationSettings, out var searchValidationErrors))
            {
                AddValidationErrors(searchValidationErrors);
            }
        }

        if (options.Count != null || options.Request.IsCountRequest())
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Count, validationSettings.AllowedQueryOptions, out var countErrors);
            AddValidationErrors(countErrors);

            if (options.Count?.TryValidate(validationSettings, out var countValidationErrors) == false)
            {
                AddValidationErrors(countValidationErrors);
            }
        }

        if (options.SkipToken != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.SkipToken, validationSettings.AllowedQueryOptions, out var skipTokenErrors);
            AddValidationErrors(skipTokenErrors);

            if (!options.SkipToken.TryValidate(validationSettings, out var skipTokenValidationErrors))
            {
                AddValidationErrors(skipTokenValidationErrors);
            }
        }

        if (options.RawValues.Expand != null)
        {
            TryValidateNotEmptyOrWhitespace(options.RawValues.Expand, ref errors);
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Expand, validationSettings.AllowedQueryOptions, out var expandErrors);
            AddValidationErrors(expandErrors);
        }

        if (options.RawValues.Select != null)
        {
            TryValidateNotEmptyOrWhitespace(options.RawValues.Select, ref errors);
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Select, validationSettings.AllowedQueryOptions, out var selectErrors);
            AddValidationErrors(selectErrors);
        }

        if (options.SelectExpand != null)
        {
            if (!options.SelectExpand.TryValidate(validationSettings, out var selectExpandValidationErrors))
            {
                AddValidationErrors(selectExpandValidationErrors);
            }
        }

        if (options.RawValues.Format != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.Format, validationSettings.AllowedQueryOptions, out var formatErrors);
            AddValidationErrors(formatErrors);
        }

        if (options.RawValues.DeltaToken != null)
        {
            TryValidateQueryOptionAllowed(AllowedQueryOptions.DeltaToken, validationSettings.AllowedQueryOptions, out var deltaTokenErrors);
            AddValidationErrors(deltaTokenErrors);
        }

        validationErrors = errors ?? Enumerable.Empty<string>(); // Avoid allocating a new empty list.
        return errors == null || errors.Count == 0;
    }


    private static void ValidateQueryOptionAllowed(AllowedQueryOptions queryOption, AllowedQueryOptions allowed)
    {
        if ((queryOption & allowed) == AllowedQueryOptions.None)
        {
            throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, queryOption, nameof(AllowedQueryOptions)));
        }
    }

    private static void TryValidateQueryOptionAllowed(AllowedQueryOptions queryOption, AllowedQueryOptions allowed, out IEnumerable<string> validationErrors)
    {
        List<string> errors = new List<string>();
        if ((queryOption & allowed) == AllowedQueryOptions.None)
        {
            errors.Add(Error.Format(SRResources.NotAllowedQueryOption, queryOption, nameof(AllowedQueryOptions)));
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

    private static void TryValidateNotEmptyOrWhitespace(string rawValue, ref List<string> validationErrors)
    {
        if (rawValue != null && string.IsNullOrWhiteSpace(rawValue))
        {
            validationErrors.Add(SRResources.SelectExpandEmptyOrWhitespace);
        }
    }
}
