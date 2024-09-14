//-----------------------------------------------------------------------------
// <copyright file="OrderByQueryOptionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class OrderByQueryOptionTest
{
    [Fact]
    public void ConstructorNullContextThrows()
    {
        ExceptionAssert.Throws<ArgumentNullException>(() =>
            new OrderByQueryOption("Name", null));
    }

    [Fact]
    public void ConstructorNullRawValueThrows()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

        // Act & Assert
        ExceptionAssert.Throws<ArgumentException>(() =>
            new OrderByQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
    }

    [Fact]
    public void ConstructorEmptyRawValueThrows()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

        // Act & Assert
        ExceptionAssert.Throws<ArgumentException>(() =>
            new OrderByQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
    }

    [Fact]
    public void ConstructorNullQueryOptionParserThrows()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() =>
            new OrderByQueryOption("test", new ODataQueryContext(model, typeof(Customer)), queryOptionParser: null),
            "queryOptionParser");
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("''")]
    public void CanConstructValidFilterQuery(string orderbyValue)
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer));
        var orderby = new OrderByQueryOption(orderbyValue, context);

        Assert.Same(context, orderby.Context);
        Assert.Equal(orderbyValue, orderby.RawValue);
    }

    [Fact]
    public void PropertyNodes_Getter_Parses_Query()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderby = new OrderByQueryOption("Name,Website", context);

        ICollection<OrderByNode> nodes = orderby.OrderByNodes;

        // Assert
        Assert.False(nodes.OfType<OrderByItNode>().Any());
        IEnumerable<OrderByPropertyNode> propertyNodes = nodes.OfType<OrderByPropertyNode>();
        Assert.NotNull(propertyNodes);
        Assert.Equal(2, propertyNodes.Count());
        Assert.Equal("Name", propertyNodes.First().Property.Name);
        Assert.Equal("Website", propertyNodes.Last().Property.Name);
    }

    //[Theory]
    //[InlineData("BadPropertyName")]
    //[InlineData("''")]
    //[InlineData(" ")]
    //[InlineData("Id,Id")]
    //[InlineData("Id,Name,Id")]
    //public void ApplyInValidOrderbyQueryThrows(string orderbyValue)
    //{
    //    var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
    //    var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
    //    var orderby = new OrderByQueryOption(orderbyValue, context);

    //    ExceptionAssert.Throws<ODataException>(() =>
    //        orderby.ApplyTo(ODataQueryOptionTest.Customers));
    //}

    [Theory]
    [InlineData("id")]
    [InlineData("iD")]
    [InlineData("ID")]
    public void CanApplyOrderByQueryCaseInsensitive(string orderbyValue)
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderby = new OrderByQueryOption(orderbyValue, context);

        var customers = (new List<Customer>{
            new Customer { Id = 2, Name = "Aaron" },
            new Customer { Id = 1, Name = "Andy" },
            new Customer { Id = 3, Name = "Alex" }
        }).AsQueryable();

        var results = orderby.ApplyTo(customers).ToArray();
        Assert.Equal(1, results[0].Id);
        Assert.Equal(2, results[1].Id);
        Assert.Equal(3, results[2].Id);
    }

    [Fact]
    [Trait("Description", "Can apply an orderby")]
    public void CanApplyOrderBy()
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Name", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "Andy" },
            new Customer { Id = 2, Name = "Aaron" },
            new Customer { Id = 3, Name = "Alex" }
        }).AsQueryable();

        var results = orderByOption.ApplyTo(customers).ToArray();
        Assert.Equal(2, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(1, results[2].Id);
    }

    [Fact]
    [Trait("Description", "Can apply an orderby")]
    public void CanApplyOrderByAsc()
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Name asc", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "Andy" },
            new Customer { Id = 2, Name = "Aaron" },
            new Customer { Id = 3, Name = "Alex" }
        }).AsQueryable();

        var results = orderByOption.ApplyTo(customers).ToArray();
        Assert.Equal(2, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(1, results[2].Id);
    }

    [Fact]
    [Trait("Description", "Can apply an orderby descending")]
    public void CanApplyOrderByDescending()
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Name desc", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "Andy" },
            new Customer { Id = 2, Name = "Aaron" },
            new Customer { Id = 3, Name = "Alex" }
        }).AsQueryable();

        var results = orderByOption.ApplyTo(customers).ToArray();
        Assert.Equal(1, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(2, results[2].Id);
    }

    [Fact]
    [Trait("Description", "Can apply a compound orderby")]
    public void CanApplyOrderByThenBy()
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Name,Website", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "ACME", Website = "http://www.acme.net" },
            new Customer { Id = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
            new Customer { Id = 3, Name = "ACME", Website = "http://www.acme.com" }
        }).AsQueryable();

        var results = orderByOption.ApplyTo(customers).ToArray();
        Assert.Equal(2, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(1, results[2].Id);
    }

    [Fact]
    [Trait("Description", "Can apply a OrderByDescending followed by ThenBy")]
    public void CanApplyOrderByDescThenBy()
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Name desc,Website", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "ACME", Website = "http://www.acme.net" },
            new Customer { Id = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
            new Customer { Id = 3, Name = "ACME", Website = "http://www.acme.com" }
        }).AsQueryable();

        var results = orderByOption.ApplyTo(customers).ToArray();
        Assert.Equal(3, results[0].Id);
        Assert.Equal(1, results[1].Id);
        Assert.Equal(2, results[2].Id);
    }

    [Fact]
    [Trait("Description", "Can apply a OrderByDescending followed by ThenBy")]
    public void CanApplyOrderByDescThenByDesc()
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Name desc,Website desc", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "ACME", Website = "http://www.acme.net" },
            new Customer { Id = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
            new Customer { Id = 3, Name = "ACME", Website = "http://www.acme.com" }
        }).AsQueryable();

        var results = orderByOption.ApplyTo(customers).ToArray();
        Assert.Equal(1, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(2, results[2].Id);
    }

    [Fact]
    public void ApplyToEnums_ReturnsCorrectQueryable()
    {
        // Arrange
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<EnumModel>("EnumModels");
        var model = builder.GetEdmModel();

        var context = new ODataQueryContext(model, typeof(EnumModel)) { RequestContainer = new MockServiceProvider() };
        var orderbyOption = new OrderByQueryOption("Flag", context);
        IEnumerable<EnumModel> enumModels = FilterQueryOptionTest.EnumModelTestData;

        // Act
        IQueryable queryable = orderbyOption.ApplyTo(enumModels.AsQueryable());

        // Assert
        Assert.NotNull(queryable);
        IEnumerable<EnumModel> actualCustomers = Assert.IsAssignableFrom<IEnumerable<EnumModel>>(queryable);
        Assert.Equal(
            new int[] { 5, 2, 1, 3, 6 },
            actualCustomers.Select(enumModel => enumModel.Id));
    }

    [Theory]
    [InlineData(true, "FirstNameAlias")]
    [InlineData(false, "FirstName")]
    public void ApplyTo_PropertyAliased_IfEnabled(bool modelAliasing, string propertyName)
    {
        // Arrange
        var builder = new ODataConventionModelBuilder(TestAssemblyResolver.Instance);
        builder.ModelAliasingEnabled = modelAliasing;
        builder.EntitySet<PropertyAlias>("PropertyAliases");
        var model = builder.GetEdmModel();

        var context = new ODataQueryContext(model, typeof(PropertyAlias)) { RequestContainer = new MockServiceProvider(model) };
        var orderByOption = new OrderByQueryOption(propertyName, context);
        IEnumerable<PropertyAlias> propertyAliases = FilterQueryOptionTest.PropertyAliasTestData;

        // Act
        IQueryable queryable = orderByOption.ApplyTo(propertyAliases.AsQueryable());

        // Assert
        Assert.NotNull(queryable);
        IEnumerable<PropertyAlias> actualCustomers = Assert.IsAssignableFrom<IEnumerable<PropertyAlias>>(queryable);
        Assert.Equal(
            new[] { "abc", "def", "xyz" },
            actualCustomers.Select(propertyAlias => propertyAlias.FirstName));
    }

    [Theory]
    [InlineData("SharePrice add 1")]
    [InlineData("tolower(Name)")]
    public void OrderBy_Works_For_Expressions(string orderByQuery)
    {
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var orderByOption = new OrderByQueryOption(orderByQuery, new ODataQueryContext(model, typeof(Customer)));

        var nodes = orderByOption.OrderByNodes;
        OrderByNode orderByNode = Assert.Single(nodes);
        OrderByClauseNode clauseNode = Assert.IsType<OrderByClauseNode>(orderByNode);
    }

    //[Fact]
    //public void CanTurnOffValidationForOrderBy()
    //{
    //    // Arrange
    //    ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();

    //    OrderByQueryOption option = new OrderByQueryOption("Name", context);
    //    ODataValidationSettings settings = new ODataValidationSettings();
    //    settings.AllowedOrderByProperties.Add("Id");

    //    // Act & Assert
    //    ExceptionAssert.Throws<ODataException>(() => option.Validate(settings),
    //        "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");

    //    option.Validator = null;
    //    ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
    //}

    [Fact]
    public void OrderByDuplicatePropertyThrows()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderbyOption = new OrderByQueryOption("Name, Name", context);

        // Act
        ExceptionAssert.Throws<ODataException>(
            () => orderbyOption.ApplyTo(Enumerable.Empty<Customer>().AsQueryable()),
            "Duplicate property named 'Name' is not supported in '$orderby'.");
    }

    [Fact]
    public void OrderByDuplicateItThrows()
    {
        // Arrange
        var context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        var orderbyOption = new OrderByQueryOption("$it, $it", context);

        // Act
        ExceptionAssert.Throws<ODataException>(
            () => orderbyOption.ApplyTo(Enumerable.Empty<int>().AsQueryable()),
            "Multiple '$it' nodes are not supported in '$orderby'.");
    }

    [Fact]
    public void OrderByDuplicatePropertyOfComplexTypeThrows()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();

        var context = new ODataQueryContext(model, typeof(Customer)){ RequestContainer = new MockServiceProvider() };
        var orderbyOption = new OrderByQueryOption("Address/City, Address/City", context);

        // Act
        ExceptionAssert.Throws<ODataException>(
            () => orderbyOption.ApplyTo(Enumerable.Empty<Customer>().AsQueryable()),
            "Duplicate property named 'Address/City' is not supported in '$orderby'.");
    }

    [Fact]
    public void ApplyTo_NestedProperties_Succeeds()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Address/City asc", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Address = new Address { City = "C" } },
            new Customer { Id = 2, Address = new Address { City = "B" } },
            new Customer { Id = 3, Address = new Address { City = "A" } }
        }).AsQueryable();

        // Act
        var results = orderByOption.ApplyTo(customers).ToArray();

        // Assert
        Assert.Equal(3, results[0].Id);
        Assert.Equal(2, results[1].Id);
        Assert.Equal(1, results[2].Id);
    }

    [Fact]
    public void ApplyTo_NestedProperties_WithDuplicateName_Succeeds()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Address/City,City", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, City = "A", Address = new Address { City = "A" } },
            new Customer { Id = 2, City = "B", Address = new Address { City = "B" } },
            new Customer { Id = 3, City = "A", Address = new Address { City = "B" } }
        }).AsQueryable();

        // Act
        var results = orderByOption.ApplyTo(customers).ToArray();

        // Assert
        Assert.Equal(1, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(2, results[2].Id);
    }

    [Fact]
    public void ApplyTo_NestedProperties_WithDuplicatePathType_Succeeds()
    {
        // Arrange
        var model =
            new ODataModelBuilder().Add_Customer_EntityType_With_DuplicatedAddress()
                .Add_Customers_EntitySet()
                .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) {RequestContainer = new MockServiceProvider()};
        var orderByOption = new OrderByQueryOption("City,Address/City,WorkAddress/City", context);
        var customers = (new List<Customer>
        {
            new Customer
            {
                Id = 1,
                City = "B",
                Address = new Address {City = "B"},
                WorkAddress = new Address {City = "B"}
            },
            new Customer
            {
                Id = 2,
                City = "B",
                Address = new Address {City = "B"},
                WorkAddress = new Address {City = "A"}
            },
            new Customer
            {
                Id = 3,
                City = "B",
                Address = new Address {City = "A"},
                WorkAddress = new Address {City = "A"}
            },
            new Customer
            {
                Id = 4,
                City = "A",
                Address = new Address {City = "A"},
                WorkAddress = new Address {City = "A"}
            }
        }).AsQueryable();

        // Act
        var results = orderByOption.ApplyTo(customers).ToArray();

        // Assert
        Assert.Equal(4, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(2, results[2].Id);
        Assert.Equal(1, results[3].Id);
    }

    [Fact]
    public void ApplyTo_NestedProperties_HandlesNullPropagation_Succeeds()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Address/City asc", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Address = null },
            new Customer { Id = 2, Address = new Address { City = "B" } },
            new Customer { Id = 3, Address = new Address { City = "A" } }
        }).AsQueryable();

        // Act
        ODataQuerySettings settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True };
        var results = orderByOption.ApplyTo(customers, settings).ToArray();

        // Assert
        Assert.Equal(1, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(2, results[2].Id);
    }

    [Fact]
    public void ApplyTo_NestedProperties_DoesNotHandleNullPropagation_IfExplicitInSettings()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Address/City asc", context);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Address = null },
            new Customer { Id = 2, Address = new Address { City = "B" } },
            new Customer { Id = 3, Address = new Address { City = "A" } }
        }).AsQueryable();
        ODataQuerySettings settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False };

        // Act & Assert
        ExceptionAssert.Throws<NullReferenceException>(() => orderByOption.ApplyTo(customers, settings).ToArray());
    }

    [Fact]
    public void Property_OrderByNodes_WorksWithUnTypedContext()
    {
        // Arrange
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
        OrderByQueryOption orderBy = new OrderByQueryOption("ID desc", context);

        // Act & Assert
        Assert.NotNull(orderBy.OrderByNodes);
    }

    [Fact]
    public void ApplyTo_WithUnTypedContext_Throws_InvalidOperation()
    {
        // Arrange
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
        OrderByQueryOption orderBy = new OrderByQueryOption("ID desc", context);
        IQueryable queryable = new Mock<IQueryable>().Object;

        // Act & Assert
        ExceptionAssert.Throws<NotSupportedException>(() => orderBy.ApplyTo(queryable),
            "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
    }

    [Fact]
    public void CanApplyOrderBy_WithParameterAlias()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Address_ComplexType().GetEdmModel();

        var parser = new ODataQueryOptionParser(
            model,
            model.FindType("Microsoft.AspNetCore.OData.Tests.Models.Customer"),
            model.FindDeclaredNavigationSource("Default.Container.Customers"),
            new Dictionary<string, string> { { "$orderby", "@q desc,@p asc" }, { "@q", "Address/HouseNumber" }, { "@p", "Id" } });

        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("@q desc,@p asc", context, parser);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Address = new Address{HouseNumber = 2}},
            new Customer { Id = 2, Address = new Address{HouseNumber = 1}},
            new Customer { Id = 3, Address = new Address{HouseNumber = 3}},
            new Customer { Id = 4, Address = new Address{HouseNumber = 2}},
            new Customer { Id = 5, Address = new Address{HouseNumber = 1}},
        }).AsQueryable();

        // Act
        var results = orderByOption.ApplyTo(customers).ToArray();

        // Assert
        Assert.Equal(3, results[0].Id);
        Assert.Equal(1, results[1].Id);
        Assert.Equal(4, results[2].Id);
        Assert.Equal(2, results[3].Id);
        Assert.Equal(5, results[4].Id);
    }

    [Fact]
    public void CanApplyOrderBy_WithNestedParameterAlias()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

        var parser = new ODataQueryOptionParser(
            model,
            model.FindType("Microsoft.AspNetCore.OData.Tests.Models.Customer"),
            model.FindDeclaredNavigationSource("Default.Container.Customers"),
            new Dictionary<string, string> { { "$orderby", "@p1" }, { "@p2", "Name" }, { "@p1", "@p2" } });

        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("@p1", context, parser);

        var customers = (new List<Customer>{
            new Customer { Id = 1, Name = "Andy" },
            new Customer { Id = 2, Name = "Aaron" },
            new Customer { Id = 3, Name = "Alex" }
        }).AsQueryable();

        // Act
        var results = orderByOption.ApplyTo(customers).ToArray();

        // Assert
        Assert.Equal(2, results[0].Id);
        Assert.Equal(3, results[1].Id);
        Assert.Equal(1, results[2].Id);
    }

    [Theory]
    [InlineData("Orders/$count")]
    [InlineData("Addresses/$count")]
    [InlineData("Aliases/$count")]
    public void CanApplyOrderBy_WithCollectionCount(string orderby)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Order_EntityType()
                        .Add_Customer_EntityType_With_Address()
                        .Add_CustomerOrders_Relationship()
                        .Add_Customer_EntityType_With_CollectionProperties()
                        .Add_Customers_EntitySet()
                        .GetEdmModel();

        var parser = new ODataQueryOptionParser(
            model,
            model.FindType("Microsoft.AspNetCore.OData.Tests.Models.Customer"),
            model.FindDeclaredNavigationSource("Default.Container.Customers"),
            new Dictionary<string, string> { { "$orderby", orderby } });

        var orderByOption = new OrderByQueryOption(orderby, new ODataQueryContext(model, typeof(Customer)), parser);

        var customers = (new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "Andy",
                Orders = new List<Order>
                {
                    new Order { OrderId = 1 },
                    new Order { OrderId = 2 }
                },
                Addresses = new List<Address>
                {
                    new Address { City = "1" },
                    new Address { City = "2" }
                },
                Aliases = new List<string> { "1", "2" }
            },
            new Customer
            {
                Id = 2,
                Name = "Aaron",
                Orders = new List<Order>
                {
                    new Order { OrderId = 3 }
                },
                Addresses = new List<Address>
                {
                    new Address { City = "3" }
                },
                Aliases = new List<string> { "3" }
            },
            new Customer { Id = 3, Name = "Alex" }
        }).AsQueryable();

        // Act
        ODataQuerySettings settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True };
        var results = orderByOption.ApplyTo(customers, settings).ToArray();

        // Assert
        Assert.Equal(3, results[0].Id);
        Assert.Equal(2, results[1].Id);
        Assert.Equal(1, results[2].Id);
    }

    [Fact]
    public void OrderBy_Works_ParameterAlias()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        var orderByOption = new OrderByQueryOption("@p", new ODataQueryContext(model, typeof(Customer)));

        // Act & Assert
        var nodes = orderByOption.OrderByNodes;
        OrderByNode orderByNode = Assert.Single(nodes);
        OrderByClauseNode clauseNode = Assert.IsType<OrderByClauseNode>(orderByNode);
    }

    [Fact]
    public void GetOrderByRawValues_Works_OrderByClause()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Address/City asc", context);

        // Act
        List<string> rawValues = orderByOption.GetOrderByRawValues();

        // Assert
        string clause = Assert.Single(rawValues);
        Assert.Equal("Address/City asc", clause);
    }

    [Fact]
    public void GetOrderByRawValues_Works_MultipleOrderByClause()
    {
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var orderByOption = new OrderByQueryOption("Address/City asc,substring(Name,2,1),tolower(substring(Name,2,1))", context);

        // Act
        List<string> rawValues = orderByOption.GetOrderByRawValues();

        // Assert
        Assert.Equal(3, rawValues.Count);
        Assert.Equal("Address/City", rawValues[0]); // Here, without 'asc' since it's omitted by default
        Assert.Equal("substring(Name,2,1)", rawValues[1]);
        Assert.Equal("tolower(substring(Name,2,1))", rawValues[2]);
    }
}
