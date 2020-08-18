// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    /// <summary>
    /// Base class for <c>TheoryDataSet</c> classes.
    /// </summary>
    public abstract class TheoryDataSet : IEnumerable<object[]>
    {
        private readonly List<object[]> data = new List<object[]>();

        protected void AddItem(params object[] values)
        {
            data.Add(values);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
