//-----------------------------------------------------------------------------
// <copyright file="ODataOutputFormatterFactoryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataOutputFormatterFactoryTests
    {
        private static IEdmModel _edmModel = GetEdmModel();
        private static IList<ODataOutputFormatter> _formatters = ODataOutputFormatterFactory.Create();

        [Fact]
        public void CreateReturnsCorrectOutputFormattersCount()
        {
            // Arrange & Act & Assert
            Assert.Equal(3, _formatters.Count);
        }

        [Fact]
        public void ODataOutputFormattersContainsSupportedMediaTypes()
        {
            // Arrange
            string[] expectedMediaTypes = new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
                "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false",
                "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true",
                "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false",
                "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true",
                "application/json;odata.metadata=minimal;IEEE754Compatible=false",
                "application/json;odata.metadata=minimal;IEEE754Compatible=true",
                "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false",
                "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true",
                "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false",
                "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true",
                "application/json;odata.metadata=full;IEEE754Compatible=false",
                "application/json;odata.metadata=full;IEEE754Compatible=true",
                "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false",
                "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true",
                "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true",
                "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false",
                "application/json;odata.metadata=none;IEEE754Compatible=false",
                "application/json;odata.metadata=none;IEEE754Compatible=true",
                "application/json;odata.streaming=true;IEEE754Compatible=false",
                "application/json;odata.streaming=true;IEEE754Compatible=true",
                "application/json;odata.streaming=false;IEEE754Compatible=false",
                "application/json;odata.streaming=false;IEEE754Compatible=true",
                "application/json;IEEE754Compatible=false",
                "application/json;IEEE754Compatible=true",
                "application/xml",
                "text/plain",
                "application/octet-stream"
            };

            // Act
            IEnumerable<string> supportedMediaTypes = _formatters.SelectMany(f => f.SupportedMediaTypes).Distinct();

            // Assert
            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void ODataOutputFormattersContainsSupportedEncodings()
        {
            // Arrange
            IEnumerable<Encoding> expectedEncodings = new Encoding[]
            {
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true)
            };

            // Act
            IEnumerable<Encoding> supportedEncodings = _formatters.SelectMany(f => f.SupportedEncodings).Distinct();

            // Assert
            Assert.True(expectedEncodings.SequenceEqual(supportedEncodings));
        }

        public static TheoryDataSet<Type, string[]> OutputFormatterSupportedMediaTypesTests
        {
            get
            {
                string[] applicationJsonMediaTypes = new[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full;odata.streaming=true",
                    "application/json;odata.metadata=full;odata.streaming=false",
                    "application/json;odata.metadata=full",
                    "application/json;odata.metadata=none;odata.streaming=true",
                    "application/json;odata.metadata=none;odata.streaming=false",
                    "application/json;odata.metadata=none",
                    "application/json;odata.streaming=true",
                    "application/json;odata.streaming=false",
                    "application/json",
                    "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false",
                    "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false",
                    "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true",
                    "application/json;odata.metadata=minimal;IEEE754Compatible=false",
                    "application/json;odata.metadata=minimal;IEEE754Compatible=true",
                    "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false",
                    "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true",
                    "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false",
                    "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true",
                    "application/json;odata.metadata=full;IEEE754Compatible=false",
                    "application/json;odata.metadata=full;IEEE754Compatible=true",
                    "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false",
                    "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true",
                    "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true",
                    "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false",
                    "application/json;odata.metadata=none;IEEE754Compatible=false",
                    "application/json;odata.metadata=none;IEEE754Compatible=true",
                    "application/json;odata.streaming=true;IEEE754Compatible=false",
                    "application/json;odata.streaming=true;IEEE754Compatible=true",
                    "application/json;odata.streaming=false;IEEE754Compatible=false",
                    "application/json;odata.streaming=false;IEEE754Compatible=true",
                    "application/json;IEEE754Compatible=false",
                    "application/json;IEEE754Compatible=true"
                };

                return new TheoryDataSet<Type, string[]>
                {
                    { typeof(IEnumerable<SampleType>), applicationJsonMediaTypes },
                    { typeof(SampleType), applicationJsonMediaTypes },
                    { typeof(int), applicationJsonMediaTypes },
                    { typeof(IEnumerable<int>), applicationJsonMediaTypes },
                    { typeof(Uri), applicationJsonMediaTypes },
                    { typeof(IEnumerable<int>), applicationJsonMediaTypes },
                    { typeof(ODataServiceDocument), applicationJsonMediaTypes },
                    { typeof(IEdmModel), new [] { "application/xml", "application/json" } },
                    { typeof(ODataError), applicationJsonMediaTypes }
                };
            }
        }

        [Theory]
        [MemberData(nameof(OutputFormatterSupportedMediaTypesTests))]
        public void ODataOutputFormattersForWriteTypeReturnsSupportedMediaTypes(Type type, string[] expectedMediaTypes)
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(opt => opt.AddRouteComponents(_edmModel));
            request.Configure("", _edmModel, new ODataPath());
            var metadataDocumentFormatters = _formatters.Where(f => CanWriteType(f, type, request));

            // Act
            IEnumerable<string> supportedMediaTypes = metadataDocumentFormatters.SelectMany(f => f.SupportedMediaTypes).Distinct();

            // Assert
            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        public static TheoryDataSet<Type, MediaTypeHeaderValue> OutputFormatterContentTypeTests
        {
            get
            {
                MediaTypeHeaderValue jsonMedia = MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true");
                MediaTypeHeaderValue xmlMedia = MediaTypeHeaderValue.Parse("application/xml");

                return new TheoryDataSet<Type, MediaTypeHeaderValue>
                {
                    { typeof(IEnumerable<SampleType>), jsonMedia },
                    { typeof(SampleType), jsonMedia },
                    { typeof(int), jsonMedia },
                    { typeof(ODataServiceDocument), jsonMedia },
                    { typeof(IEdmModel), xmlMedia }
                };
            }
        }

        [Theory]
        [MemberData(nameof(OutputFormatterContentTypeTests))]
        public void ODataOutputFormatters_Feed_DefaultContentType(Type type, MediaTypeHeaderValue expect)
        {
            // Arrange & Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(_edmModel, type);

            // Assert
            Assert.Equal(expect, mediaType);
        }

        public static TheoryDataSet<Type, string, string> OutputFormatterDollarFormatContentTypeTests
        {
            get
            {
                Type[] testTypes = new[]
                {
                    typeof(IEnumerable<SampleType>),
                    typeof(SampleType),
                    typeof(int),
                    typeof(Uri),
                    typeof(IEnumerable<int>),
                    typeof(ODataServiceDocument),
                    typeof(ODataError)
                };

                KeyValuePair<string, string>[] dollarFormats = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("json", "application/json"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false"),
                    new KeyValuePair<string, string>("application%2fjson", "application/json"),
                    new KeyValuePair<string, string>("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")
                };

                TheoryDataSet<Type, string, string> data = new TheoryDataSet<Type, string, string>();
                foreach (Type type in testTypes)
                {
                    foreach (KeyValuePair<string, string> dollarFormat in dollarFormats)
                    {
                        data.Add(type, dollarFormat.Key, dollarFormat.Value);
                    }
                }

                return data;
            }
        }

        public static TheoryDataSet<Type, string, string> OutputFormatterDollarFormatForModelContentTypeTests
        {
            get
            {
                return new TheoryDataSet<Type, string, string>
                {
                    { typeof(IEdmModel), "xml", "application/xml" },
                    { typeof(IEdmModel), "application%2fxml", "application/xml" },
                };
                
            }
        }

        [Theory]
        [MemberData(nameof(OutputFormatterDollarFormatContentTypeTests))]
        [MemberData(nameof(OutputFormatterDollarFormatForModelContentTypeTests))]
        public void TestCreate_DollarFormat_Error(Type type, string dollarFormatValue, string expectedMediaType)
        {
            // Arrange & Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(_edmModel, type, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        private MediaTypeHeaderValue GetDefaultContentType(IEdmModel model, Type type)
        {
            return GetContentTypeFromQueryString(model, type, null);
        }

        private MediaTypeHeaderValue GetContentTypeFromQueryString(IEdmModel model, Type type, string dollarFormat)
        {
            Action<ODataOptions> setupAction = opt => opt.AddRouteComponents("odata", model);
            ODataPath path = new ODataPath();
            HttpRequest request = string.IsNullOrEmpty(dollarFormat)
                ? RequestFactory.Create("Get", "http://any", setupAction)
                : RequestFactory.Create("Get", "http://any/?$format=" + dollarFormat, setupAction);
            request.Configure("odata", model, path);

            var context = new OutputFormatterWriteContext(
                request.HttpContext,
                CreateWriter,
                type,
                new MemoryStream());

            foreach (var formatter in _formatters)
            {
                context.ContentType = new StringSegment();
                context.ContentTypeIsServerDefined = false;

                if (formatter.CanWriteResult(context))
                {
                    MediaTypeHeaderValue mediaType = MediaTypeHeaderValue.Parse(context.ContentType.ToString());

                    // We don't care what the charset is for these tests.
                    if (mediaType.Parameters.Where(p => p.Name == "charset").Any())
                    {
                        mediaType.Parameters.Remove(mediaType.Parameters.Single(p => p.Name == "charset"));
                    }

                    return mediaType;
                }
            }

            return null;
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntityType<SampleType>();
            return model.GetEdmModel();
        }

        private static bool CanWriteType(ODataOutputFormatter formatter, Type type, HttpRequest request)
        {
            var context = new OutputFormatterWriteContext(
                request.HttpContext,
                CreateWriter,
                objectType: type,
                @object: null);

            return formatter.CanWriteResult(context);
        }

        private static TextWriter CreateWriter(Stream stream, Encoding encoding)
        {
            const int DefaultBufferSize = 16 * 1024;
            return new HttpResponseStreamWriter(stream, encoding, DefaultBufferSize);
        }

        private class SampleType
        {
            public int Id { get; set; }
        }
    }
}
