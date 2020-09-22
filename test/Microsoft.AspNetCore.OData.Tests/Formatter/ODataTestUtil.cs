// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    internal static class ODataTestUtil
    {
        public static ODataMessageWriter GetMockODataMessageWriter()
        {
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            return new ODataMessageWriter(requestMessage);
        }

        public static ODataMessageReader GetMockODataMessageReader()
        {
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            return new ODataMessageReader(requestMessage);
        }

        public static ODataSerializerProvider GetMockODataSerializerProvider(ODataEdmTypeSerializer serializer)
        {
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(sp => sp.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(serializer);
            return serializerProvider.Object;
        }

        internal static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        internal static IODataRequestMessage GetODataMessage(this HttpRequest request, string content)
        {
            // While NetCore does not use this for AspNet, it can be used here to create
            // an HttpRequestODataMessage, which is a Test type that implments IODataRequestMessage
            // wrapped around an HttpRequestMessage.

            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            request.Body = new MemoryStream(contentBytes);
            request.ContentType = "application/json";
            request.ContentLength = contentBytes.Length;
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Accept", "application/json;odata.metadata=full");

            //MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            //mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            //request.Headers.Accept.Add(mediaType);
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new HttpRequestODataMessage(request);
        }
    }
}
