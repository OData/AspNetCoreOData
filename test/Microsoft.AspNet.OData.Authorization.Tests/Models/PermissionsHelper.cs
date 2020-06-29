using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.AspNet.OData.Authorization.Tests.Models
{
    public class PermissionsHelper
    {
        public static void AddPermissionsTo(EdmModel model, IEdmVocabularyAnnotatable target, string restrictionName, params string[] scopes)
        {
            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                target,
                model.FindTerm(restrictionName),
                CreatePermission(scopes)));
        }

        public static IEdmExpression CreatePermission(params string[] scopeNames)
        {
            var restriction = new EdmRecordExpression(
                CreatePermissionProperty(scopeNames));

            return restriction;
        }

        public static IEdmPropertyConstructor CreatePermissionProperty(params string[] scopeNames)
        {
            var scopes = scopeNames.Select(scope => new EdmRecordExpression(
                   new EdmPropertyConstructor("Scope", new EdmStringConstant(scope)),
                   new EdmPropertyConstructor("RestrictedProperties", new EdmStringConstant("*"))));

            var permission = new EdmRecordExpression(
                new EdmPropertyConstructor("SchemeName", new EdmStringConstant("AuthScheme")),
                new EdmPropertyConstructor("Scopes", new EdmCollectionExpression(scopes)));

            var property = new EdmPropertyConstructor("Permissions", new EdmCollectionExpression(permission));
            return property;
        }
    }
}
