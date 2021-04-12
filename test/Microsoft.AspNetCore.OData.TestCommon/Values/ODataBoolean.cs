// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// A OData boolean value.
    /// </summary>
    public class ODataBoolean : IODataValue
    {
        public static ODataBoolean True = new ODataBoolean(true);

        public static ODataBoolean False = new ODataBoolean(false);

        private ODataBoolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; }
    }
}
