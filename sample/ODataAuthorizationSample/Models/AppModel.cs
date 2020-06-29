using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using System.Linq;

namespace ODataAuthorizationSample.Models
{
    public class AppModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.Function("GetTopCustomer").ReturnsFromEntitySet<Customer>("Customers");

            var model = builder.GetEdmModel();
            AddPermissions(model as EdmModel);

            return model;
        }

        private static void AddPermissions(EdmModel model)
        {
            var readRestrictions = "Org.OData.Capabilities.V1.ReadRestrictions";
            var insertRestrictions = "Org.OData.Capabilities.V1.InsertRestrictions";
            var updateRestrictions = "Org.OData.Capabilities.V1.UpdateRestrictions";
            var deleteRestrictions = "Org.OData.Capabilities.V1.DeleteRestrictions";
            var operationRestrictions = "Org.OData.Capabilities.V1.OperationRestrictions";

            var customers = model.FindDeclaredEntitySet("Customers");
            var getTopCustomer = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetTopCustomer");


            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                customers,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    CreatePermissionProperty(new string[] { "Customers.Read", "Product.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", CreatePermission(new[] { "Customers.ReadByKey" })))));

            AddPermissionsTo(model, customers, insertRestrictions, "Customers.Insert");
            AddPermissionsTo(model, customers, updateRestrictions, "Customers.Update");
            AddPermissionsTo(model, customers, deleteRestrictions, "Customers.Delete");
            AddPermissionsTo(model, getTopCustomer, operationRestrictions, "Customers.GetTop");
        }

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
