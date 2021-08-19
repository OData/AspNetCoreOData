//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataCustomizedSample.Models
{
    public class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Player>("Players");

            var function = builder.EntityType<Player>().Function("PlayPiano").Returns<string>();
            function.Parameter<int>("kind");
            function.Parameter<string>("name");

            return builder.GetEdmModel();
        }

        public static IEdmModel BuildEdmModel()
        {
            var models = new Dictionary<string, string>()
            {
                {"Customer", "CustomerId" },
                {"Car", "CarId" },
                {"School", "SchoolId" }
            };

            var model = new EdmModel();

            EdmEntityContainer container = new EdmEntityContainer("Default", "Container");

            for (int i = 0; i < models.Count; i++)
            {
                var name = models.ElementAt(i);
                EdmEntityType element = new EdmEntityType("Default", name.Key, null, false, true);
                element.AddKeys(element.AddStructuralProperty(name.Value, EdmPrimitiveTypeKind.Int32));

                model.AddElement(element);
                container.AddEntitySet(name.Key, element);
            }

            model.AddElement(container);
            return model;
        }
    }
}
