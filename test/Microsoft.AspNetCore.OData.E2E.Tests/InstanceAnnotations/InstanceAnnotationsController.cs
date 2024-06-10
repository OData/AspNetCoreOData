//-----------------------------------------------------------------------------
// <copyright file="InstanceAnnotationsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations
{
    public class CustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            IDictionary<string, object> annotations = new Dictionary<string, object>
            {
                { "NS.TestAnnotation", 1978 }
            };

            Request.SetInstanceAnnotations(annotations);

            return Ok(InstanceAnnotationsDataSource.Customers);
        }

        public IActionResult Get(int key)
        {
            InsCustomer customer = InstanceAnnotationsDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer using key {key}!");
            }

            return Ok(customer);
        }

        public IActionResult GetAge(int key)
        {
            InsCustomer customer = InstanceAnnotationsDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer using key {key}!");
            }

            IDictionary<string, object> annotationsOfAge = null;
            if (customer.AnnotationContainer != null)
            {
                annotationsOfAge = customer.AnnotationContainer.GetPropertyAnnotations("Age");
            }

            // Set the top-level instance annotations
            Request.SetInstanceAnnotations(annotationsOfAge);

            return Ok(customer.Age);
        }

        public IActionResult GetName(int key)
        {
            InsCustomer customer = InstanceAnnotationsDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer using key {key}!");
            }

            IDictionary<string, object> annotationsOfAge = null;
            if (customer.AnnotationContainer != null)
            {
                annotationsOfAge = customer.AnnotationContainer.GetPropertyAnnotations("Name");
            }

            // Set the top-level instance annotations
            Request.SetInstanceAnnotations(annotationsOfAge);

            return Ok(customer.Name);
        }

        public IActionResult GetMagics(int key)
        {
            InsCustomer customer = InstanceAnnotationsDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer using key {key}!");
            }

            IDictionary<string, object> annotationsOfAge = null;
            if (customer.AnnotationContainer != null)
            {
                annotationsOfAge = customer.AnnotationContainer.GetPropertyAnnotations("Magics");
            }

            // Set the top-level instance annotations
            Request.SetInstanceAnnotations(annotationsOfAge);

            return Ok(customer.Magics);
        }

        public IActionResult GetLocation(int key)
        {
            InsCustomer customer = InstanceAnnotationsDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer using key {key}!");
            }

            if (customer.Location == null)
            {
                return Ok(customer.Location);
            }

            IDictionary<string, object> annotationsOfAge = null;
            if (customer.AnnotationContainer != null)
            {
                annotationsOfAge = customer.AnnotationContainer.GetPropertyAnnotations("Location");
            }

            // Set the top-level instance annotations
            Request.SetInstanceAnnotations(annotationsOfAge);

            return Ok(customer.Location);
        }

        public IActionResult Post([FromBody] InsCustomer customer)
        {
            Assert.NotNull(customer);
            if (customer.Name == "AnnotationOnTypeName1")
            {
                Assert.NotNull(customer.AnnotationContainer);
                IODataInstanceAnnotationContainer container = customer.AnnotationContainer;
                IDictionary<string, object> resourceAnnotations = container.GetResourceAnnotations();
                Assert.Equal(2, resourceAnnotations.Count);
                Assert.Equal(44, resourceAnnotations["NS.Primitive"]);
                InsAddress address = Assert.IsType<InsAddress>(resourceAnnotations["NS.Resource"]);
                Assert.Equal("148TH AVE", address.Street);
                Assert.Equal("Seattle", address.City);
                Assert.Null(address.AnnotationContainer);
            }
            else if (customer.Name == "AnnotationOnPropertyName1")
            {
                Assert.NotNull(customer.AnnotationContainer);
                IODataInstanceAnnotationContainer container = customer.AnnotationContainer;

                IDictionary<string, object> resourceAnnotations = container.GetResourceAnnotations();
                KeyValuePair<string, object> resourceAnnotation = Assert.Single(resourceAnnotations);
                Assert.Equal("NS.Primitive", resourceAnnotation.Key);
                Assert.Equal(45, resourceAnnotation.Value);

                IDictionary<string, object> propertyAnnotations = container.GetPropertyAnnotations("Age");

                Assert.Equal(2, propertyAnnotations.Count);
                Assert.Equal(74, propertyAnnotations["NS.Primitive"]);
                IEnumerable collection = propertyAnnotations["NS.CollectionTerm"] as IEnumerable;
                Assert.Equal(new int[] { 1, 2, 3 }, collection);

                propertyAnnotations = container.GetPropertyAnnotations("Magics");
                InsAddress address = Assert.IsType<InsAddress>(propertyAnnotations["NS.Resource"]);
                Assert.Equal("228TH ST", address.Street);
                Assert.Equal("Issaquah", address.City);
                Assert.Null(address.AnnotationContainer);
            }
            else if (customer.Name == "AdvancedAnnotations")
            {

            }
            else if (customer.Name == "UntypedAnnotations")
            {
                Assert.NotNull(customer.AnnotationContainer);
                IODataInstanceAnnotationContainer container = customer.AnnotationContainer;

                IDictionary<string, object> resourceAnnotations = container.GetResourceAnnotations();
                Assert.Null(resourceAnnotations);

                IDictionary<string, object> propertyAnnotations = container.GetPropertyAnnotations("Magics");

                KeyValuePair<string, object> annotation = Assert.Single(propertyAnnotations);
                Assert.Equal("NS.Collection", annotation.Key);

                IEnumerable<object> collection = annotation.Value as IEnumerable<object>;
                Assert.Collection(collection,
                    e =>
                    {
                        EdmUntypedObject untypedObject = Assert.IsType<EdmUntypedObject>(e);
                        Assert.Equal("1199 RD", untypedObject["Street"]);
                        Assert.Equal("Xin", untypedObject["City"]);
                        Assert.Equal("Mei", untypedObject["Region"]);

                    },
                    e => Assert.Null(e),
                    e =>
                    {
                        InsAddress address = Assert.IsType<InsAddress>(e);
                        Assert.Equal("Ren RD", address.Street);
                        Assert.Equal("Shang", address.City);
                        Assert.Null(address.AnnotationContainer);
                    });

                // TODO: ODL can't write the untyped instance annotation value correctly, see https://github.com/OData/odata.net/issues/2994
                // So, have to clear the annotation for serialization correctly. Please remove the codes below when fix the issue at ODL.
                container.InstanceAnnotations["Magics"].Clear();
            }

            // In real APP, use the following line to add the new customer into DB.
            // For the test purpose, skip it to avoid conflict
            // customer.Id = InstanceAnnotationsDataSource.Customers.Count + 1;
            // InstanceAnnotationsDataSource.Customers.Add(customer);

            return Created(customer);
        }

        public IActionResult Patch(int key, Delta<InsCustomer> patch)
        {
            if (key == 77)
            {
                Assert.NotNull(patch);
                Assert.True(patch.TryGetPropertyValue("AnnotationContainer", out object container));
                ODataInstanceAnnotationContainer annotationContainer = Assert.IsType<ODataInstanceAnnotationContainer>(container);
                Assert.Equal(3, annotationContainer.InstanceAnnotations.Count);
                Assert.Single(annotationContainer.GetResourceAnnotations());
                Assert.Equal(2, annotationContainer.GetPropertyAnnotations("Age").Count);
                Assert.Equal(2, annotationContainer.GetPropertyAnnotations("Magics").Count);

                InsCustomer dummyCustomer = new InsCustomer
                {
                    Id = 77,
                    Name = "Patch a Dummy"
                };

                Assert.Null(dummyCustomer.AnnotationContainer); // Guard

                patch.Patch(dummyCustomer);

                Assert.Equal(3, dummyCustomer.AnnotationContainer.InstanceAnnotations.Count);
                var annotationsForCustomer = Assert.Single(dummyCustomer.AnnotationContainer.GetResourceAnnotations());
                Assert.Equal("NS.Primitive", annotationsForCustomer.Key);
                Assert.Equal(777, annotationsForCustomer.Value);

                var annotationsForAgeProperty = annotationContainer.GetPropertyAnnotations("Age");
                Assert.Equal(2, annotationsForAgeProperty.Count);
                Assert.Equal(2077, annotationsForAgeProperty["NS.BirthYear"]);
                Assert.Equal(new int[] { 71, 72, 73 }, annotationsForAgeProperty["NS.CollectionTerm"]);

                var annotationsForMagicsProperty = annotationContainer.GetPropertyAnnotations("Magics");
                Assert.Equal(2, annotationsForMagicsProperty.Count);
                Assert.Equal(new object[] { "Skyline", 7, "Beaver" }, annotationsForMagicsProperty["NS.StringCollection"]);

                InsAddress address = Assert.IsType<InsAddress>(annotationsForMagicsProperty["NS.Resource"]);
                Assert.Equal("228TH ST", address.Street);
                Assert.Equal("Earth", address.City);
                Assert.Null(address.AnnotationContainer);

                return Updated(dummyCustomer);
            }

            InsCustomer customer = InstanceAnnotationsDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer using key {key}!");
            }

            return Updated(customer);
        }
    }
}
