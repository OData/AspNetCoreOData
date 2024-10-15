//-----------------------------------------------------------------------------
// <copyright file="InstanceAnnotationsEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations
{
    internal class InstanceAnnotationsEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<InsCustomer>("Customers");
            EdmModel model = builder.GetEdmModel() as EdmModel;

            AddTerm(model);
            return model;
        }

        private static void AddTerm(EdmModel model)
        {
            EdmTerm term1 = new EdmTerm("NS", "CollectionTerm", new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetInt32(true))));
            model.AddElement(term1);
        }
    }
}
