// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace ODataDynamicModel.Extensions
{
    public interface IDataSourceProvider
    {
        IDictionary<string, IDataSource> DataSources { get; }
    }
}
