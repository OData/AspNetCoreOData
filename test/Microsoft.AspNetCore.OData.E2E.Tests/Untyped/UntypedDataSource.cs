//-----------------------------------------------------------------------------
// <copyright file="UntypedDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped
{
    public class UntypedDataSource
    {
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
                        Data = new NotInModelAddress { Value = "<--->" }, // un-declared in the model
                        Infos = new object[] { new NotInModelAddress { Value = "<===>" } },
                        Containers = new Dictionary<string, object>
                        {
                            { "ComplexDynamic1",new InModelAddress{ City = "BlackCity", Street = "Shang Rd" } },
                            { "ComplexDynamic2", new NotInModelAddress { Value = "AnyDynanicValue" } },
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
                        Infos = new object[] { new NotInModelAddress { Value = "<===>" } },
                        Containers = new Dictionary<string, object>
                        {
                            { "AnyDynamic1",new InModelAddress{ City = "RedCity", Street = "Mos Rd" } },
                            { "AnyDynamic2", new NotInModelAddress { Value = "AnyDynanicValue" } },
                        }
                    },
                };
            }

            return _people;
        }
    }
}
