// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// A OData null value.
    /// </summary>
    public class ODataNull : IODataValue
    {
        public static ODataNull Null = new ODataNull();
    }
}
