// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace ODataRoutingSample.Models
{
    public class Organization
    {
        public int OrganizationId { get; set; }

        public string Name { get; set; }

        public IList<Department> Departs { get; set; }
    }

    public class Department
    {
        public int DepartmentId { get; set; }

        public string Alias { get; set; }

        public string Name { get; set; }
    }
}
