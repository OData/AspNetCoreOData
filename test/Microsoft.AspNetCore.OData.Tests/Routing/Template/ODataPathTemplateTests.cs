// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ODataPathTemplateTests
    {
        private static IEdmTypeReference IntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

        [Fact]
        public void CtorODataPathTemplate_SetsSegments_UsingEnumerable()
        {
            // Arrange
            ODataSegmentTemplate template = new Mock<ODataSegmentTemplate>().Object;
            IEnumerable<ODataSegmentTemplate> templates = new ODataSegmentTemplate[]
            {
                template
            };

            // Act
            ODataPathTemplate path = new ODataPathTemplate(templates);

            // Assert
            ODataSegmentTemplate actual = Assert.Single(path);
            Assert.Same(template, actual);
        }

        [Fact]
        public void CtorODataPathTemplate_SetsSegments_UsingList()
        {
            // Arrange
            ODataSegmentTemplate template = new Mock<ODataSegmentTemplate>().Object;
            IList<ODataSegmentTemplate> templates = new List<ODataSegmentTemplate>
            {
                template
            };

            // Act
            ODataPathTemplate path = new ODataPathTemplate(templates);

            // Assert
            ODataSegmentTemplate actual = Assert.Single(path);
            Assert.Same(template, actual);
        }

        [Fact]
        public void GetTemplatesReturnsCorrectWithEmptySegments()
        {
            // Arrange
            ODataPathTemplate path = new ODataPathTemplate();

            // Act
            IEnumerable<string> templates = path.GetTemplates();

            // Assert
            var template = Assert.Single(templates);
            Assert.Equal("", template);
        }

        [Fact]
        public void GetTemplatesReturnsCorrectWithMetadataSegment()
        {
            // Arrange
            ODataPathTemplate path = new ODataPathTemplate(MetadataSegmentTemplate.Instance);

            // Act
            IEnumerable<string> templates = path.GetTemplates();

            // Assert
            var template = Assert.Single(templates);
            Assert.Equal("$metadata", template);
        }

        [Fact]
        public void GetTemplatesReturnsCorrectWithTwoSegments()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entitySet = new EdmEntitySet(container, "set", entityType);
            EdmAction action = new EdmAction("NS", "action", null, true, null);
            ODataPathTemplate path = new ODataPathTemplate(new EntitySetSegmentTemplate(entitySet),
                new ActionSegmentTemplate(action, null));

            // Act
            IEnumerable<string> templates = path.GetTemplates();

            // Act & Assert
            Assert.Collection(templates,
                e =>
                {
                    Assert.Equal("set/NS.action", e);
                },
                e =>
                {
                    Assert.Equal("set/action", e);
                });
        }

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
                new NavigationLinkSegmentTemplate(navigation, entitySet)
                {
                    Key = new KeySegmentTemplate(keySegment)
                });

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
        public void GetTemplatesWorks_ForEdmFunctionWithoutParameters(bool bound, string[] expects)
        {
            // Arrange
            ODataPathTemplate template;
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: bound, entitySetPathExpression: null, isComposable: false);
            if (bound)
            {
                EdmEntityType customer = new EdmEntityType("NS", "Customer");
                getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
                template = new ODataPathTemplate(new FunctionSegmentTemplate(getSalaray, null));
            }
            else
            {
                EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
                template = new ODataPathTemplate(new FunctionImportSegmentTemplate(new EdmFunctionImport(container, "GetWholeSalary", getSalaray), null));
            }

            // Act
            IEnumerable<string> items = template.GetTemplates();

            // Assert
            Assert.Equal(expects.Length, items.Count());
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
        public void GetTemplatesWorks_ForEdmFunctionWithoutOptionalParameters(bool bound, string[] expects)
        {
            // Arrange
            ODataPathTemplate template;
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: bound, entitySetPathExpression: null, isComposable: false);
            if (bound)
            {
                EdmEntityType customer = new EdmEntityType("NS", "Customer");
                customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
                getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            }

            getSalaray.AddParameter("minSalary", IntType);
            getSalaray.AddParameter("maxSalary", IntType);

            if (bound)
            {
                template = new ODataPathTemplate(new FunctionSegmentTemplate(getSalaray, null));
            }
            else
            {
                EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
                template = new ODataPathTemplate(new FunctionImportSegmentTemplate(new EdmFunctionImport(container, "GetWholeSalary", getSalaray), null));
            }

            // Act
            IEnumerable<string> items = template.GetTemplates();

            // Assert
            Assert.Equal(expects.Length, items.Count());
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
                            "NS.GetWholeSalary(salary={salary},minSalary={min},maxSalary={max})",
                            "GetWholeSalary(salary={salary},minSalary={min},maxSalary={max})"
                        }
                    },
                    { false, new[]
                        {
                            "GetWholeSalary(salary={salary},minSalary={min},maxSalary={max})"
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

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "salary", "{salary}" },
                { "minSalary", "{min}" },
                { "maxSalary", "{max}" }, // without "aveSalary"
            };

            ODataPathTemplate template;
            if (bound)
            {
                template = new ODataPathTemplate(new FunctionSegmentTemplate(parameters, getSalaray, null));
            }
            else
            {
                EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
                template = new ODataPathTemplate(new FunctionImportSegmentTemplate(parameters, new EdmFunctionImport(container, "GetWholeSalary", getSalaray), null));
            }

            // Act
            IEnumerable<string> items = template.GetTemplates();

            // Assert
            Assert.Equal(expects.Length, items.Count());
            Assert.Equal(expects, items);
        }
        #endregion
    }
}
