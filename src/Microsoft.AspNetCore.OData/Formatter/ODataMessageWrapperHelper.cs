﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Formatter
{
    internal static class ODataMessageWrapperHelper
    {
        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers)
        {
            return Create(stream, headers, contentIdMapping: null);
        }

        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers, IServiceProvider container)
        {
            return Create(stream, headers, null, container);
        }

        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers, IDictionary<string, string> contentIdMapping, IServiceProvider container)
        {
            ODataMessageWrapper responseMessageWrapper = Create(stream, headers, contentIdMapping);
            responseMessageWrapper.Container = container;

            return responseMessageWrapper;
        }

        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers, IDictionary<string, string> contentIdMapping)
        {
            return new ODataMessageWrapper(
                stream,
                headers.ToDictionary(kvp => kvp.Key, kvp => string.Join(";", kvp.Value)),
                contentIdMapping);
        }
    }
}
