//-----------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.Common
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Unescape Uri string for %2F
        /// See details at: https://github.com/dotnet/aspnetcore/issues/14170#issuecomment-533342396
        /// </summary>
        /// <param name="uriString">The Uri string.</param>
        /// <returns>Unescaped back slash Uri string.</returns>
        public static string UnescapeBackSlashUriString(this string uriString)
        {
            if (uriString == null)
            {
                return null;
            }

            return uriString.Replace("%2f", "%2F").Replace("%2F", "/");
        }

        /// <summary>
        /// Escape Uri string for '/'
        /// </summary>
        /// <param name="uriString">The Uri string.</param>
        /// <returns>Escaped back slash Uri string.</returns>
        public static string EscapeBackSlashUriString(this string uriString)
        {
            if (uriString == null)
            {
                return null;
            }

            return uriString.Replace("/", "%2F");
        }

        /// <summary>
        /// Normalize the http method.
        /// </summary>
        /// <param name="method">The http method.</param>
        /// <returns>Normalized http method.</returns>
        internal static string NormalizeHttpMethod(this string method)
        {
            switch (method.ToUpperInvariant())
            {
                case "POSTTO":
                    return "Post";

                case "PUTTO":
                    return "Put";

                case "PATCHTO":
                    return "Patch";

                case "DELETETO":
                    return "Delete";

                default:
                    return method;
            }
        }

        /// <summary>
        /// Check whether given literal matches the uri template pattern {literals}.
        /// </summary>
        /// <param name="literalText">The text to be evaluated</param>
        /// <returns>True if <paramref name="literalText"/> is valid for Uri template</returns>
        internal static bool IsValidTemplateLiteral(this string literalText)
        {
            return !string.IsNullOrEmpty(literalText)
                && literalText.StartsWith("{", StringComparison.Ordinal)
                && literalText.EndsWith("}", StringComparison.Ordinal);
        }

#if false // comment out the following codes for backup.
        /// <summary>
        /// Extract key/value pairs, the value could have "=" or ', or "", etc.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="pairs"></param>
        /// <returns>true or false</returns>
        public static bool TryExtractKeyValuePairs(this string input, out IDictionary<string, string> pairs)
        {
            if (input == null)
            {
                throw Error.ArgumentNull(nameof(input));
            }

            // "  minSalary='af''d,2,=897abc' , maxSalary=3"
            pairs = new Dictionary<string, string>();
            input = input.Trim();

            int length = input.Length;
            int start = 0;
            string key, value;
            int end;
            while (true)
            {
                (key, end) = ReadKey(input, start);

                if (end == length || input[end] != '=')
                {
                    throw new ODataException();
                }

                start = end + 1; // skip =
                (value, end) = ReadValue(input, start);

                pairs[key.Trim()] = value.Trim();
                start = end + 1;

                if (end >= length)
                {
                    break;
                }
            }

            return true;
        }

        private static (string, int) ReadKey(string input, int start)
        {
            int length = input.Length;
            int end = start;
            for (; end < length; end++)
            {
                if (input[end] == '=')
                {
                    break;
                }
            }

            if (end == length)
            {
                throw new ODataException("");
            }

            return (input.Substring(start, end - start), end);
        }

        private static (string, int) ReadValue(string input, int start)
        {
            int length = input.Length;

            char ch = input[start];
            if (ch == '\'') // TODO: it could be {. [
            {
                int end = start;
                do
                {
                    int j = end + 1;
                    for (; j < length; j++)
                    {
                        if (input[j] == '\'')
                        {
                            break;
                        }
                    }

                    if (j == length)
                    {
                        throw new ODataException("");
                    }

                    end = j + 1;
                }
                while (end != length && input[end] == '\'');

                return (input.Substring(start, end - start), end);
            }
            else
            {
                int j = start + 1;
                for (; j < length; j++)
                {
                    if (input[j] == ',')
                    {
                        break;
                    }
                }

                if (j == length)
                {
                    return (input.Substring(start), j);
                }

                return (input.Substring(start, j - start), j);
            }
        }

        /// <summary>
        /// Each key/value pair is separated using ",".
        /// Each value could be a string using single quote, so it could have "," and escaped double single quote.
        /// "minSalary=2,maxSalary=3"
        /// "minSalary='af''d,2,897abc' , maxSalary=3"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ExtractKeyValuePairs(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // "minSalary=2,maxSalary=3"
            // "minSalary='afd,2',maxSalary=3"
            input = input.Trim();

            IDictionary<string, string> result = new Dictionary<string, string>();
            while (!string.IsNullOrEmpty(input))
            {
                int index = input.IndexOf("=", StringComparison.Ordinal);
                if (index <= 0)
                {
                    throw new ODataException($"Invalid string '{input}' without '='.");
                }

                string key = input.Substring(0, index);
                input = input.Substring(index + 1); // ignore =
                if (string.IsNullOrEmpty(input))
                {
                    throw new ODataException($"Empty string '{input}'");
                }

                string value;
                if (input[0] == '\'')
                {
                    bool found = false;
                    int i = 1;
                    // If it's a string
                    for (; i < input.Length; i++)
                    {
                        if (input[i] == '\'')
                        {
                            if (i == input.Length - 1)
                            {
                                found = true;
                                break;
                            }
                            else
                            {
                                if (input[i + 1] == '\'') // '' escaped single quote
                                {
                                    i++;
                                    continue;
                                }
                                else
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!found)
                    {
                        throw new ODataException($"non match ''' for '{input}'");
                    }

                    value = input.Substring(i);
                    input = input.Substring(i + 1);
                }
                else
                {
                    int valueIndex = input.IndexOf(",", StringComparison.Ordinal);
                    if (valueIndex < 0)
                    {
                        value = input;
                        input = null;
                    }
                    else
                    {
                        value = input.Substring(0, valueIndex);
                        input = input.Substring(valueIndex + 1); // ignore ,
                    }
                }

                result[key] = value;
                input = input.Trim();
            }

            return result;
        }

        public static string[] ExtractItems(this string input, params string[] seperators)
        {
            string text = input;
            List<string> items = new List<string>();
            for (int i = seperators.Length - 1; i >= 0; i--)
            {
                int index = text.IndexOf(seperators[i], StringComparison.Ordinal);
                if (index > 0)
                {
                    items.Add(text.Substring(index + seperators[i].Length));
                    text = text.Substring(0, index);
                }
            }

            items.Reverse();
            return items.ToArray();
        }

        public static string ExtractParenthesis(this string identifier, out string parenthesisExpressions)
        {
            // also supports: name(abc)(efg)
            // => name
            // => (abc)(efg)
            parenthesisExpressions = null;

            int parenthesisStart = identifier.IndexOf('(', StringComparison.Ordinal);
            if (parenthesisStart >= 0)
            {
                if (identifier[identifier.Length - 1] != ')')
                {
                    throw new Exception($"Invalid identifier {identifier}, cannot find the end ')' character.");
                }

                // split the string to grab the identifier and remove the parentheses
                string returnStr = identifier.Substring(0, parenthesisStart);
                parenthesisExpressions = identifier.Substring(parenthesisStart, identifier.Length - returnStr.Length);
                identifier = returnStr;
            }

            return identifier;
        }

        public static void ExtractKeyValuePairs(this string input, out IDictionary<string, string> pairs, out string remaining)
        {
            pairs = new Dictionary<string, string>();
            remaining = null;

            if (String.IsNullOrEmpty(input))
            {
                return;
            }

            // maybe with keys after function parameters
            // .... (...)(...)
            int startIndex = input.IndexOf('(', StringComparison.Ordinal);
            int endIndex = input.IndexOf(')', startIndex + 1);
            string parenthsis = input.Substring(startIndex + 1, endIndex - startIndex - 1);

            if (endIndex != input.Length - 1)
            {
                remaining = input.Substring(endIndex + 1);
            }

            var items = parenthsis.Split(',');
            foreach (var item in items)
            {
                var subItems = item.Split('=');

                if (subItems.Length == 1)
                {
                    if (pairs.TryGetValue(String.Empty, out string value))
                    {
                        throw new Exception($"Invalid string '{input}', has multiple items without '='.");
                    }

                    // VerifyIsKeyOrParameterTemplate(subItems[0]);

                    pairs[String.Empty] = subItems[0].Trim();
                }
                else if (subItems.Length == 2)
                {
                    if (String.IsNullOrEmpty(subItems[0]))
                    {
                        throw new Exception($"Invalid string '{input}', has empty key in '{item}'.");
                    }

                    // VerifyIsKeyOrParameterTemplate(subItems[1]);
                    pairs[subItems[0].Trim()] = subItems[1].Trim();
                }
                else
                {
                    throw new Exception($"Invalid parameter at {item}");
                }
            }
        }
#endif
    }
}
