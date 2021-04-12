// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class ExpressionLexerTests
    {
        [Theory]
        [InlineData("42")]
        [InlineData("123.001")]
        [InlineData("72BB05FC-38CE-44DB-A13F-974B3E623908")]
        [InlineData("'  abc''=,'")]
        [InlineData("Pròjè_x00A2_tÎð瑞갂థ్క_x0020_Iiلإَّ")]
        public void ExpressionLexerInitialShouldReturnLiteral(string expression)
        {
            // Arrange
            ExpressionLexer lexer = new ExpressionLexer(expression);

            // Act
            ExpressionToken token = lexer.CurrentToken;
            Assert.Equal(ExpressionTokenKind.Literal, token.Kind);
            Assert.Equal(expression, token.Text);
            Assert.Equal(0, token.Position);
        }

        [Fact]
        public void ExpressionLexerCanParseEqualStartingExpression()
        {
            // Arrange
            ExpressionLexer lexer = new ExpressionLexer(" = abc");

            // Act
            Assert.Equal(ExpressionTokenKind.Equal, lexer.CurrentToken.Kind);

            lexer.NextToken();
            Assert.Equal(ExpressionTokenKind.Literal, lexer.CurrentToken.Kind);
            Assert.Equal("abc", lexer.CurrentToken.Text);
            Assert.Equal(3, lexer.CurrentToken.Position);

            lexer.NextToken();
            Assert.Equal(ExpressionTokenKind.TextEnd, lexer.CurrentToken.Kind);
        }

        [Fact]
        public void ExpressionLexerCanParseCommaStartingExpression()
        {
            // Arrange
            ExpressionLexer lexer = new ExpressionLexer(" , ,abc");

            // Act
            Assert.Equal(ExpressionTokenKind.Comma, lexer.CurrentToken.Kind); // first ,

            lexer.NextToken();
            Assert.Equal(ExpressionTokenKind.Comma, lexer.CurrentToken.Kind); // second ,

            lexer.NextToken();
            Assert.Equal(ExpressionTokenKind.Literal, lexer.CurrentToken.Kind);
            Assert.Equal("abc", lexer.CurrentToken.Text);
            Assert.Equal(4, lexer.CurrentToken.Position);

            lexer.NextToken();
            Assert.Equal(ExpressionTokenKind.TextEnd, lexer.CurrentToken.Kind);
        }

        [Fact]
        public void ExpressionLexerShouldThrows()
        {
            // Arrange
            ExpressionLexer lexer = new ExpressionLexer("name='xzg");

            // Act
            lexer.NextToken(); // move to '='
            Action test = () => lexer.NextToken(); // test 'xzg

            ExceptionAssert.Throws<ODataException>(test, "There is an unterminated string literal at position 9 in 'name='xzg'.");
        }

        [Fact]
        public void ExpressionLexerShouldParseComplexKeyValuePairs()
        {
            // Arrange
            string expression = " 123 = true, inOffice = 12345, name = 'abc,''efg' , 瑞갂థ్క= @p";
            ExpressionLexer lexer = new ExpressionLexer(expression);

            // Act
            Action<string, string, bool> verifyAction = (key, value, hasComma) =>
            {
                Assert.Equal(ExpressionTokenKind.Literal, lexer.CurrentToken.Kind);
                Assert.Equal(key, lexer.CurrentToken.Text);

                lexer.NextToken();
                Assert.Equal(ExpressionTokenKind.Equal, lexer.CurrentToken.Kind);

                lexer.NextToken();
                Assert.Equal(ExpressionTokenKind.Literal, lexer.CurrentToken.Kind);
                Assert.Equal(value, lexer.CurrentToken.Text);

                lexer.NextToken();
                if (hasComma)
                {
                    Assert.Equal(ExpressionTokenKind.Comma, lexer.CurrentToken.Kind);
                    lexer.NextToken();
                }
                else
                {
                    Assert.Equal(ExpressionTokenKind.TextEnd, lexer.CurrentToken.Kind);
                }
            };

            verifyAction("123", "true", true);
            verifyAction("inOffice", "12345", true);
            verifyAction("name", "'abc,''efg'", true);
            verifyAction("瑞갂థ్క", "@p", false);
        }
    }
}
