//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData entity reference link payloads.
    /// </summary>
    public class ODataEntityReferenceLinkDeserializer : ODataDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntityReferenceLinkDeserializer"/> class.
        /// </summary>
        public ODataEntityReferenceLinkDeserializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        {
        }

        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull(nameof(messageReader));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            ODataEntityReferenceLink entityReferenceLink = await messageReader.ReadEntityReferenceLinkAsync().ConfigureAwait(false);

            if (entityReferenceLink != null)
            {
                return ResolveContentId(entityReferenceLink.Url, readContext);
            }

            return null;
        }

        private static Uri ResolveContentId(Uri uri, ODataDeserializerContext readContext)
        {
            if (uri != null)
            {
                IDictionary<string, string> contentIDToLocationMapping = readContext.Request.GetODataContentIdMapping();
                if (contentIDToLocationMapping != null)
                {
                    Uri baseAddress = new Uri(readContext.Request.CreateODataLink());
                    string relativeUrl = uri.IsAbsoluteUri ? baseAddress.MakeRelativeUri(uri).OriginalString : uri.OriginalString;
                    string resolvedUrl = ContentIdHelpers.ResolveContentId(relativeUrl, contentIDToLocationMapping);
                    Uri resolvedUri = new Uri(resolvedUrl, UriKind.RelativeOrAbsolute);
                    if (!resolvedUri.IsAbsoluteUri)
                    {
                        resolvedUri = new Uri(baseAddress, uri);
                    }
                    return resolvedUri;
                }
            }

            return uri;
        }
    }
}
