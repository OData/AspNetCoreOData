// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Text;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    public class StringContentWithLength : StringContent
    {
        public StringContentWithLength(string content)
            : base(content)
        {
            EnsureContentLength();
        }

        public StringContentWithLength(string content, Encoding encoding)
            : base(content, encoding)
        {
            EnsureContentLength();
        }

        public StringContentWithLength(string content, Encoding encoding, string mediaType)
            : base(content, encoding, mediaType)
        {
            EnsureContentLength();
        }

        public StringContentWithLength(string content, string unvalidatedContentType)
            : base(content)
        {
            Headers.TryAddWithoutValidation("Content-Type", unvalidatedContentType);
            EnsureContentLength();
        }

        private void EnsureContentLength()
        {
            // See: https://github.com/dotnet/aspnetcore/issues/18463
            _ = Headers.ContentLength;
        }
    }
}
