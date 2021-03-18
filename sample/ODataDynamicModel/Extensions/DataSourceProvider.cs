// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace ODataDynamicModel.Extensions
{
    public class DataSourceProvider : IDataSourceProvider
    {
        public DataSourceProvider()
        {
            DataSources = new Dictionary<string, IDataSource>
            {
                { "mydatasource", new MyDataSource() },
                { "anotherdatasource", new AnotherDataSource() }
            };
        }

        public IDictionary<string, IDataSource> DataSources { get; }
    }
}
