//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class DollarFilterDataSource
{
    private static IList<Person> people;
    private static List<Vendor> vendors;
    private static List<Vendor> badVendors;
    private static List<Customer> customers;
    private static List<Customer> badCustomers;
    private static List<Product> products;
    private static List<Basket> baskets;
    private static List<BasicType> basicTypes;

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

        #region Customers

        var customer1ContactInfo = new ContactInfo
        {
            DeclaredEmails = new List<string> { "temp1a@test.com", "temp1b@test.com" },
            DeclaredAddresses = new List<Address>
            {
                new Address
                {
                    DeclaredStreet = "Temple Street",
                    DeclaredFloors = new List<Floor>
                    {
                        new Floor { DeclaredNumber = 2, Properties = new Dictionary<string, object> { { "DynamicNumber", 2 } } },
                        new Floor { DeclaredNumber = 3, Properties = new Dictionary<string, object> { { "DynamicNumber", 3 } } }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "DynamicStreet", "Temple Street" },
                        {
                            "DynamicFloors", new List<Floor>
                            {
                                new Floor { DeclaredNumber = 2, Properties = new Dictionary<string, object> { { "DynamicNumber", 2 } } },
                                new Floor { DeclaredNumber = 3, Properties = new Dictionary<string, object> { { "DynamicNumber", 3 } } }
                            }
                        }
                    }
                },
                new Address
                {
                    DeclaredStreet = "Wujiang Road",
                    DeclaredFloors = new List<Floor>
                    {
                        new Floor { DeclaredNumber = 4, Properties = new Dictionary<string, object> { { "DynamicNumber", 4 } } },
                        new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "DynamicStreet", "Wujiang Road" },
                        {
                            "DynamicFloors", new List<Floor>
                            {
                                new Floor { DeclaredNumber = 4, Properties = new Dictionary<string, object> { { "DynamicNumber", 4 } } },
                                new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } }
                            }
                        }
                    }
                }
            },
            Properties = new Dictionary<string, object>
            {
                { "DynamicEmails", new List<string>{ "temp1a@test.com", "temp1b@test.com" } },
                {
                    "DynamicAddresses", new List<Address>
                    {
                        new Address
                        {
                            DeclaredStreet = "Temple Street",
                            DeclaredFloors = new List<Floor>
                            {
                                new Floor { DeclaredNumber = 2, Properties = new Dictionary<string, object> { { "DynamicNumber", 2 } } },
                                new Floor { DeclaredNumber = 3, Properties = new Dictionary<string, object> { { "DynamicNumber", 3 } } }
                            },
                            Properties = new Dictionary<string, object>
                            {
                                { "DynamicStreet", "Temple Street" },
                                {
                                    "DynamicFloors", new List<Floor>
                                    {
                                        new Floor { DeclaredNumber = 2, Properties = new Dictionary<string, object> { { "DynamicNumber", 2 } } },
                                        new Floor { DeclaredNumber = 3, Properties = new Dictionary<string, object> { { "DynamicNumber", 3 } } }
                                    }
                                }
                            }
                        },
                        new Address
                        {
                            DeclaredStreet = "Wujiang Road",
                            DeclaredFloors = new List<Floor>
                            {
                                new Floor { DeclaredNumber = 4, Properties = new Dictionary<string, object> { { "DynamicNumber", 4 } } },
                                new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } }
                            },
                            Properties = new Dictionary<string, object>
                            {
                                { "DynamicStreet", "Wujiang Road" },
                                {
                                    "DynamicFloors", new List<Floor>
                                    {
                                        new Floor { DeclaredNumber = 4, Properties = new Dictionary<string, object> { { "DynamicNumber", 4 } } },
                                        new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var customer2ContactInfo = new ContactInfo
        {
            DeclaredEmails = new List<string> { "temp2a@test.com", "temp2b@test.com" },
            DeclaredAddresses = new List<Address>
            {
                new Address
                {
                    DeclaredStreet = "Buchanan Street",
                    DeclaredFloors = new List<Floor>
                    {
                        new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } },
                        new Floor { DeclaredNumber = 6, Properties = new Dictionary<string, object> { { "DynamicNumber", 6 } } }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "DynamicStreet", "Buchanan Street" },
                        {
                            "DynamicFloors", new List<Floor>
                            {
                                new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } },
                                new Floor { DeclaredNumber = 6, Properties = new Dictionary<string, object> { { "DynamicNumber", 6 } } }
                            }
                        }
                    }
                },
                new Address
                {
                    DeclaredStreet = "Victoria Street",
                    DeclaredFloors = new List<Floor>
                    {
                        new Floor { DeclaredNumber = 7, Properties = new Dictionary<string, object> { { "DynamicNumber", 7 } } },
                        new Floor { DeclaredNumber = 8, Properties = new Dictionary<string, object> { { "DynamicNumber", 8 } } }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "DynamicStreet", "Victoria Street" },
                        {
                            "DynamicFloors", new List<Floor>
                            {
                                new Floor { DeclaredNumber = 7, Properties = new Dictionary<string, object> { { "DynamicNumber", 7 } } },
                                new Floor { DeclaredNumber = 8, Properties = new Dictionary<string, object> { { "DynamicNumber", 8 } } }
                            }
                        }
                    }
                }
            },
            Properties = new Dictionary<string, object>
            {
                { "DynamicEmails", new List<string>{ "temp2a@test.com", "temp2b@test.com" } },
                {
                    "DynamicAddresses", new List<Address>
                    {
                        new Address
                        {
                            DeclaredStreet = "Buchanan Street",
                            DeclaredFloors = new List<Floor>
                            {
                                new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } },
                                new Floor { DeclaredNumber = 6, Properties = new Dictionary<string, object> { { "DynamicNumber", 6 } } }
                            },
                            Properties = new Dictionary<string, object>
                            {
                                { "DynamicStreet", "Buchanan Street" },
                                {
                                    "DynamicFloors", new List<Floor>
                                    {
                                        new Floor { DeclaredNumber = 5, Properties = new Dictionary<string, object> { { "DynamicNumber", 5 } } },
                                        new Floor { DeclaredNumber = 6, Properties = new Dictionary<string, object> { { "DynamicNumber", 6 } } }
                                    }
                                }
                            }
                        },
                        new Address
                        {
                            DeclaredStreet = "Victoria Street",
                            DeclaredFloors = new List<Floor>
                            {
                                new Floor { DeclaredNumber = 7, Properties = new Dictionary<string, object> { { "DynamicNumber", 7 } } },
                                new Floor { DeclaredNumber = 8, Properties = new Dictionary<string, object> { { "DynamicNumber", 8 } } }
                            },
                            Properties = new Dictionary<string, object>
                            {
                                { "DynamicStreet", "Victoria Street" },
                                {
                                    "DynamicFloors", new List<Floor>
                                    {
                                        new Floor { DeclaredNumber = 7, Properties = new Dictionary<string, object> { { "DynamicNumber", 7 } } },
                                        new Floor { DeclaredNumber = 8, Properties = new Dictionary<string, object> { { "DynamicNumber", 8 } } }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                DeclaredContactInfo = customer1ContactInfo,
                Properties = new Dictionary<string, object> { { "DynamicContactInfo",  customer1ContactInfo } }
            },
            new Customer
            {
                Id = 2,
                DeclaredContactInfo = customer2ContactInfo,
                Properties = new Dictionary<string, object> { { "DynamicContactInfo", customer2ContactInfo } }
            }
        };

        #endregion Customers

        #region Bad Customers

        badCustomers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                DeclaredContactInfo = new ContactInfo
                {
                    Properties = new Dictionary<string, object>
                    {
                        {
                            "DynamicNonOpenAddresses", new List<NonOpenAddress>
                            {
                                { new NonOpenAddress { DeclaredStreet = "Madero Street" } }
                            }
                        },
                        {
                            "DynamicNotInModelAddresses", new List<NotInModelAddress>
                            {
                                { new NotInModelAddress { DeclaredStreet = "No Way" } }
                            }
                        },
                        {
                            "DynamicAddress", new Address { DeclaredStreet = "Madero Street" }
                        },
                        { "Foo", "Bar" }
                    }
                },
                Properties = new Dictionary<string, object>
                {
                    {
                        "DynamicContactInfo", new ContactInfo
                        {
                            Properties = new Dictionary<string, object>
                            {
                                {
                                    "DynamicNonOpenAddresses", new List<NonOpenAddress>
                                    {
                                        { new NonOpenAddress { DeclaredStreet = "Madero Street" } }
                                    }
                                },
                                {
                                    "DynamicNotInModelAddresses", new List<NotInModelAddress>
                                    {
                                        { new NotInModelAddress { DeclaredStreet = "No Way" } }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "PropertyIsNotCollectionContactInfo", new PropertyIsNotCollectionContactInfo
                        {
                            DeclaredAddress = new Address(),
                            Properties = new Dictionary<string, object>
                            {
                                { "DynamicAddress", new Address() }
                            }
                        }
                    },
                    {
                        "DynamicAddresses", new List<Address>
                        {
                            new Address { DeclaredStreet = "Landhies Road" },
                            new Address { DeclaredStreet = "Racecourse Road" }
                        }
                    },
                    { "Foo", "Bar" }
                }
            }
        };

        #endregion Bad Customers

        #region Products

        products = new List<Product>
        {
            new Product
            {
                Id = "accdx34g-3d16-473e-9251-378c68de859e",
                DeclaredSingleValuedProperty = "a",
                DeclaredCollectionValuedProperty = new List<int> { 1, 2, 3 },
                Properties = new Dictionary<string, object>
                {
                    { "DynamicSingleValuedProperty", "a" },
                    { "DynamicCollectionValuedProperty", new int[] { 1, 2, 3 } }
                }
            },
            new Product
            {
                Id = "abc8fh64-3d16-473e-9251-378c68de859f",
                DeclaredSingleValuedProperty = "b",
                DeclaredCollectionValuedProperty = new List<int> { 2, 3, 4 },
                Properties = new Dictionary<string, object>
                {
                    { "DynamicSingleValuedProperty", "b" },
                    { "DynamicCollectionValuedProperty", new int[] { 2, 3, 4 } }
                }
            },
            new Product
            {
                Id = "abc8fh64-3d16-473e-9251-378c68de859g",
                DeclaredSingleValuedProperty = "c",
                DeclaredCollectionValuedProperty = new List<int> { 3, 4, 5 },
                Properties = new Dictionary<string, object>
                {
                    { "DynamicSingleValuedProperty", "c" },
                    { "DynamicCollectionValuedProperty", new int[] { 3, 4, 5 } }
                }
            }
        };

        #endregion Products

        #region Baskets

        baskets = new List<Basket>
        {
            new Basket
            {
                Id = 1,
                DeclaredCollectionValuedProperty = new List<Fruit>
                {
                    new Fruit
                    {
                        Name = "Apple",
                        DynamicProperties = new Dictionary<string, object> { { "Family", "Rosaceae" } }
                    },
                    new Fruit
                    {
                        Name = "Dragon Fruit",
                        DynamicProperties = new Dictionary<string, object> { {  "Family", "Cactaceae" } }
                    }
                },
                DynamicProperties = new Dictionary<string, object>
                {
                    {
                        "DynamicCollectionValuedProperty",
                        new List<Fruit>
                        {
                            new Fruit
                            {
                                Name = "Apple",
                                DynamicProperties = new Dictionary<string, object> { { "Family", "Rosaceae" } }
                            },
                            new Fruit
                            {
                                Name = "Dragon Fruit",
                                DynamicProperties = new Dictionary<string, object> { {  "Family", "Cactaceae" } }
                            }
                        }
                    }
                }
            },
            new Basket
            {
                Id = 2,
                DeclaredCollectionValuedProperty = new List<Fruit>
                {
                    new Fruit
                    {
                        Name = "Orange",
                        DynamicProperties = new Dictionary<string, object> { { "Family", "Rutaceae" } }
                    },
                    new Fruit
                    {
                        Name = "Peach",
                        DynamicProperties = new Dictionary<string, object> { { "Family", "Rosaceae" } }
                    }
                },
                DynamicProperties = new Dictionary<string, object>
                {
                    {
                        "DynamicCollectionValuedProperty",
                        new List<Fruit>
                        {
                            new Fruit
                            {
                                Name = "Orange",
                                DynamicProperties = new Dictionary<string, object> { { "Family", "Rutaceae" } }
                            },
                            new Fruit
                            {
                                Name = "Peach",
                                DynamicProperties = new Dictionary<string, object> { { "Family", "Rosaceae" } }
                            }
                        }
                    }
                }
            }
        };

        #endregion Baskets

        #region Basic Types

        var literalInfo = new LiteralInfo
        {
            DeclaredBooleanProperty = true,
            DeclaredByteProperty = (byte)1,
            DeclaredSignedByteProperty = (sbyte)9,
            DeclaredInt16Property = (short)7,
            DeclaredInt32Property = 13,
            DeclaredInt64Property = 6078747774547L,
            DeclaredSingleProperty = 3.142F,
            DeclaredDoubleProperty = 3.14159265359D,
            DeclaredDecimalProperty = 7654321M,
            DeclaredGuidProperty = new Guid(23, 59, 59, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }),
            DeclaredStringProperty = "Foo",
            DeclaredTimeSpanProperty = new TimeSpan(23, 59, 59),
            DeclaredTimeOfDayProperty = new TimeOnly(23, 59, 59, 0),
            DeclaredDateProperty = new DateOnly(1970, 1, 1),
            DeclaredDateTimeOffsetProperty = new DateTimeOffset(new DateTime(1970, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc)),
            DeclaredEnumProperty = Color.Black,
            DeclaredByteArrayProperty = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 },
            DynamicProperties = new Dictionary<string, object>
            {
                { "DynamicBooleanProperty", true },
                { "DynamicByteProperty", (byte)1 },
                { "DynamicSignedByteProperty", (sbyte)9 },
                { "DynamicInt16Property", (short)7 },
                { "DynamicInt32Property", 13 },
                { "DynamicInt64Property", 6078747774547L },
                { "DynamicSingleProperty", 3.142F },
                { "DynamicDoubleProperty", 3.14159265359D },
                { "DynamicDecimalProperty", 7654321M },
                { "DynamicGuidProperty", new Guid(23, 59, 59, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
                { "DynamicStringProperty", "Foo" },
                { "DynamicTimeSpanProperty", new TimeSpan(23, 59, 59) },
                { "DynamicTimeOfDayProperty", new TimeOnly(23, 59, 59, 0) },
                { "DynamicDateProperty", new DateOnly(1970, 1, 1) },
                { "DynamicDateTimeOffsetProperty", new DateTimeOffset(new DateTime(1970, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc)) },
                { "DynamicEnumProperty", Color.Black },
                { "DynamicByteArrayProperty", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } }
            }
        };

        basicTypes = new List<BasicType>
        {
            new BasicType
            {
                Id = 1,
                DeclaredLiteralInfo = literalInfo,
                DeclaredLiteralInfos = new List<LiteralInfo> { literalInfo },
                DynamicProperties = new Dictionary<string, object>
                {
                    { "DynamicLiteralInfo", literalInfo },
                    { "DynamicLiteralInfos", new List<LiteralInfo> { literalInfo } }
                }
            }
        };

        #endregion Basic Types
    }

    public static IList<Person> People => people;

    public static List<Vendor> Vendors => vendors;

    public static List<Vendor> BadVendors => badVendors;

    public static List<Customer> Customers => customers;

    public static List<Customer> BadCustomers => badCustomers;

    public static List<Product> Products => products;

    public static List<Basket> Baskets => baskets;

    public static List<BasicType> BasicTypes => basicTypes;
}
