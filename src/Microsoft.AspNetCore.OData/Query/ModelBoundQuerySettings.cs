//-----------------------------------------------------------------------------
// <copyright file="ModelBoundQuerySettings.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Config;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This class describes the model bound settings to use during query composition.
/// </summary>
internal static class ModelBoundQuerySettingsExtensions
{
    /// <summary>
    /// Copy the <see cref="ExpandConfiguration"/>s of navigation properties.
    /// </summary>
    internal static void CopyExpandConfigurations(this ModelBoundQuerySettings settings, Dictionary<string, ExpandConfiguration> expandConfigurations)
    {
        settings.ExpandConfigurations.Clear();
        foreach (var expandConfiguration in expandConfigurations)
        {
            settings.ExpandConfigurations.Add(expandConfiguration.Key, expandConfiguration.Value);
        }
    }

    /// <summary>
    /// Copy the $orderby configuration of properties.
    /// </summary>
    internal static void CopyOrderByConfigurations(this ModelBoundQuerySettings settings, Dictionary<string, bool> orderByConfigurations)
    {
        settings.OrderByConfigurations.Clear();
        foreach (var orderByConfiguration in orderByConfigurations)
        {
            settings.OrderByConfigurations.Add(orderByConfiguration.Key, orderByConfiguration.Value);
        }
    }

    /// <summary>
    /// Copy the $select configuration of properties.
    /// </summary>
    internal static void CopySelectConfigurations(this ModelBoundQuerySettings settings, Dictionary<string, SelectExpandType> selectConfigurations)
    {
        settings.SelectConfigurations.Clear();
        foreach (var selectConfiguration in selectConfigurations)
        {
            settings.SelectConfigurations.Add(selectConfiguration.Key, selectConfiguration.Value);
        }
    }

    /// <summary>
    /// Copy the $filter configuration of properties.
    /// </summary>
    internal static void CopyFilterConfigurations(this ModelBoundQuerySettings settings, Dictionary<string, bool> filterConfigurations)
    {
        settings.FilterConfigurations.Clear();
        foreach (var filterConfiguration in filterConfigurations)
        {
            settings.FilterConfigurations.Add(filterConfiguration.Key, filterConfiguration.Value);
        }
    }

    internal static bool IsAutomaticExpand(this ModelBoundQuerySettings settings, string propertyName)
    {
        ExpandConfiguration expandConfiguration;
        if (settings.ExpandConfigurations.TryGetValue(propertyName, out expandConfiguration))
        {
            return expandConfiguration.ExpandType == SelectExpandType.Automatic;
        }
        else
        {
            return settings.DefaultExpandType.HasValue && settings.DefaultExpandType == SelectExpandType.Automatic;
        }
    }

    internal static bool IsAutomaticSelect(this ModelBoundQuerySettings settings, string propertyName)
    {
        SelectExpandType selectExpandType;
        if (settings.SelectConfigurations.TryGetValue(propertyName, out selectExpandType))
        {
            return selectExpandType == SelectExpandType.Automatic;
        }
        else
        {
            return settings.DefaultSelectType.HasValue && settings.DefaultSelectType == SelectExpandType.Automatic;
        }
    }

    internal static bool Expandable(this ModelBoundQuerySettings settings, string propertyName)
    {
        ExpandConfiguration expandConfiguration;
        if (settings.ExpandConfigurations.TryGetValue(propertyName, out expandConfiguration))
        {
            return expandConfiguration.ExpandType != SelectExpandType.Disabled;
        }
        else
        {
            return settings.DefaultExpandType.HasValue && settings.DefaultExpandType != SelectExpandType.Disabled;
        }
    }

    internal static bool Selectable(this ModelBoundQuerySettings settings, string propertyName)
    {
        SelectExpandType selectExpandType;
        if (settings.SelectConfigurations.TryGetValue(propertyName, out selectExpandType))
        {
            return selectExpandType != SelectExpandType.Disabled;
        }
        else
        {
            return settings.DefaultSelectType.HasValue && settings.DefaultSelectType != SelectExpandType.Disabled;
        }
    }

    internal static bool Sortable(this ModelBoundQuerySettings settings, string propertyName)
    {
        bool enable;
        if (settings.OrderByConfigurations.TryGetValue(propertyName, out enable))
        {
            return enable;
        }
        else
        {
            return settings.DefaultEnableOrderBy == true;
        }
    }

    internal static bool Filterable(this ModelBoundQuerySettings settings, string propertyName)
    {
        bool enable;
        if (settings.FilterConfigurations.TryGetValue(propertyName, out enable))
        {
            return enable;
        }
        else
        {
            return settings.DefaultEnableFilter == true;
        }
    }
}
