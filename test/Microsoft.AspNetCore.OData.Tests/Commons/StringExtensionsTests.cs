// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Common;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class StringExtensionsTests
    {
        [Fact]
        public void TryExtractKeyValuePairsWorksAsExpected()
        {
            string input = " inOffice=true , name='abc,''efg'";
            bool result = input.TryExtractKeyValuePairs(out IDictionary<string, string> pairs);

            Assert.True(result);
        }
    }
}
