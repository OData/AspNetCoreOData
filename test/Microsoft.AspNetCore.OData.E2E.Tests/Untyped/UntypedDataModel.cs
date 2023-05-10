//-----------------------------------------------------------------------------
// <copyright file="UntypedDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped
{
    public class InModelPerson
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public object Data { get; set; } // ==> Declared Edm.Untyped

        public IList<object> Infos { get; set; } // ==> Declared Collection(Edm.Untyped)

        public IDictionary<string, object> Containers { get; set; }
    }

    public class InModelAddress
    {
        public string City { get; set; }
        public string Street { get; set; }
    }

    public enum InModelColor
    {
        Red,
        Green,
        Blue,
    }

    // These classes are not built into Edm model, just for normal type
    public class NotInModelAddress
    {
        public string ZipCode { get; set; }
        public string Location { get; set; }
    }

    public enum NotInModelEnum
    {
        Apple,
        Peach
    }
}
