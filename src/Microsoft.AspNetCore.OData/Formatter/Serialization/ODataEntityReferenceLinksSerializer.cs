//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinksSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization;

/// <summary>
/// Represents an <see cref="ODataSerializer"/> for serializing $ref response for a collection navigation property.
/// </summary>
public class ODataEntityReferenceLinksSerializer : ODataSerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataEntityReferenceLinksSerializer"/> class.
    /// </summary>
    public ODataEntityReferenceLinksSerializer()
        : base(ODataPayloadKind.EntityReferenceLinks)
    {
    }

    /// <inheridoc />
    public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
    {
        if (messageWriter == null)
        {
            throw Error.ArgumentNull(nameof(messageWriter));
        }

        if (writeContext == null)
        {
            throw Error.ArgumentNull(nameof(writeContext));
        }

        if (graph != null)
        {
            ODataEntityReferenceLinks entityReferenceLinks = graph as ODataEntityReferenceLinks;
            if (entityReferenceLinks == null)
            {
                IEnumerable<Uri> uris = graph as IEnumerable<Uri>;
                if (uris == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                }

                entityReferenceLinks = new ODataEntityReferenceLinks
                {
                    Links = uris.Select(uri => new ODataEntityReferenceLink { Url = uri })
                };

                if (writeContext.Request != null)
                {
                    entityReferenceLinks.Count = writeContext.Request.ODataFeature().TotalCount;
                }
            }

            await messageWriter.WriteEntityReferenceLinksAsync(entityReferenceLinks).ConfigureAwait(false);
        }
    }
}
