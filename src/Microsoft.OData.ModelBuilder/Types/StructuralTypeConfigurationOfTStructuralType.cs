// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public abstract class StructuralTypeConfiguration<TStructuralType> where TStructuralType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralTypeConfiguration{TStructuralType}"/> class.
        /// </summary>
        /// <param name="configuration">The inner configuration of the structural type.</param>
        protected StructuralTypeConfiguration(StructuralTypeConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets the collection of EDM structural properties that belong to this type.
        /// </summary>
        public IEnumerable<PropertyConfiguration> Properties => Configuration.Properties;

        /// <summary>
        /// Gets the full name of this EDM type.
        /// </summary>
        public string FullName => Configuration.FullName;

        /// <summary>
        /// Gets and sets the namespace of this EDM type.
        /// </summary>
        public string Namespace
        {
            get => Configuration.Namespace;
            set => Configuration.Namespace = value;
        }

        /// <summary>
        /// Gets and sets the name of this EDM type.
        /// </summary>
        public string Name
        {
            get => Configuration.Name;
            set => Configuration.Name = value;
        }

        /// <summary>
        /// Gets an indicator whether this EDM type is an open type or not.
        /// Returns <c>true</c> if this is an open type; <c>false</c> otherwise.
        /// </summary>
        public bool IsOpen => Configuration.IsOpen;

        internal StructuralTypeConfiguration Configuration { get; }

        /// <summary>
        /// Excludes a property from the type.
        /// </summary>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <remarks>This method is used to exclude properties from the type that would have been added by convention during model discovery.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        public virtual void Ignore<TProperty>(Expression<Func<TStructuralType, TProperty>> propertyExpression)
        {
            PropertyInfo ignoredProperty = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            Configuration.RemoveProperty(ignoredProperty);
        }

        /// <summary>
        /// Adds a string property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public LengthPropertyConfiguration Property(Expression<Func<TStructuralType, string>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true) as LengthPropertyConfiguration;
        }

        /// <summary>
        /// Adds a binary property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public LengthPropertyConfiguration Property(Expression<Func<TStructuralType, byte[]>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true) as LengthPropertyConfiguration;
        }

        /// <summary>
        /// Adds a stream property the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, Stream>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true);
        }

        /// <summary>
        /// Adds an deciaml primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public DecimalPropertyConfiguration Property(Expression<Func<TStructuralType, decimal?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true) as DecimalPropertyConfiguration;
        }

        /// <summary>
        /// Adds an deciaml primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public DecimalPropertyConfiguration Property(Expression<Func<TStructuralType, decimal>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: false) as DecimalPropertyConfiguration;
        }

        /// <summary>
        /// Adds an time-of-day primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeOfDay?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an time-of-day primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeOfDay>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: false) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an duration primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeSpan?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an duration primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeSpan>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: false) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an datetime-with-offset primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, DateTimeOffset?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an datetime-with-offset primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, DateTimeOffset>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: false) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an optional primitive property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The primitive property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T?>> propertyExpression) where T : struct
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: true);
        }

        /// <summary>
        /// Adds a required primitive property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The primitive property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T>> propertyExpression) where T : struct
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, nullable: false);
        }

        /// <summary>
        /// Adds an optional enum property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The enum property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public EnumPropertyConfiguration EnumProperty<T>(Expression<Func<TStructuralType, T?>> propertyExpression) where T : struct
        {
            return GetEnumPropertyConfiguration(propertyExpression, nullable: true);
        }

        /// <summary>
        /// Adds a required enum property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The enum property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public EnumPropertyConfiguration EnumProperty<T>(Expression<Func<TStructuralType, T>> propertyExpression) where T : struct
        {
            return GetEnumPropertyConfiguration(propertyExpression, nullable: false);
        }

        /// <summary>
        /// Adds a complex property to the EDM type.
        /// </summary>
        /// <typeparam name="TComplexType">The complex type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public ComplexPropertyConfiguration ComplexProperty<TComplexType>(Expression<Func<TStructuralType, TComplexType>> propertyExpression)
        {
            return GetComplexPropertyConfiguration(propertyExpression);
        }

        /// <summary>
        /// Adds a collection property to the EDM type.
        /// </summary>
        /// <typeparam name="TElementType">The element type of the collection.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public CollectionPropertyConfiguration CollectionProperty<TElementType>(Expression<Func<TStructuralType, IEnumerable<TElementType>>> propertyExpression)
        {
            return GetCollectionPropertyConfiguration(propertyExpression);
        }

        /// <summary>
        /// Adds a dynamic property dictionary property.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the dynamic property dictionary for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "More specific expression type is clearer")]
        public void HasDynamicProperties(Expression<Func<TStructuralType, IDictionary<string, object>>> propertyExpression)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            Configuration.AddDynamicPropertyDictionary(propertyInfo);
        }

        /// <summary>
        /// Configures a many relationship from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasMany<TTargetEntity>(Expression<Func<TStructuralType, IEnumerable<TTargetEntity>>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.Many);
        }

        /// <summary>
        /// Configures an optional relationship from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.ZeroOrOne);
        }

        /// <summary>
        /// Configures an optional relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(
                navigationPropertyExpression,
                referentialConstraintExpression,
                EdmMultiplicity.ZeroOrOne,
                null);
        }

        /// <summary>
        /// Configures an optional relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression">The partner expression for this relationship.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, IEnumerable<TStructuralType>>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(
                navigationPropertyExpression,
                referentialConstraintExpression,
                EdmMultiplicity.ZeroOrOne,
                partnerExpression);
        }

        /// <summary>
        /// Configures an optional relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression">The partner expression for this relationship.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, TStructuralType>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(
                navigationPropertyExpression,
                referentialConstraintExpression,
                EdmMultiplicity.ZeroOrOne,
                partnerExpression);
        }

        /// <summary>
        /// Configures a required relationship from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.One);
        }

        /// <summary>
        /// Configures a required relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression">The partner expression for this relationship.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, IEnumerable<TStructuralType>>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(navigationPropertyExpression, referentialConstraintExpression, EdmMultiplicity.One, partnerExpression);
        }

        /// <summary>
        /// Configures a required relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(navigationPropertyExpression, referentialConstraintExpression, EdmMultiplicity.One, null);
        }

        /// <summary>
        /// Configures a required relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression"></param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, TStructuralType>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(navigationPropertyExpression, referentialConstraintExpression, EdmMultiplicity.One, partnerExpression);
        }

        private NavigationPropertyConfiguration HasNavigationProperty<TTargetEntity>(Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression, EdmMultiplicity multiplicity, Expression partnerProperty)
            where TTargetEntity : class
        {
            NavigationPropertyConfiguration navigation =
                GetOrCreateNavigationProperty(navigationPropertyExpression, multiplicity);

            IDictionary<PropertyInfo, PropertyInfo> referentialConstraints =
                PropertyPairSelectorVisitor.GetSelectedProperty(referentialConstraintExpression);

            foreach (KeyValuePair<PropertyInfo, PropertyInfo> constraint in referentialConstraints)
            {
                navigation.HasConstraint(constraint);
            }

            if (partnerProperty != null)
            {
                var partnerPropertyInfo = PropertySelectorVisitor.GetSelectedProperty(partnerProperty);
                if (typeof(IEnumerable).IsAssignableFrom(partnerPropertyInfo.PropertyType))
                {
                    Configuration.ModelBuilder
                        .EntityType<TTargetEntity>().HasMany((Expression<Func<TTargetEntity, IEnumerable<TStructuralType>>>)partnerProperty);
                }
                else
                {
                    Configuration.ModelBuilder
                        .EntityType<TTargetEntity>().HasRequired((Expression<Func<TTargetEntity, TStructuralType>>)partnerProperty);
                }
                var prop = Configuration.ModelBuilder
                        .EntityType<TTargetEntity>()
                        .Properties
                        .First(p => p.Name == partnerPropertyInfo.Name)
                    as NavigationPropertyConfiguration;

                navigation.Partner = prop;
            }

            return navigation;
        }

        /// <summary>
        /// Configures a relationship from this structural type to a contained collection navigation property.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for
        ///  the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET
        ///  <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration ContainsMany<TTargetEntity>(
            Expression<Func<TStructuralType, IEnumerable<TTargetEntity>>> navigationPropertyExpression)
            where TTargetEntity : class
        {
            return GetOrCreateContainedNavigationProperty(navigationPropertyExpression, EdmMultiplicity.Many);
        }

        /// <summary>
        /// Configures an optional relationship from this structural type to a single contained navigation property.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for
        ///  the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET
        ///  <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration ContainsOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateContainedNavigationProperty(navigationPropertyExpression, EdmMultiplicity.ZeroOrOne);
        }

        /// <summary>
        /// Configures a required relationship from this structural type to a single contained navigation property.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for
        ///  the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET
        ///  <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        public NavigationPropertyConfiguration ContainsRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateContainedNavigationProperty(navigationPropertyExpression, EdmMultiplicity.One);
        }

        internal NavigationPropertyConfiguration GetOrCreateNavigationProperty(Expression navigationPropertyExpression, EdmMultiplicity multiplicity)
        {
            PropertyInfo navigationProperty = PropertySelectorVisitor.GetSelectedProperty(navigationPropertyExpression);
            return Configuration.AddNavigationProperty(navigationProperty, multiplicity);
        }

        internal NavigationPropertyConfiguration GetOrCreateContainedNavigationProperty(Expression navigationPropertyExpression, EdmMultiplicity multiplicity)
        {
            PropertyInfo navigationProperty = PropertySelectorVisitor.GetSelectedProperty(navigationPropertyExpression);
            return Configuration.AddContainedNavigationProperty(navigationProperty, multiplicity);
        }

        private PrimitivePropertyConfiguration GetPrimitivePropertyConfiguration(Expression propertyExpression, bool nullable)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            PrimitivePropertyConfiguration property = Configuration.AddProperty(propertyInfo);
            if (nullable)
            {
                property.IsNullable();
            }

            return property;
        }

        private EnumPropertyConfiguration GetEnumPropertyConfiguration(Expression propertyExpression, bool nullable)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            EnumPropertyConfiguration property = Configuration.AddEnumProperty(propertyInfo);
            if (nullable)
            {
                property.IsNullable();
            }

            return property;
        }

        private ComplexPropertyConfiguration GetComplexPropertyConfiguration(Expression propertyExpression, bool nullable = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            ComplexPropertyConfiguration property = Configuration.AddComplexProperty(propertyInfo);
            if (nullable)
            {
                property.IsNullable();
            }
            else
            {
                property.IsRequired();
            }

            return property;
        }

        private CollectionPropertyConfiguration GetCollectionPropertyConfiguration(Expression propertyExpression, bool nullable = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            CollectionPropertyConfiguration property;

            property = Configuration.AddCollectionProperty(propertyInfo);

            if (nullable)
            {
                property.IsNullable();
            }
            else
            {
                property.IsRequired();
            }

            return property;
        }
    }
}
