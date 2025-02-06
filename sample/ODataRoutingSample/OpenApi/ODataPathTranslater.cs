//-----------------------------------------------------------------------------
// <copyright file="ODataPathTranslater.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;

namespace ODataRoutingSample.OpenApi;

public static class ODataPathTranslater
{
    public static ODataPath Translate(this ODataPathTemplate pathTemplate, IEdmModel model)
    {
        if (pathTemplate.Count == 0)
        {
            // It's service root, so far, let's skip it.
            return null;
        }

        IList<ODataSegment> newSegments = new List<ODataSegment>();
        foreach (var segment in pathTemplate)
        {
            switch (segment)
            {
                case MetadataSegmentTemplate:
                    newSegments.Add(new ODataMetadataSegment());
                    break;

                case EntitySetSegmentTemplate entitySet:
                    newSegments.Add(entitySet.ConvertTo());
                    break;

                case SingletonSegmentTemplate singleton:
                    newSegments.Add(singleton.ConvertTo());
                    break;

                case KeySegmentTemplate key:
                    newSegments.Add(key.ConvertTo());
                    break;

                case CastSegmentTemplate cast:
                    newSegments.Add(cast.ConvertTo(model));
                    break;

                case PropertySegmentTemplate property:
                    // TODO: 
                    return null;

                case NavigationSegmentTemplate navigation:
                    newSegments.Add(navigation.ConvertTo());
                    break;

                case FunctionSegmentTemplate function:
                    newSegments.Add(function.ConvertTo());
                    break;

                case ActionSegmentTemplate action:
                    newSegments.Add(action.ConvertTo());
                    break;

                case FunctionImportSegmentTemplate functionImport:
                    newSegments.Add(functionImport.ConvertTo());
                    break;

                case ActionImportSegmentTemplate actionImport:
                    newSegments.Add(actionImport.ConvertTo());
                    break;

                case ValueSegmentTemplate value:
                    return null;

                case NavigationLinkSegmentTemplate navigationLink:
                    return null;

                case CountSegmentTemplate count:
                    newSegments.Add(count.ConvertTo());
                    break;

                case PathTemplateSegmentTemplate:
                    return null;

                case DynamicSegmentTemplate:
                    return null;

                case NavigationLinkTemplateSegmentTemplate navigationLinkTemplate:
                    return null;

                default:
                    return null;
                    // throw new NotSupportedException("Please update codes here, maybe just return null if you add some new OData endpoints into this sample.");
            }
        }

        return new ODataPath(newSegments);
    }

    public static ODataNavigationSourceSegment ConvertTo(this EntitySetSegmentTemplate entitySet)
    {
        return new ODataNavigationSourceSegment(entitySet.Segment.EntitySet);
    }

    public static ODataNavigationSourceSegment ConvertTo(this SingletonSegmentTemplate singleton)
    {
        return new ODataNavigationSourceSegment(singleton.Singleton);
    }

    public static ODataKeySegment ConvertTo(this KeySegmentTemplate key)
    {
        return new ODataKeySegment(key.EntityType, key.KeyMappings);
    }

    public static ODataTypeCastSegment ConvertTo(this CastSegmentTemplate cast, IEdmModel model)
    {
        // So far, only support the entity type cast
        return new ODataTypeCastSegment(cast.ExpectedType as IEdmEntityType, model);
    }

    //public static ODataTypeCastSegment ConvertTo(this PropertySegmentTemplate property)
    //{
    //    // So far, only support the entity type cast
    //    return new ODataTypeCastSegment(property);
    //}

    public static ODataNavigationPropertySegment ConvertTo(this NavigationSegmentTemplate navigation)
    {
        return new ODataNavigationPropertySegment(navigation.Segment.NavigationProperty);
    }

    public static ODataOperationSegment ConvertTo(this FunctionSegmentTemplate function)
    {
        return new ODataOperationSegment(function.Function);
    }

    public static ODataOperationSegment ConvertTo(this ActionSegmentTemplate action)
    {
        return new ODataOperationSegment(action.Action);
    }

    public static ODataOperationImportSegment ConvertTo(this FunctionImportSegmentTemplate functionImport)
    {
        return new ODataOperationImportSegment(functionImport.FunctionImport, functionImport.ParameterMappings);
    }

    public static ODataOperationImportSegment ConvertTo(this ActionImportSegmentTemplate actionImport)
    {
        return new ODataOperationImportSegment(actionImport.ActionImport);
    }

    public static ODataDollarCountSegment ConvertTo(this CountSegmentTemplate count)
    {
        return new ODataDollarCountSegment();
    }
}
