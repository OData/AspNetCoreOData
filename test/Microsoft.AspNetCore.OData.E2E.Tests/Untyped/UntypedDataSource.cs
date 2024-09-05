//-----------------------------------------------------------------------------
// <copyright file="UntypedDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Formatter.Value;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped;

public class UntypedDataSource
{
    public static IList<InModelPerson> Managers => new List<InModelPerson>
    {
        new InModelPerson
        {
            Id = 1,
            Name = "Sun",
            Data = new InModelAddress
            {
                City = "Shanghai", Street = "Fengjin RD"
            },
            Infos = new object[] { 1, "abc", 3},
            Containers = new Dictionary<string, object>
            {
                { "D_Data", new InModelAddress { City = "Shanghai", Street = "Fengjin RD" } },
                { "D_Infos", new object[] { 1, "abc", 3} },
            }
        },
        new InModelPerson
        {
            Id = 2,
            Name = "Sun",
            Data = new object[]
            {
                42,
                null,
                "abc",
                new EdmUntypedObject
                {
                    {  "ACity", "Shanghai" },
                    {  "AData", new EdmUntypedCollection
                        {
                            42,
                            InModelColor.Red
                        }
                    }
                }
            },
            Infos = new EdmUntypedCollection
            {
                42,
                InModelColor.Red
            },
            Containers = new Dictionary<string, object>
            {
                { "D_Data", new EdmUntypedObject { { "D_City", new EdmUntypedCollection() } } },
                { "D_Infos", new EdmUntypedCollection { new Dictionary<string, object> { {"k", "v"} } } }
            }
        }
    };

    private static IList<InModelPerson> _people;

    public static IList<InModelPerson> GetAllPeople()
    {
        if (_people == null)
        {
            _people = new List<InModelPerson>
            {
                // Basic primitive value
                new InModelPerson
                {
                    Id = 1,
                    Name = "Kerry",
                    Data = 13,
                    Infos = new object[] { 1, 2, 3},
                    Containers = new Dictionary<string, object>
                    {
                        { "Dynamic1", 13 },
                        { "Dynamic2", true },
                    }
                },

                // In model and not in model enum
                new InModelPerson
                {
                    Id = 2,
                    Name = "Xu",
                    Data = InModelColor.Red,
                    Infos = new object[] { InModelColor.Blue, InModelColor.Green, NotInModelEnum.Apple },
                    Containers = new Dictionary<string, object>
                    {
                        { "EnumDynamic1", InModelColor.Blue },
                        { "EnumDynamic2", NotInModelEnum.Apple },
                    }
                },

                // not in model enum
                new InModelPerson
                {
                    Id = 22, // special Id number
                    Name = "Yin",
                    Data = NotInModelEnum.Apple,
                    Infos = new object[] { InModelColor.Blue, InModelColor.Green, NotInModelEnum.Apple },
                    Containers = new Dictionary<string, object>
                    {
                        { "EnumDynamic1", InModelColor.Blue },
                        { "EnumDynamic2", NotInModelEnum.Apple },
                    }
                },

                // In model complex
                new InModelPerson
                {
                    Id = 3,
                    Name = "Mars",
                    Data = new InModelAddress{ City = "Redmond", Street = "134TH AVE" }, // declared
                    Infos = new object[]
                    {
                        new InModelAddress{ City = "Issaq", Street = "Klahanie Way" }
                    },
                    Containers = new Dictionary<string, object>
                    {
                        { "ComplexDynamic1",new InModelAddress{ City = "RedCity", Street = "Mos Rd" } }
                    }
                },

                // In and Not in model complex
                new InModelPerson
                {
                    Id = 4,
                    Name = "Wu",
                    Data = new NotInModelAddress { ZipCode = "<--->", Location = "******"}, // un-declared in the model
                    Infos = new object[] { new NotInModelAddress { ZipCode = "<===>", Location = "Info-Locations" } },
                    Containers = new Dictionary<string, object>
                    {
                        { "ComplexDynamic1",new InModelAddress{ City = "BlackCity", Street = "Shang Rd" } },
                        { "ComplexDynamic2", new NotInModelAddress { ZipCode = "AnyDynanicValue", Location = "In Dy location." } },
                    }
                },

                // Collection using in and not in model types
                new InModelPerson
                {
                    Id = 5,
                    Name = "Wen",
                    Data = new object[]
                        {
                            null,
                            42,
                            new InModelAddress{ City = "Redmond", Street = "134TH AVE" }
                        },
                    Infos = new object[] { new NotInModelAddress { ZipCode = "<===>", Location = "!@#$" } },
                    Containers = new Dictionary<string, object>
                    {
                        { "AnyDynamic1",new InModelAddress{ City = "RedCity", Street = "Mos Rd" } },
                        { "AnyDynamic2", new NotInModelAddress { ZipCode = "AnyDynanicValue", Location = "Duck Location" } },
                    }
                },
            };

            // Collection in collection
            InModelPerson p = new InModelPerson
            {
                Id = 99, // special Id to test collection in collection
                Name = "Chuan",
                Data = new object[]
                    {
                        null,
                        new object[] { 42, new InModelAddress { City = "Redmond", Street = "134TH AVE" } }
                    },
                Infos = new object[]
                {
                    new EdmUntypedCollection
                    {
                        new NotInModelAddress { ZipCode = "NoAValidZip", Location = "OnEarth" },
                        null,
                        new EdmUntypedCollection
                        {
                            new EdmUntypedCollection
                            {
                                new object[]
                                {
                                    new InModelAddress { City = "Issaquah", Street = "80TH ST" }
                                }
                            }
                        }
                    },
                    42
                },
                Containers = new Dictionary<string, object>
                {
                    { "Dp", new object[] { new InModelAddress{ City = "BlackCastle", Street = "To Castle Rd" } } }
                }
            };

            _people.Add(p);
        }

        return _people;
    }
}
