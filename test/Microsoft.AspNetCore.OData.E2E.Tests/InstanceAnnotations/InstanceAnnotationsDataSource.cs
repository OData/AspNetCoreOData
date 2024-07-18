//-----------------------------------------------------------------------------
// <copyright file="InstanceAnnotationsDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations
{
    public class InstanceAnnotationsDataSource
    {
        private static IList<InsCustomer> _customers = null;

        public static IList<InsCustomer> Customers => _customers;

        static InstanceAnnotationsDataSource()
        {
            _customers = new List<InsCustomer>();

            // Be noted: The data for customers (1,2...6) are designed for certain test cases.
            // If you want to change, remember to change the related test cases.

            // 1. Without any instance annotation
            InsCustomer customer1 = new InsCustomer
            {
                Id = 1,
                Name = "Peter",
                Age = 19,
                Magics = new List<int>() { 1, 2 },
                Location = new InsAddress { City = "City 1", Street = "Street 1" },
                FavoriteSports = new InsFavoriteSports()
                {
                    LikeMost = InsSport.Soccer,
                    Like = new List<InsSport> { InsSport.Basketball, InsSport.Badminton }
                }
            };
            _customers.Add(customer1);

            // 2. With instance annotation on entity
            InsCustomer customer2 = new InsCustomer
            {
                Id = 2,
                Name = "Sam",
                Age = 40,
                Magics = new List<int>() { 15 },
                Location = new InsAddress { City = "City 2", Street = "Street 2" },
                FavoriteSports = new InsFavoriteSports()
                {
                    LikeMost = InsSport.Badminton,
                    Like = new List<InsSport> { InsSport.Soccer, InsSport.Tennis }
                }
            };
            customer2.AnnotationContainer = new ODataInstanceAnnotationContainer();
            customer2.AnnotationContainer.AddResourceAnnotation("NS.CUSTOMER2.Primitive", 22);
            _customers.Add(customer2);

            // 3. With instance annotation on property
            InsCustomer customer3 = new InsCustomer
            {
                Id = 3,
                Name = "John",
                Age = 34,
                Magics = new List<int>() { 98, 81 },
                Location = new InsAddress { City = "City 3", Street = "Street 3" },
                FavoriteSports = new InsFavoriteSports()
                {
                    LikeMost = InsSport.Swimming,
                    Like = new List<InsSport> { InsSport.Tennis }
                }
            };
            customer3.AnnotationContainer = new ODataInstanceAnnotationContainer();
            customer3.AnnotationContainer.AddPropertyAnnotation("Age", "NS.CUSTOMER3.Primitive", 33);
            customer3.AnnotationContainer.AddPropertyAnnotation("Age", "NS.CUSTOMER3.Collection", new string[] { "abc", "xyz"});
            _customers.Add(customer3);

            // 4. With instance annotation on entity and property
            InsCustomer customer4 = new InsCustomer
            {
                Id = 4,
                Name = "Kerry",
                Age = 29,
                Magics = new List<int>() { 6, 4, 5 },
                Location = new InsAddress { City = "City 4", Street = "Street 4" },
                FavoriteSports = new InsFavoriteSports()
                {
                    LikeMost = InsSport.Tennis,
                    Like = new List<InsSport> { InsSport.Soccer }
                }
            };
            customer4.AnnotationContainer = new ODataInstanceAnnotationContainer();
            customer4.AnnotationContainer.AddResourceAnnotation("NS.CUSTOMER4.Complex", new InsAddress
            {
                Street = "1199 RD",
                City = "Shanghai"
            });
            customer4.AnnotationContainer.AddPropertyAnnotation("Magics", "NS.CUSTOMER4.Enum", InsSport.Badminton);
            _customers.Add(customer4);

            // 5. With nested instance annotation on entity and property
            InsCustomer customer5 = new InsCustomer
            {
                Id = 5,
                Name = "Alex",
                Age = 08,
                Magics = new List<int>() { 9, 10 },
                Location = new InsAddress { City = "City 5", Street = "Street 5" },
                FavoriteSports = new InsFavoriteSports()
                {
                    LikeMost = InsSport.Baseball,
                    Like = new List<InsSport> { InsSport.Baseball }
                }
            };

            customer5.AnnotationContainer = new ODataInstanceAnnotationContainer();

            InsAddress address = new InsAddress
            {
                Street = "1115 Star",
                City = "Mars",
                AnnotationContainer = new ODataInstanceAnnotationContainer()
            };
            address.AnnotationContainer.AddResourceAnnotation("NS.CUSTOMER5.NESTED.Primitive", 1101);
            address.AnnotationContainer.AddPropertyAnnotation("Street", "NS.CUSTOMER5.NESTED.Primitive", 987);

            customer5.AnnotationContainer.AddPropertyAnnotation("Name", "NS.CUSTOMER5.Complex", address);
            _customers.Add(customer5);

            // 6. With instance annotations on complex property and on its type also
            InsCustomer customer6 = new InsCustomer
            {
                Id = 6,
                Name = "Liang",
                Age = 08,
                Magics = new List<int>() { 15 },
                Location = new InsAddress { City = "City 6", Street = "Street 6" },
                FavoriteSports = new InsFavoriteSports()
                {
                    LikeMost = InsSport.Badminton,
                    Like = new List<InsSport> { InsSport.Badminton }
                }
            };

            customer6.AnnotationContainer = new ODataInstanceAnnotationContainer();

            // annotation for enum property
            customer6.AnnotationContainer.AddPropertyAnnotation("FavoriteSports", "NS.CUSTOMER6.Primitive", 921);

            // annotation for complex property on parent entity
            customer6.AnnotationContainer.AddPropertyAnnotation("Location", "NS.CUSTOMER6.Primitive", 1115);

            // annotation for complex property on its entity
            customer6.Location.AnnotationContainer = new ODataInstanceAnnotationContainer();
            customer6.Location.AnnotationContainer.AddResourceAnnotation("NS.CUSTOMER6.Location.Primitive", 71);

            _customers.Add(customer6);
        }
    }
}
