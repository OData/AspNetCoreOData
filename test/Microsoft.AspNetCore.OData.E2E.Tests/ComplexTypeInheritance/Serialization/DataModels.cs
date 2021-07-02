// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization
{

    public class InheritanceCustomer
    {
        public int Id { get; set; }

        public InheritanceLocation Location { get; set; }
    }

    public class InheritanceLocation
    {
        public string Name { get; set; }

        public InheritanceAddress Address { get; set; }
    }

    public class InheritanceAddress
    {
        public string City { get; set; }

        public string Street { get; set; }
    }

    public class InheritanceUsAddress : InheritanceAddress
    {
        public int ZipCode { get; set; }
    }

    public class InheritanceCnAddress : InheritanceAddress
    {
        public string PostCode { get; set; }
    }

}
