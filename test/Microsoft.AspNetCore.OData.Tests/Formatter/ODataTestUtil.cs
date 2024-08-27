//-----------------------------------------------------------------------------
// <copyright file="ODataTestUtil.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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

        public static IODataSerializerProvider GetMockODataSerializerProvider(ODataEdmTypeSerializer serializer)
        {
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            serializerProvider.Setup(sp => sp.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(serializer);
            return serializerProvider.Object;
        }

        internal static ODataMessageReader GetODataMessageReader(IODataRequestMessageAsync oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        internal static IODataRequestMessageAsync GetODataMessage(this HttpRequest request, string content)
        {
            // While NetCore does not use this for AspNet, it can be used here to create
            // an HttpRequestODataMessage, which is a Test type that implments IODataRequestMessage
            // wrapped around an HttpRequestMessage.

            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            request.Body = new MemoryStream(contentBytes);
            request.ContentType = "application/json";
            request.ContentLength = contentBytes.Length;
            request.Headers.Append("OData-Version", "4.0");
            request.Headers.Append("Accept", "application/json;odata.metadata=full");

            //MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            //mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            //request.Headers.Accept.Add(mediaType);
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new HttpRequestODataMessage(request);
        }
    }
}
