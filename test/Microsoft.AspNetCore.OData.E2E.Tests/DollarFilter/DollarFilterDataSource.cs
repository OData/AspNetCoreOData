//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter
{
    public class DollarFilterDataSource
    {
        private static IList<Person> people;
        private static List<Vendor> vendors;
        private static List<Vendor> badVendors;

        static DollarFilterDataSource()
        {
            people = new List<Person>
            {
                new Person { Id = 1, SSN = "a'bc" },
                new Person { Id = 2, SSN = "'def" },
                new Person { Id = 3, SSN = "xyz'" },
                new Person { Id = 4, SSN = "'pqr'" }
            };

            #region Vendors

            vendors = new List<Vendor>
            {
                new Vendor
                {
                    Id = 1,
                    DeclaredPrimitiveProperty = 19,
                    DeclaredSingleValuedProperty = new VendorAddress
                    {
                        Street = "Bourbon Street",
                        City = new VendorCity
                        {
                            Name = "New Orleans",
                            DynamicProperties = new Dictionary<string, object>
                            {
                                { "State", "Louisiana" }
                            }
                        },
                        DynamicProperties = new Dictionary<string, object>
                        {
                            { "ZipCode", "25810" }
                        }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "DynamicPrimitiveProperty", 19 },
                        {
                            "DynamicSingleValuedProperty",
                            new VendorAddress
                            {
                                Street = "Bourbon Street",
                                City = new VendorCity
                                {
                                    Name = "New Orleans",
                                    DynamicProperties = new Dictionary<string, object>
                                    {
                                        { "State", "Louisiana" }
                                    }
                                },
                                DynamicProperties = new Dictionary<string, object>
                                {
                                    { "ZipCode", "25810" }
                                }
                            }
                        }
                    }
                },
                new Vendor
                {
                    Id = 2,
                    DeclaredPrimitiveProperty = 13,
                    DeclaredSingleValuedProperty = new VendorAddress
                    {
                        Street = "Ocean Drive",
                        City = new VendorCity
                        {
                            Name = "Miami",
                            DynamicProperties = new Dictionary<string, object>
                            {
                                { "State", "Florida" }
                            }
                        },
                        DynamicProperties = new Dictionary<string, object>
                        {
                            { "ZipCode", "73857" }
                        }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "DynamicPrimitiveProperty", 13 },
                        {
                            "DynamicSingleValuedProperty",
                            new VendorAddress
                            {
                                Street = "Ocean Drive",
                                City = new VendorCity
                                {
                                    Name = "Miami",
                                    DynamicProperties = new Dictionary<string, object>
                                    {
                                        { "State", "Florida" }
                                    }
                                },
                                DynamicProperties = new Dictionary<string, object>
                                {
                                    { "ZipCode", "73857" }
                                }
                            }
                        }
                    }
                },
                new Vendor
                {
                    Id = 3,
                    DeclaredPrimitiveProperty = 17,
                    DeclaredSingleValuedProperty = new VendorAddress
                    {
                        Street = "Canal Street",
                        City = new VendorCity
                        {
                            Name = "New Orleans",
                            DynamicProperties = new Dictionary<string, object>
                            {
                                { "State", "Louisiana" }
                            }
                        },
                        DynamicProperties = new Dictionary<string, object>
                        {
                            { "ZipCode", "11065" }
                        }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "DynamicPrimitiveProperty", 17 },
                        {
                            "DynamicSingleValuedProperty",
                            new VendorAddress
                            {
                                Street = "Canal Street",
                                City = new VendorCity
                                {
                                    Name = "New Orleans",
                                    DynamicProperties = new Dictionary<string, object>
                                    {
                                        { "State", "Louisiana" }
                                    }
                                },
                                DynamicProperties = new Dictionary<string, object>
                                {
                                    { "ZipCode", "11065" }
                                }
                            }
                        }                    }
                }
            };

            #endregion Vendors

            #region Bad Vendors

            badVendors = new List<Vendor>
        {
            new Vendor
            {
                Id = 1,
                DynamicProperties = new Dictionary<string, object>
                {
                    {
                        "WarehouseAddress",
                        new NonOpenVendorAddress
                        {
                            Street = "Madero Street"
                        }
                    },
                    {
                        "Foo",
                        "Bar"
                    },
                    {
                        "NotInModelAddress",
                        new NotInModelVendorAddress
                        {
                            Street = "No Way"
                        }
                    },
                    {
                        "ContainerPropertyNullAddress",
                        new VendorAddress
                        {
                            Street = "Genova Street",
                            DynamicProperties = null
                        }
                    }
                }
            }
        };

            #endregion Bad Vendors
        }

        public static IList<Person> People => people;

        public static List<Vendor> Vendors => vendors;

        public static List<Vendor> BadVendors => badVendors;
    }
}
