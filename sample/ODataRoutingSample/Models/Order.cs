// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace ODataRoutingSample.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public virtual Category Category { get; set; }
    }

    public class VipOrder : Order
    {
        public virtual Category VipCategory { get; set; }
    }
}
