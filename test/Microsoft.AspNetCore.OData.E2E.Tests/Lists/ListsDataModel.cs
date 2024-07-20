//-----------------------------------------------------------------------------
// <copyright file="EnumsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    public class Employee
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<string> ListTestString { get; set; }

        public IList<bool> ListTestBool { get; set; }

        public IList<int> ListTestInt { get; set; }

        public IList<double> ListTestDouble { get; set; }

        public IList<float> ListTestFloat { get; set; }

        public IList<Uri> ListTestUri { get; set; }

        public uint[] ListTestUint { get; set; }
    }
}
