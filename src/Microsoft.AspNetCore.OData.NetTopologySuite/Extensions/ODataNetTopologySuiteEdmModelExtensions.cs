//-----------------------------------------------------------------------------
// <copyright file="ODataNetTopologySuiteEdmModelExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.AspNetCore.OData.NetTopologySuite.Edm;
using Microsoft.AspNetCore.OData.NetTopologySuite.UriParser.Parsers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;

/// <summary>
/// Extensions to attach NetTopologySuite type mapping to an Edm model.
/// </summary>
public static class ODataNetTopologySuiteEdmModelExtensions
{
    /// <summary>
    /// Attaches the NetTopologySuite type mapper to the given Edm model.
    /// </summary>
    public static IEdmModel UseNetTopologySuite(this IEdmModel model)
    {
        if (model == null)
        {
            throw Error.ArgumentNull(nameof(model));
        }

        model.SetTypeMapper(ODataNetTopologySuiteTypeMapper.Instance);
        // TODO: Make parser target Edm.Geometry* and Edm.Geography* or leave it general?
        model.AddCustomUriLiteralParser(ODataNetTopologySuiteUriLiteralParser.Instance);

        return model;
    }
}
