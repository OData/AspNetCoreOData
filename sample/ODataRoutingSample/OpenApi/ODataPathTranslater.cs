// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using ODataSegmentKind = Microsoft.AspNetCore.OData.Routing.Template.ODataSegmentKind;

namespace ODataRoutingSample.OpenApi
{
    public static class ODataPathTranslater
    {
        public static ODataPath Translate(this ODataPathTemplate pathTemplate)
        {
            if (pathTemplate.Count == 0)
            {
                // It's service root, so far, let's skip it.
                return null;
            }

            IList<ODataSegment> newSegments = new List<ODataSegment>();
            foreach (var segment in pathTemplate)
            {
                switch (segment.Kind)
                {
                    case ODataSegmentKind.Metadata:
                        newSegments.Add(new ODataMetadataSegment());
                        break;

                    case ODataSegmentKind.EntitySet:
                        EntitySetSegmentTemplate entitySet = (EntitySetSegmentTemplate)segment;
                        newSegments.Add(entitySet.ConvertTo());
                        break;

                    case ODataSegmentKind.Singleton:
                        SingletonSegmentTemplate singleton = (SingletonSegmentTemplate)segment;
                        newSegments.Add(singleton.ConvertTo());
                        break;

                    case ODataSegmentKind.Key:
                        KeySegmentTemplate key = (KeySegmentTemplate)segment;
                        newSegments.Add(key.ConvertTo());
                        break;

                    case ODataSegmentKind.Cast:
                        CastSegmentTemplate cast = (CastSegmentTemplate)segment;
                        newSegments.Add(cast.ConvertTo());
                        break;

                    case ODataSegmentKind.Property:
                        // TODO: 
                        return null;
                        //PropertySegmentTemplate property = (PropertySegmentTemplate)segment;
                        //newSegments.Add(property.ConvertTo());
                        //break;

                    case ODataSegmentKind.Navigation:
                        NavigationSegmentTemplate navigation = (NavigationSegmentTemplate)segment;
                        newSegments.Add(navigation.ConvertTo());
                        break;

                    case ODataSegmentKind.Function:
                        FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;
                        newSegments.Add(function.ConvertTo());
                        break;

                    case ODataSegmentKind.Action:
                        ActionSegmentTemplate action = (ActionSegmentTemplate)segment;
                        newSegments.Add(action.ConvertTo());
                        break;

                    case ODataSegmentKind.FunctionImport:
                        FunctionImportSegmentTemplate functionImport = (FunctionImportSegmentTemplate)segment;
                        newSegments.Add(functionImport.ConvertTo());
                        break;

                    case ODataSegmentKind.ActionImport:
                        ActionImportSegmentTemplate actionImport = (ActionImportSegmentTemplate)segment;
                        newSegments.Add(actionImport.ConvertTo());
                        break;

                    case ODataSegmentKind.Value:
                        return null;
                        //ValueSegmentTemplate value = (ValueSegmentTemplate)segment;
                        //newSegments.Add(value.ConvertTo());
                        //break;

                    case ODataSegmentKind.Ref:
                        return null;
                        //KeySegmentTemplate key = (KeySegmentTemplate)segment;
                        //newSegments.Add(key.ConvertTo());
                        //break;

                    case ODataSegmentKind.NavigationLink:
                        return null;
                        //NavigationLinkSegmentTemplate navigationLink = (NavigationLinkSegmentTemplate)segment;
                        //newSegments.Add(navigationLink.ConvertTo());
                        //break;

                    case ODataSegmentKind.Count:
                        CountSegmentTemplate count = (CountSegmentTemplate)segment;
                        newSegments.Add(count.ConvertTo());
                        break;

                    case ODataSegmentKind.PathTemplate:
                        return null;
                        //KeySegmentTemplate key = (KeySegmentTemplate)segment;
                        //newSegments.Add(key.ConvertTo());
                        //break;

                    case ODataSegmentKind.Dynamic:
                        return null;
                        //KeySegmentTemplate key = (KeySegmentTemplate)segment;
                        //newSegments.Add(key.ConvertTo());
                        //break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return new ODataPath(newSegments);
        }

        public static ODataNavigationSourceSegment ConvertTo(this EntitySetSegmentTemplate entitySet)
        {
            return new ODataNavigationSourceSegment(entitySet.EntitySet);
        }

        public static ODataNavigationSourceSegment ConvertTo(this SingletonSegmentTemplate singleton)
        {
            return new ODataNavigationSourceSegment(singleton.Singleton);
        }

        public static ODataKeySegment ConvertTo(this KeySegmentTemplate key)
        {
            return new ODataKeySegment(key.EntityType, key.KeyMappings);
        }

        public static ODataTypeCastSegment ConvertTo(this CastSegmentTemplate cast)
        {
            // So far, only support the entity type cast
            return new ODataTypeCastSegment(cast.ExpectedType as IEdmEntityType);
        }

        //public static ODataTypeCastSegment ConvertTo(this PropertySegmentTemplate property)
        //{
        //    // So far, only support the entity type cast
        //    return new ODataTypeCastSegment(property);
        //}

        public static ODataNavigationPropertySegment ConvertTo(this NavigationSegmentTemplate navigation)
        {
            return new ODataNavigationPropertySegment(navigation.Navigation);
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
}
