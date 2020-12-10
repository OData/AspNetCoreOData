// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace ODataRoutingSample.Models
{
    public class Address
    {
        public string City { get; set; }

        public string Street { get; set; }
    }

    public class CnAddress : Address
    {
        public string Postcode { get; set; }
    }

    public class UsAddress : Address
    {
        public string Zipcode { get; set; }
    }
}
