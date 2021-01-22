// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class KeyValuePairParseTests
    {
        [Fact]
        public void ParseThrowsUnterminatedStringLiteral()
        {
            // Arrange & Act & Assert
            Action test = () => KeyValuePairParser.Parse(" inOffice='xzg  ");

            ExceptionAssert.Throws<ODataException>(test, "There is an unterminated string literal at position 16 in ' inOffice='xzg  '.");
        }

        [Theory]
        [InlineData(" =key", 2)]
        [InlineData(" ,", 2)]
        [InlineData(" =key value", 2)]
        [InlineData(" key value", 10)]
        [InlineData(" key value=", 10)]
        public void ParseThrowsSyntaxError(string expression, int pos)
        {
            // Arrange & Act & Assert
            Action test = () => KeyValuePairParser.Parse(expression);

            ExceptionAssert.Throws<ODataException>(test, string.Format("Syntax error at position {0} in '{1}'.", pos, expression));
        }

        [Fact]
        public void ParseThrowsMultipleLiteralsNotAllowed()
        {
            // Arrange & Act & Assert
            Action test = () => KeyValuePairParser.Parse(" inOffice , xzg  ");

            ExceptionAssert.Throws<ODataException>(test, "' inOffice , xzg  ' is not a valid expression.Single literal is only for single key. Multiple keys should use key=value.");
        }

        [Theory]
        [InlineData("inOffice = 1, xzg")]
        [InlineData("xzg, inOffice = 1 ")]
        public void ParseRetunsCorrectlyForSingleLiteralAndKeyValue(string expression)
        {
            // Arrange & Act
            IDictionary<string, string> pairs = KeyValuePairParser.Parse(expression);

            // Assert
            Assert.NotNull(pairs);
            Assert.Equal(2, pairs.Count);
            Assert.Equal("xzg", pairs[string.Empty]);
            Assert.Equal("1", pairs["inOffice"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void ParseReturnsNullForNullEmptyOrWhitespaceString(string expression)
        {
            // Arrange & Act
            IDictionary<string, string> pairs = KeyValuePairParser.Parse(expression);

            // Assert
            Assert.Null(pairs);
        }

        [Theory]
        [InlineData("123=true,inOffice=12345,name='abc,''efg'")]
        [InlineData("123 = true , inOffice  =  12345 , name=   'abc,''efg'   ")]
        [InlineData("   123 = true , inOffice  =  12345 , name=   'abc,''efg'")]
        public void ParseComplexFunctionParametersWorks(string expression)
        {
            // Arrange & Act
            IDictionary<string, string> pairs = KeyValuePairParser.Parse(expression);

            Assert.NotNull(pairs);
            Assert.Equal(3, pairs.Count);
            Assert.Equal("true", pairs["123"]);
            Assert.Equal("12345", pairs["inOffice"]);
            Assert.Equal("'abc,''efg'", pairs["name"]);
        }

        [Theory]
        [InlineData("123=@p1,myname=@p2")]
        [InlineData("   123=@p1,myname=@p2")]
        [InlineData("123=@p1,myname=@p2   ")]
        [InlineData("  123= @p1,  myname= @p2   ")]
        public void ParseForParameterAliasWorks(string expression)
        {
            // Arrange & Act
            IDictionary<string, string> pairs = KeyValuePairParser.Parse(expression);

            Assert.NotNull(pairs);
            Assert.Equal(2, pairs.Count);
            Assert.Equal("@p1", pairs["123"]);
            Assert.Equal("@p2", pairs["myname"]);
        }

        [Theory]
        [InlineData("'abc'")]
        [InlineData("'abc'  ")]
        [InlineData("    'abc'")]
        [InlineData("  'abc'   ")]
        public void TryParseWorksForSingleKeyString(string expression)
        {
            // Arrange & Act
            IDictionary<string, string> pairs1 = KeyValuePairParser.Parse(expression);
            bool result = KeyValuePairParser.TryParse(expression, out IDictionary<string, string> pairs2);

            // Assert
            Assert.True(result);
            Assert.Equal(pairs1, pairs2);

            KeyValuePair<string, string> keyValue = Assert.Single(pairs1);
            Assert.Equal(string.Empty, keyValue.Key);
            Assert.Equal("'abc'", keyValue.Value);
        }
    }
}
