// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Common
{
    /// <summary>
    /// Enumeration values for token kinds.
    /// </summary>
    internal enum ExpressionTokenKind
    {
        /// <summary>Unknown.</summary>
        Unknown = 0,

        /// <summary>End of text.</summary>
        TextEnd,

        /// <summary>Literal, could be an identifier, a number, null, guid, etc.</summary>
        Literal,

        /// <summary>Comma ','</summary>
        Comma,

        /// <summary>Equal '='</summary>
        Equal
    }

    /// <summary>
    /// Use this struct to represent a lexical expression token.
    /// </summary>
    internal struct ExpressionToken
    {
        /// <summary>InternalKind of token.</summary>
        public ExpressionTokenKind Kind;

        /// <summary>Token text.</summary>
        public string Text;

        /// <summary>Position of token.</summary>
        public int Position;
    }

    /// <summary>
    /// Components to parse an expression into token.
    /// </summary>
    internal class ExpressionLexer
    {
        /// <summary>The raw input text being parsed.</summary>
        protected string _rawText { get; }

        /// <summary>Length of raw input text being parsed.</summary>
        protected int _length { get; }

        /// <summary>Character being processed.</summary>
        private char? _char;

        /// <summary>Token being processed.</summary>
        private ExpressionToken _token;

        /// <summary>Position on text being parsed.</summary>
        private int _textPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionLexer" /> class.
        /// </summary>
        /// <param name="expression">The expression to lexer.</param>
        public ExpressionLexer(string expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            _rawText = expression;
            _length = expression.Length;

            SetTextPos(0);
            NextToken();
        }

        /// <summary>Token being processed. </summary>
        public ExpressionToken CurrentToken { get => _token; set => _token = value; }

        /// <summary>
        /// Reads the next token, skipping whitespace as necessary, advancing the Lexer.
        /// </summary>
        /// <returns>The parsed token.</returns>
        public ExpressionToken NextToken()
        {
            Exception error;
            ExpressionToken nextToken = this.ReadNextToken(out error);

            if (error != null)
            {
                throw error;
            }

            return nextToken;
        }

        /// <summary>Validates the current token is of the specified kind.</summary>
        /// <param name="kind">Expected token kind.</param>
        public void ValidateToken(ExpressionTokenKind kind)
        {
            if (_token.Kind != kind)
            {
                throw new ODataException(Error.Format(SRResources.ExpressionLexerSyntaxError, _textPos, _rawText));
            }
        }

        /// <summary>
        /// Gets if the current char is whitespace.
        /// </summary>
        protected virtual bool IsValidWhiteSpace => _char != null && char.IsWhiteSpace(_char.Value);

        /// <summary>
        /// Reads the next token, skipping whitespace as necessary.
        /// </summary>
        /// <param name="error">The potential error.</param>
        /// <returns>The next token, which may be 'bad' if an error occurs.</returns>
        protected virtual ExpressionToken ReadNextToken(out Exception error)
        {
            error = null;
            this.ParseWhitespace();

            ExpressionTokenKind t;
            int tokenPos = _textPos;
            switch (_char)
            {
                case ',':
                    NextChar();
                    t = ExpressionTokenKind.Comma;
                    break;

                case '=':
                    NextChar();
                    t = ExpressionTokenKind.Equal;
                    break;

                case '\'':
                    char quote = _char.Value;
                    do
                    {
                        this.AdvanceToNextOccurenceOf(quote);

                        if (_textPos == _length)
                        {
                            error = new ODataException(Error.Format(SRResources.ExpressionLexerUnterminatedStringLiteral, _textPos, _rawText));
                        }

                        // double single quote will include in the string.
                        this.NextChar();
                    }
                    while (_char.HasValue && (_char.Value == quote));

                    t = ExpressionTokenKind.Literal;
                    break;

                case '{':
                    NextChar();
                    AdvanceThroughBalancedExpression('{', '}');
                    t = ExpressionTokenKind.Literal;
                    break;

                default:
                    if (this.IsValidWhiteSpace)
                    {
                        this.ParseWhitespace();
                        t = ExpressionTokenKind.Unknown;
                        break;
                    }

                    if (_textPos == _length)
                    {
                        t = ExpressionTokenKind.TextEnd;
                        break;
                    }

                    ParseLiteral();
                    t = ExpressionTokenKind.Literal;
                    break;
            }

            _token.Kind = t;
            _token.Text = _rawText.Substring(tokenPos, _textPos - tokenPos);
            _token.Position = tokenPos;
            return _token;
        }

        /// <summary>
        /// Advance the pointer to the next occurence of the given value, swallowing all characters in between.
        /// </summary>
        /// <param name="endingValue">the ending delimiter.</param>
        protected virtual void AdvanceToNextOccurenceOf(char endingValue)
        {
            NextChar();
            while (_char.HasValue && (_char != endingValue))
            {
                NextChar();
            }
        }

        /// <summary>
        /// Parses an expression of text that we do not know how to handle in this class, which is between a
        /// <paramref name="startingCharacter"></paramref> and an <paramref name="endingCharacter"/>.
        /// </summary>
        /// <param name="startingCharacter">the starting delimiter</param>
        /// <param name="endingCharacter">the ending delimiter.</param>
        private void AdvanceThroughBalancedExpression(char startingCharacter, char endingCharacter)
        {
            int currentBracketDepth = 1;

            while (currentBracketDepth > 0)
            {
                if (_char == startingCharacter)
                {
                    currentBracketDepth++;
                }
                else if (_char == endingCharacter)
                {
                    currentBracketDepth--;
                }

                if (_char == null)
                {
                    throw new ODataException(Error.Format(SRResources.ExpressionLexer_UnbalancedBracketExpression, startingCharacter, endingCharacter));
                }

                this.NextChar();
            }
        }

        /// <summary>
        /// Parses a literal be checking for delimiting characters '\0', ',',')' and ' '
        /// </summary>
        private void ParseLiteral()
        {
            do
            {
                NextChar();
            }
            while (_char.HasValue && _char != ',' && _char != ' ' && _char != '=');
        }

        /// <summary>
        /// Advanced to the next character.
        /// </summary>
        protected void NextChar()
        {
            if (_textPos < _length)
            {
                _textPos++;
                if (_textPos < _length)
                {
                    _char = this._rawText[_textPos];
                    return;
                }
            }

            _char = null;
        }

        /// <summary>
        /// Sets the text position.
        /// </summary>
        /// <param name="pos">The new position.</param>
        private void SetTextPos(int pos)
        {
            _textPos = pos;
            _char = _textPos < _length ? _rawText[_textPos] : (char?)null;
        }

        /// <summary>
        /// Parses (skip) white spaces
        /// </summary>
        protected void ParseWhitespace()
        {
            while (IsValidWhiteSpace)
            {
                NextChar();
            }
        }

        /// <summary>This class implements IEqualityComparer for UnicodeCategory</summary>
        /// <remarks>
        /// Using this class rather than EqualityComparer&lt;T&gt;.Default
        /// saves from JIT'ing it in each AppDomain.
        /// </remarks>
        private sealed class UnicodeCategoryEqualityComparer : IEqualityComparer<UnicodeCategory>
        {
            /// <summary>
            /// Checks whether two unicode categories are equal
            /// </summary>
            /// <param name="x">first unicode category</param>
            /// <param name="y">second unicode category</param>
            /// <returns>true if they are equal, false otherwise</returns>
            public bool Equals(UnicodeCategory x, UnicodeCategory y)
            {
                return x == y;
            }

            /// <summary>
            /// Gets a hash code for the specified unicode category
            /// </summary>
            /// <param name="obj">the input value</param>
            /// <returns>The hash code for the given input unicode category, the underlying int</returns>
            public int GetHashCode(UnicodeCategory obj)
            {
                return (int)obj;
            }
        }
    }
}
