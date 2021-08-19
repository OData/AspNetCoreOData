//-----------------------------------------------------------------------------
// <copyright file="IDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace ODataDynamicModel.Extensions
{
    public interface IDataSource
    {
        IEdmModel GetEdmModel();

        void Get(IEdmEntityTypeReference entityType, EdmEntityObjectCollection collection);

        void Get(string key, EdmEntityObject entity);

        object GetProperty(string property, EdmEntityObject entity);
    }
}
