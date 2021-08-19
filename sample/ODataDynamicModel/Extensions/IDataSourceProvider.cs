//-----------------------------------------------------------------------------
// <copyright file="IDataSourceProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ODataDynamicModel.Extensions
{
    public interface IDataSourceProvider
    {
        IDictionary<string, IDataSource> DataSources { get; }
    }
}
