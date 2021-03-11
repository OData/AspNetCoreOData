// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    public class EndpointRouteInfo
    {
        public string ControllerFullName { get; set; }

        public string ActionFullName { get; set; }

        public string HttpMethods { get; set; }

        public string Template { get; set; }

        public bool IsODataRoute { get; set; }
    }
}

