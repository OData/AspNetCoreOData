using System;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Formatter.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ReplaceIllegalFieldNameCharactersAttribute : Attribute
    {
        //constant collection of illegal characters
        private static readonly string[] illegalChars = new string[] { "@", ":", "." };
        public string ReplaceAt { get; }
        public string ReplaceColon { get; }
        public string ReplaceDot { get; }

        public ReplaceIllegalFieldNameCharactersAttribute(string replaceAt, string replaceColon, string replaceDot)
        {
            //assert that the replacements are not illegal either
            if (illegalChars.Contains(replaceAt) || illegalChars.Contains(replaceColon) || illegalChars.Contains(replaceDot))
            {
                throw new ArgumentException("Replacement characters cannot be illegal characters");
            }

            ReplaceAt = replaceAt;
            ReplaceColon = replaceColon;
            ReplaceDot = replaceDot;
        }

        public ReplaceIllegalFieldNameCharactersAttribute(string replaceAnyIllegal)
        {
            if (illegalChars.Contains(replaceAnyIllegal))
            {
                throw new ArgumentException("Replacement character cannot be an illegal character");
            }

            ReplaceAt = replaceAnyIllegal;
            ReplaceColon = replaceAnyIllegal;
            ReplaceDot = replaceAnyIllegal;
        }

        public ReplaceIllegalFieldNameCharactersAttribute()
        {
            ReplaceAt = "_";
            ReplaceColon = "_";
            ReplaceDot = "_";
        }

        public string Replace(string fieldName)
        {
            return fieldName.Replace("@", ReplaceAt).Replace(":", ReplaceColon).Replace(".", ReplaceDot);
        }
    }
}
