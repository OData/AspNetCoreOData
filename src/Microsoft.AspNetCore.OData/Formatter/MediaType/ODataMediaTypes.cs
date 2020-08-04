// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// Contains media types used by the OData formatter.
    /// </summary>
    internal static class ODataMediaTypes
    {
        public const string ApplicationJson = "application/json";
        public const string ApplicationJsonODataFullMetadata = "application/json;odata.metadata=full";
        public const string ApplicationJsonODataFullMetadataStreamingFalse = "application/json;odata.metadata=full;odata.streaming=false";
        public const string ApplicationJsonODataFullMetadataStreamingTrue = "application/json;odata.metadata=full;odata.streaming=true";
        public const string ApplicationJsonODataMinimalMetadata = "application/json;odata.metadata=minimal";
        public const string ApplicationJsonODataMinimalMetadataStreamingFalse = "application/json;odata.metadata=minimal;odata.streaming=false";
        public const string ApplicationJsonODataMinimalMetadataStreamingTrue = "application/json;odata.metadata=minimal;odata.streaming=true";
        public const string ApplicationJsonODataNoMetadata = "application/json;odata.metadata=none";
        public const string ApplicationJsonODataNoMetadataStreamingFalse = "application/json;odata.metadata=none;odata.streaming=false";
        public const string ApplicationJsonODataNoMetadataStreamingTrue = "application/json;odata.metadata=none;odata.streaming=true";
        public const string ApplicationJsonStreamingFalse = "application/json;odata.streaming=false";
        public const string ApplicationJsonStreamingTrue = "application/json;odata.streaming=true";
        public const string ApplicationXml = "application/xml";

        public static ODataMetadataLevel GetMetadataLevel(string mediaType, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (mediaType == null)
            {
                return ODataMetadataLevel.Minimal;
            }

            if (!String.Equals(ODataMediaTypes.ApplicationJson, mediaType,
                StringComparison.Ordinal))
            {
                return ODataMetadataLevel.Minimal;
            }

            Contract.Assert(parameters != null);
            KeyValuePair<string, string> odataParameter =
                parameters.FirstOrDefault(
                    (p) => String.Equals("odata.metadata", p.Key, StringComparison.OrdinalIgnoreCase));

            if (!odataParameter.Equals(default(KeyValuePair<string, string>)))
            {
                if (String.Equals("full", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.Full;
                }
                if (String.Equals("none", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.None;
                }
            }

            // Minimal is the default metadata level
            return ODataMetadataLevel.Minimal;
        }
    }
}
