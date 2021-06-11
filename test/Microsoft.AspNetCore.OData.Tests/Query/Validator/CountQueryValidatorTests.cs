// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
    public class CountQueryValidatorTests
    {
        private readonly CountQueryValidator _validator;

        public CountQueryValidatorTests()
        {
            _validator = new CountQueryValidator();
        }

        [Fact]
        public void ValidateCountQueryValidator_Throws_NullOption()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => _validator.Validate(null, new ODataValidationSettings()), "countQueryOption");
        }

        [Fact]
        public void ValidateCountQueryValidator_Throws_NullSettings()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            CountQueryOption option = new CountQueryOption("true", context);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => _validator.Validate(option, null), "validationSettings");
        }

        [Theory]
        [InlineData("LimitedEntities(1)/Integers", "The property 'Integers' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexCollectionProperty", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty", "The property 'EntityCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/Strings", "The property 'Strings' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/SimpleEnums", "The property 'SimpleEnums' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty(1)/ComplexCollectionProperty", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/Integers/$count", "The property 'Integers' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexCollectionProperty/$count", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty/$count", "The property 'EntityCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/Strings/$count", "The property 'Strings' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/SimpleEnums/$count", "The property 'SimpleEnums' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty(1)/ComplexCollectionProperty/$count", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        public void Validate_Throws_DollarCountAppliedOnNotCountableCollection(string uri, string message)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            string serviceRoot = "http://localhost/";
            ODataUriParser pathHandler = new ODataUriParser(model, new Uri(serviceRoot), new Uri(uri, UriKind.RelativeOrAbsolute));
            ODataPath path = pathHandler.ParsePath();
            ODataQueryContext context = new ODataQueryContext(model, EdmCoreModel.Instance.GetInt32(false).Definition, path);
            CountQueryOption option = new CountQueryOption("true", context);
            ODataValidationSettings settings = new ODataValidationSettings();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => _validator.Validate(option, settings), message);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            // Configure LimitedEntity
            EntitySetConfiguration<LimitedEntity> limitedEntities = builder.EntitySet<LimitedEntity>("LimitedEntities");
            limitedEntities.EntityType.HasKey(p => p.Id);
            limitedEntities.EntityType.ComplexProperty(c => c.ComplexProperty);
            limitedEntities.EntityType.CollectionProperty(c => c.ComplexCollectionProperty).IsNotCountable();
            limitedEntities.EntityType.HasMany(l => l.EntityCollectionProperty).IsNotCountable();
            limitedEntities.EntityType.CollectionProperty(cp => cp.Integers).IsNotCountable();

            // Configure LimitedRelatedEntity
            EntitySetConfiguration<LimitedRelatedEntity> limitedRelatedEntities =
                builder.EntitySet<LimitedRelatedEntity>("LimitedRelatedEntities");
            limitedRelatedEntities.EntityType.HasKey(p => p.Id);
            limitedRelatedEntities.EntityType.CollectionProperty(p => p.ComplexCollectionProperty).IsNotCountable();

            // Configure Complextype
            ComplexTypeConfiguration<LimitedComplex> complexType = builder.ComplexType<LimitedComplex>();
            complexType.CollectionProperty(p => p.Strings).IsNotCountable();
            complexType.Property(p => p.Value);
            complexType.CollectionProperty(p => p.SimpleEnums).IsNotCountable();

            // Configure EnumType
            EnumTypeConfiguration<SimpleEnum> enumType = builder.EnumType<SimpleEnum>();
            enumType.Member(SimpleEnum.First);
            enumType.Member(SimpleEnum.Second);
            enumType.Member(SimpleEnum.Third);
            enumType.Member(SimpleEnum.Fourth);

            return builder.GetEdmModel();
        }

        private class LimitedEntity
        {
            public int Id { get; set; }
            public LimitedComplex ComplexProperty { get; set; }
            public LimitedComplex[] ComplexCollectionProperty { get; set; }
            public IEnumerable<LimitedRelatedEntity> EntityCollectionProperty { get; set; }
            public ICollection<int> Integers { get; set; }
        }

        private class LimitedComplex
        {
            public int Value { get; set; }
            public string[] Strings { get; set; }
            public IList<SimpleEnum> SimpleEnums { get; set; }
        }

        private class LimitedRelatedEntity
        {
            public int Id { get; set; }
            public LimitedComplex[] ComplexCollectionProperty { get; set; }
        }
    }
}