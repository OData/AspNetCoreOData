// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class ODataVersionConstraint
    {
        // The header names used for versioning in the versions 4.0+ of the OData protocol.
        internal const string ODataServiceVersionHeader = "OData-Version";
        internal const string ODataMaxServiceVersionHeader = "OData-MaxVersion";
        internal const string ODataMinServiceVersionHeader = "OData-MinVersion";
        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;
    }
}
