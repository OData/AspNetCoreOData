﻿//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.ModelBuilder;

namespace ODataAlternateKeySample.Models
{
    public class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Person>("People");

            EdmModel model = builder.GetEdmModel() as EdmModel;

            SetCustomerAlternateKey(model);
            SetOrderAlternateKey(model);
            SetPersonAlternateKey(model);
            return model;
        }

        private static void SetCustomerAlternateKey(EdmModel model)
        {
            // Add one alternate key using the extension method.
            // It's using 'OData.Community.Keys.V1.AlternateKeys'
            var customer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            var ssn = customer.FindProperty("SSN");

            model.AddAlternateKeyAnnotation(customer, new Dictionary<string, IEdmProperty>
            {
                {"SSN", ssn}
            });

            // Use Core Vocabulary version.
            model.AddAlternateKeyAnnotation(customer, new Dictionary<string, IEdmProperty>()
            {
                {"CoreSN", ssn}
            },
            true /*true means to use core.alternatekeys term*/);
        }

        private static void SetOrderAlternateKey(EdmModel model)
        {
            // Add multiple alternate keys using the Term/annotation methods.
            // It's using 'Org.OData.Core.V1.AlternateKeys'
            var order = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Order");

            var name = order.FindProperty("Name");
            model.AddAlternateKeyAnnotation(order, new Dictionary<string, IEdmProperty>
            {
                {"Name", name},
            });

            var token = order.FindProperty("Token");
            model.AddAlternateKeyAnnotation(order, new Dictionary<string, IEdmProperty>
            {
                {"Token", token},
            });

            // Use the APIs to build the core alternate keys
            var alternateKeysCollection = new List<IEdmExpression>();
            foreach (string item in new [] { "Name", "Token"})
            {
                List<IEdmExpression> propertyRefs = new List<IEdmExpression>();

                IEdmRecordExpression propertyRef = new EdmRecordExpression(
                    new EdmPropertyConstructor("Alias", new EdmStringConstant($"Core{item}")),
                    new EdmPropertyConstructor("Name", new EdmPropertyPathExpression(item)));
                propertyRefs.Add(propertyRef);

                EdmRecordExpression alternateKeyRecord = new EdmRecordExpression(
                   new EdmPropertyConstructor("Key", new EdmCollectionExpression(propertyRefs)));

                alternateKeysCollection.Add(alternateKeyRecord);
            }

            var term = model.FindTerm("Org.OData.Core.V1.AlternateKeys");
            var annotation = new EdmVocabularyAnnotation(order, term, new EdmCollectionExpression(alternateKeysCollection));

            annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
            model.SetVocabularyAnnotation(annotation);
        }

        private static void SetPersonAlternateKey(EdmModel model)
        {
            // Add composed alternate keys using the Term/annotation methods.
            // It's using 'Org.OData.Core.V1.AlternateKeys'
            var person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            var cr = person.FindProperty("CountryOrRegion");
            var passport = person.FindProperty("Passport");

            model.AddAlternateKeyAnnotation(person, new Dictionary<string, IEdmProperty>
            {
                {"c_or_r", cr},
                {"passport", passport},
            });

            // Use Core Vocabulary version.
            model.AddAlternateKeyAnnotation(person, new Dictionary<string, IEdmProperty>
            {
                {"core_c_r", cr},
                {"core_passport", passport},
            },
            true);
        }
    }
}
