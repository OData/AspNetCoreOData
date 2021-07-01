// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Validation
{
    public class PatchCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [RegularExpression("Some value")]
        public string ExtraProperty { get; set; }
    }
}
