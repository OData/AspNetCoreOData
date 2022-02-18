//-----------------------------------------------------------------------------
// <copyright file="DataSourceProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
