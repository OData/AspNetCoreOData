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
            //check if the replacement characters are not null
            if (replaceAt == null || replaceColon == null || replaceDot == null)
            {
                throw new ArgumentNullException("Replacement characters cannot be null");
            }

            // check if any of the the replacement characters provided contain any of the illegal characters
            // ex. if replaceAt contains any of the illegal characters checked one by one
            if (illegalChars.Any(illegalChar => replaceAt.Contains(illegalChar) || replaceColon.Contains(illegalChar) || replaceDot.Contains(illegalChar)))
            {
                throw new ArgumentException("Replacement character cannot be an illegal character");
            }

            ReplaceAt = replaceAt;
            ReplaceColon = replaceColon;
            ReplaceDot = replaceDot;
        }

        public ReplaceIllegalFieldNameCharactersAttribute(string replaceAnyIllegal)
        {
            if (illegalChars.Any(illegalChar => replaceAnyIllegal.Contains(illegalChar)))
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
