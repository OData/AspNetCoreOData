// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// A OData string value.
    /// </summary>
    public class ODataString : IODataValue
    {
        public ODataString(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
