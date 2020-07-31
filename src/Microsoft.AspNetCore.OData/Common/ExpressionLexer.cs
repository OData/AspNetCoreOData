// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Common
{
    internal class ExpressionLexer
    {
        public ExpressionLexer(string expression)
        {
            Text = expression;
            TextLen = expression.Length;
        }

        /// <summary>Text being parsed.</summary>
        protected string Text { get; }

        /// <summary>Length of text being parsed.</summary>
        protected int TextLen { get; }
    }
}
