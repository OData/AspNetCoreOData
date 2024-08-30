//-----------------------------------------------------------------------------
// <copyright file="KeyValuePairParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Common;

/// <summary>
/// Parsing function parameters and entity key in paths.
/// </summary>
internal class KeyValuePairParser
{
    /// <summary>
    /// Parse the expression string into key/value pairs
    /// </summary>
    /// <param name="expression">The contents of the key/value pairs.</param>
    /// <returns>true/false</returns>
    public static IDictionary<string, string> Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        ExpressionLexer lexer = new ExpressionLexer(expression);

        IDictionary<string, string> pairs = new Dictionary<string, string>();

        while (lexer.CurrentToken.Kind != ExpressionTokenKind.TextEnd)
        {
            // It should start with a literal, and the content of literal could be anything.
            lexer.ValidateToken(ExpressionTokenKind.Literal);

            string key = lexer.CurrentToken.Text;
            lexer.NextToken();

            // it could be the end or a comma, in this case, it's a single key value
            if (lexer.CurrentToken.Kind == ExpressionTokenKind.TextEnd ||
                lexer.CurrentToken.Kind == ExpressionTokenKind.Comma)
            {
                // it should be a single key value
                if (pairs.ContainsKey(string.Empty))
                {
                    throw new ODataException(Error.Format(SRResources.MultipleSingleLiteralNotAllowed, expression));
                }

                // we use the "string.empty" as the key for single key pattern.
                pairs[string.Empty] = key;

                if (lexer.CurrentToken.Kind == ExpressionTokenKind.Comma)
                {
                    lexer.NextToken();
                    continue;
                }
                else
                {
                    // Hit the text end.
                    break;
                }
            }

            // Verify the current token should be equal '=' and let's skip it.
            lexer.ValidateToken(ExpressionTokenKind.Equal);
            lexer.NextToken();

            // Verify the current token should be literal
            lexer.ValidateToken(ExpressionTokenKind.Literal);
            string value = lexer.CurrentToken.Text;

            pairs[key] = value; // if have same key, the last will win

            lexer.NextToken();
            if (lexer.CurrentToken.Kind == ExpressionTokenKind.Comma)
            {
                lexer.NextToken();
            }
        }

        return pairs;
    }

    /// <summary>
    /// Tries to parse the expression string into key/value pairs
    /// </summary>
    /// <param name="expression">The contents of the key/value pairs.</param>
    /// <param name="pairs">the output key/value pair</param>
    /// <returns>true/false</returns>
    public static bool TryParse(string expression, out IDictionary<string, string> pairs)
    {
        pairs = null;
        try
        {
            pairs = Parse(expression);
            if (pairs == null)
            {
                return false;
            }

            return true;
        }
        catch(ODataException)
        {
            return false;
        }
    }
}
