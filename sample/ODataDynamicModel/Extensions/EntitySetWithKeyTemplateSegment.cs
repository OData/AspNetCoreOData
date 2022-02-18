//-----------------------------------------------------------------------------
// <copyright file="EntitySetWithKeyTemplateSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataDynamicModel.Extensions
{
    public class EntitySetWithKeyTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/{key}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("entityset", out object entitysetNameObj))
            {
                return false;
            }

            if (!context.RouteValues.TryGetValue("key", out object keyObj))
            {
                return false;
            }

            string entitySetName = entitysetNameObj as string;
            string keyValue = keyObj as string;

            // if you want to support case-insensitive
            var edmEntitySet = context.Model.EntityContainer.EntitySets()
                .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

            //var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);
            if (edmEntitySet != null)
            {
                EntitySetSegment entitySet = new EntitySetSegment(edmEntitySet);
                IEdmEntityType entityType = entitySet.EntitySet.EntityType();

                IEdmProperty keyProperty = entityType.Key().First();

                object newValue = ODataUriUtils.ConvertFromUriLiteral(keyValue, ODataVersion.V4, context.Model, keyProperty.Type);

                // for non FromODataUri, so update it, for example, remove the single quote for string value.
                context.UpdatedValues["key"] = newValue;

                // For FromODataUri, let's refactor it later.
                string prefixName = ODataParameterValue.ParameterValuePrefix + "key";
                context.UpdatedValues[prefixName] = new ODataParameterValue(newValue, keyProperty.Type);

                IDictionary<string, object> keysValues = new Dictionary<string, object>();
                keysValues[keyProperty.Name] = newValue;

                KeySegment keySegment = new KeySegment(keysValues, entityType, entitySet.EntitySet);

                context.Segments.Add(entitySet);
                context.Segments.Add(keySegment);

                return true;
            }

            return false;
        }
    }
}
