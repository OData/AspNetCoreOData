// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
