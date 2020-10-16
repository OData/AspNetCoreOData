// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ODataPathTemplateExtensionsTests
    {
        private static IEdmTypeReference IntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

        [Fact]
        public void GetTemplatesWorksForBasicPath()
        {
            // Arrange
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet("Customers", customer);

            ODataPathTemplate template = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                KeySegmentTemplate.CreateKeySegment(customer, entitySet));

            // Act
            IEnumerable<string> actual = template.GetTemplates();

            // Assert
            Assert.Equal(2, actual.Count());
            Assert.Equal(new[] { "Customers({key})", "Customers/{key}" }, actual);
        }

        [Fact]
        public void GetTemplatesWorksForODataPathWithDollarRefOnSingleNavigation()
        {
            // Arrange
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet("Customers", customer);
            var navigation = customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                TargetMultiplicity = EdmMultiplicity.One,
                Name = "SubCustomer",
                Target = customer
            });

            ODataPathTemplate template = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                KeySegmentTemplate.CreateKeySegment(customer, entitySet),
                new NavigationLinkSegmentTemplate(navigation, entitySet));

            // Act
            IEnumerable<string> actual = template.GetTemplates();

            // Assert
            Assert.Equal(2, actual.Count());
            Assert.Equal(new[]
                {
                    "Customers({key})/SubCustomer/$ref",
                    "Customers/{key}/SubCustomer/$ref"
                }, actual);
        }

        [Fact]
        public void GetTemplatesWorksForODataPathWithDollarRefOnCollectionNavigation()
        {
            // Arrange
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet("Customers", customer);
            var navigation = customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                TargetMultiplicity = EdmMultiplicity.Many,
                Name = "SubCustomers",
                Target = customer
            });

            KeyValuePair<string, object>[] keys = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("Id", "{nextKey}")
            };
            KeySegment keySegment = new KeySegment(keys, customer, entitySet);
            ODataPathTemplate template = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                KeySegmentTemplate.CreateKeySegment(customer, entitySet),
                new NavigationLinkSegmentTemplate(navigation, entitySet),
                new KeySegmentTemplate(keySegment));

            // Act
            IEnumerable<string> actual = template.GetTemplates();

            // Assert
            Assert.Equal(4, actual.Count());
            Assert.Equal(new[]
                {
                    "Customers({key})/SubCustomers({nextKey})/$ref",
                    "Customers({key})/SubCustomers/{nextKey}/$ref",
                    "Customers/{key}/SubCustomers({nextKey})/$ref",
                    "Customers/{key}/SubCustomers/{nextKey}/$ref"
                }, actual);
        }

        [Fact]
        public void GetTemplatesWorksForPathWithTypeCastAndFunction()
        {
            // Arrange
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            // VipCustomer
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet("Customers", customer);

            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmEntityTypeReference(vipCustomer, false));
            getSalaray.AddParameter("salary", IntType);
            getSalaray.AddOptionalParameter("minSalary", IntType);
            getSalaray.AddOptionalParameter("maxSalary", IntType, "129");

            ODataPathTemplate template = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                KeySegmentTemplate.CreateKeySegment(customer, entitySet),
                new CastSegmentTemplate(vipCustomer, customer, entitySet),
                new FunctionSegmentTemplate(getSalaray, null));

            // Act
            IEnumerable<string> actual = template.GetTemplates();

            Assert.Equal(4, actual.Count());
            Assert.Equal(new[]
            {
                "Customers({key})/NS.VipCustomer/NS.GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
                "Customers({key})/NS.VipCustomer/GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
                "Customers/{key}/NS.VipCustomer/NS.GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
                "Customers/{key}/NS.VipCustomer/GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
            }, actual);
        }


        #region GenerateFunctionTemplates

        public static TheoryDataSet<bool, string[]> FunctionWithoutParametersData
        {
            get
            {
                return new TheoryDataSet<bool, string[]>()
                {
                    { true, new[]
                        {
                            "NS.GetWholeSalary()",
                            "GetWholeSalary()"
                        }
                    },
                    { false, new[] { "GetWholeSalary()" } },
                };
            }
        }

        [Theory]
        [MemberData(nameof(FunctionWithoutParametersData))]
        public void GenerateFunctionTemplatesWorksForEdmFunctionWithoutParameters(bool bound, string[] expects)
        {
            // Arrange
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: bound, entitySetPathExpression: null, isComposable: false);
            if (bound)
            {
                EdmEntityType customer = new EdmEntityType("NS", "Customer");
                getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            }

            // Act
            IList<string> items = getSalaray.GenerateFunctionTemplates();

            // Assert
            Assert.Equal(expects.Length, items.Count);
            Assert.Equal(expects, items);
        }

        public static TheoryDataSet<bool, string[]> FunctionWithoutOptionalParametersData
        {
            get
            {
                return new TheoryDataSet<bool, string[]>()
                {
                    { true, new[]
                        {
                            "NS.GetWholeSalary(minSalary={minSalary},maxSalary={maxSalary})",
                            "GetWholeSalary(minSalary={minSalary},maxSalary={maxSalary})"
                        }
                    },
                    { false, new[] { "GetWholeSalary(minSalary={minSalary},maxSalary={maxSalary})" } },
                };
            }
        }

        [Theory]
        [MemberData(nameof(FunctionWithoutOptionalParametersData))]
        public void GenerateFunctionTemplatesWorksForEdmFunctionWithoutOptionalParameters(bool bound, string[] expects)
        {
            // Arrange
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: bound, entitySetPathExpression: null, isComposable: false);
            if (bound)
            {
                EdmEntityType customer = new EdmEntityType("NS", "Customer");
                customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
                getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            }

            getSalaray.AddParameter("minSalary", IntType);
            getSalaray.AddParameter("maxSalary", IntType);

            // Act
            IList<string> items = getSalaray.GenerateFunctionTemplates();

            // Assert
            Assert.Equal(expects.Length, items.Count);
            Assert.Equal(expects, items);
        }

        public static TheoryDataSet<bool, string[]> FunctionWithOptionalParametersData
        {
            get
            {
                return new TheoryDataSet<bool, string[]>()
                {
                    { true, new[]
                        {
                            "NS.GetWholeSalary(salary={salary})",
                            "NS.GetWholeSalary(salary={salary},aveSalary={aveSalary})",
                            "NS.GetWholeSalary(salary={salary},maxSalary={maxSalary})",
                            "NS.GetWholeSalary(salary={salary},maxSalary={maxSalary},aveSalary={aveSalary})",
                            "NS.GetWholeSalary(salary={salary},minSalary={minSalary})",
                            "NS.GetWholeSalary(salary={salary},minSalary={minSalary},aveSalary={aveSalary})",
                            "NS.GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
                            "NS.GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary},aveSalary={aveSalary})",

                            "GetWholeSalary(salary={salary})",
                            "GetWholeSalary(salary={salary},aveSalary={aveSalary})",
                            "GetWholeSalary(salary={salary},maxSalary={maxSalary})",
                            "GetWholeSalary(salary={salary},maxSalary={maxSalary},aveSalary={aveSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary},aveSalary={aveSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary},aveSalary={aveSalary})",
                        }
                    },
                    { false, new[]
                        {
                            "GetWholeSalary(salary={salary})",
                            "GetWholeSalary(salary={salary},aveSalary={aveSalary})",
                            "GetWholeSalary(salary={salary},maxSalary={maxSalary})",
                            "GetWholeSalary(salary={salary},maxSalary={maxSalary},aveSalary={aveSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary},aveSalary={aveSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary})",
                            "GetWholeSalary(salary={salary},minSalary={minSalary},maxSalary={maxSalary},aveSalary={aveSalary})",
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(FunctionWithOptionalParametersData))]
        public void GenerateFunctionTemplatesWorksForEdmFunctionWithOptionalParameters(bool bound, string[] expects)
        {
            // Arrange
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: bound, entitySetPathExpression: null, isComposable: false);
            if (bound)
            {
                EdmEntityType customer = new EdmEntityType("NS", "Customer");
                getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            }

            getSalaray.AddParameter("salary", IntType);
            getSalaray.AddOptionalParameter("minSalary", IntType);
            getSalaray.AddOptionalParameter("maxSalary", IntType);
            getSalaray.AddOptionalParameter("aveSalary", IntType, "129");

            // Act
            var items = getSalaray.GenerateFunctionTemplates();

            // Assert
            Assert.Equal(expects.Length, items.Count);
            Assert.Equal(expects, items);
        }
        #endregion
    }
}
