// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Allows configuration to be performed for a navigation source (entity set, singleton) in a model.
    /// </summary>
    public abstract class NavigationSourceConfiguration
    {
        private string _url;
        private readonly ODataModelBuilder _modelBuilder;

        private readonly
            Dictionary<NavigationPropertyConfiguration, Dictionary<string, NavigationPropertyBindingConfiguration>>
            _navigationPropertyBindings = new Dictionary<NavigationPropertyConfiguration, Dictionary<string, NavigationPropertyBindingConfiguration>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSourceConfiguration"/> class.
        /// The default constructor is intended for use by unit testing only.
        /// </summary>
        protected NavigationSourceConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSourceConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityClrType">The <see cref="Type"/> of the entity type contained in this navigation source.</param>
        /// <param name="name">The name of the navigation source.</param>
        protected NavigationSourceConfiguration(ODataModelBuilder modelBuilder, Type entityClrType, string name)
            : this(modelBuilder, new EntityTypeConfiguration(modelBuilder, entityClrType), name)
        {
            _url = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSourceConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityType">The entity type <see cref="EntityTypeConfiguration"/> contained in this navigation source.</param>
        /// <param name="name">The name of the navigation source.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The property being set, ClrType, is on a different object.")]
        protected NavigationSourceConfiguration(ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (String.IsNullOrEmpty(name))
            {
                throw Error.ArgumentNullOrEmpty("name");
            }

            _modelBuilder = modelBuilder;
            Name = name;
            EntityType = entityType;
            ClrType = entityType.ClrType;
        }

        /// <summary>
        /// Gets the navigation targets of <see cref=" NavigationSourceConfiguration"/>.
        /// </summary>
        public IEnumerable<NavigationPropertyBindingConfiguration> Bindings
        {
            get { return _navigationPropertyBindings.Values.SelectMany(e => e.Values); }
        }

        /// <summary>
        /// Gets the entity type contained in this navigation source.
        /// </summary>
        public virtual EntityTypeConfiguration EntityType { get; private set; }

        /// <summary>
        /// Gets the backing <see cref="Type"/> for the entity type contained in this navigation source.
        /// </summary>
        public Type ClrType { get; private set; }

        /// <summary>
        /// Gets the name of this navigation source.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the navigation source URL.
        /// </summary>
        /// <returns>The navigation source URL.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This Url property is not required to be a valid Uri")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Consistent with EF Has/Get pattern")]
        public virtual string GetUrl()
        {
            return _url;
        }

        /// <summary>
        /// Binds the given navigation property to the target navigation source.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="targetNavigationSource">The target navigation source.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual NavigationPropertyBindingConfiguration AddBinding(NavigationPropertyConfiguration navigationConfiguration,
            NavigationSourceConfiguration targetNavigationSource)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            if (targetNavigationSource == null)
            {
                throw Error.ArgumentNull("targetNavigationSource");
            }

            IList<MemberInfo> bindingPath = new List<MemberInfo> { navigationConfiguration.PropertyInfo };
            if (navigationConfiguration.DeclaringType != EntityType)
            {
                bindingPath.Insert(0, TypeHelper.AsMemberInfo(navigationConfiguration.DeclaringType.ClrType));
            }

            return AddBinding(navigationConfiguration, targetNavigationSource, bindingPath);
        }

        /// <summary>
        /// Binds the given navigation property to the target navigation source.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="targetNavigationSource">The target navigation source.</param>
        /// <param name="bindingPath">The binding path.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual NavigationPropertyBindingConfiguration AddBinding(NavigationPropertyConfiguration navigationConfiguration,
            NavigationSourceConfiguration targetNavigationSource, IList<MemberInfo> bindingPath)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            if (targetNavigationSource == null)
            {
                throw Error.ArgumentNull("targetNavigationSource");
            }

            if (bindingPath == null || !bindingPath.Any())
            {
                throw Error.ArgumentNull("bindingPath");
            }

            VerifyBindingPath(navigationConfiguration, bindingPath);

            string path = bindingPath.ConvertBindingPath();

            Dictionary<string, NavigationPropertyBindingConfiguration> navigationPropertyBindingMap;
            NavigationPropertyBindingConfiguration navigationPropertyBinding;
            if (_navigationPropertyBindings.TryGetValue(navigationConfiguration, out navigationPropertyBindingMap))
            {
                if (navigationPropertyBindingMap.TryGetValue(path, out navigationPropertyBinding))
                {
                    if (navigationPropertyBinding.TargetNavigationSource != targetNavigationSource)
                    {
                        throw Error.NotSupported(SRResources.RebindingNotSupported);
                    }
                }
                else
                {
                    navigationPropertyBinding = new NavigationPropertyBindingConfiguration(navigationConfiguration,
                        targetNavigationSource, bindingPath);
                    _navigationPropertyBindings[navigationConfiguration][path] = navigationPropertyBinding;
                }
            }
            else
            {
                _navigationPropertyBindings[navigationConfiguration] =
                    new Dictionary<string, NavigationPropertyBindingConfiguration>();
                navigationPropertyBinding = new NavigationPropertyBindingConfiguration(navigationConfiguration,
                    targetNavigationSource, bindingPath);
                _navigationPropertyBindings[navigationConfiguration][path] = navigationPropertyBinding;
            }

            return navigationPropertyBinding;
        }

        /// <summary>
        /// Removes the bindings for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property</param>
        public virtual void RemoveBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            _navigationPropertyBindings.Remove(navigationConfiguration);
        }

        /// <summary>
        /// Removes the binding for the given navigation property and the given binding path.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="bindingPath">The binding path.</param>
        public virtual void RemoveBinding(NavigationPropertyConfiguration navigationConfiguration, string bindingPath)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            Dictionary<string, NavigationPropertyBindingConfiguration> navigationPropertyBindingMap;
            if (_navigationPropertyBindings.TryGetValue(navigationConfiguration, out navigationPropertyBindingMap))
            {
                navigationPropertyBindingMap.Remove(bindingPath);

                if (!navigationPropertyBindingMap.Any())
                {
                    _navigationPropertyBindings.Remove(navigationConfiguration);
                }
            }
        }

        /// <summary>
        /// Finds the bindings <see cref="NavigationPropertyBindingConfiguration"/> for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <returns>The list of <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual IEnumerable<NavigationPropertyBindingConfiguration> FindBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            Dictionary<string, NavigationPropertyBindingConfiguration> navigationPropertyBindings;
            if (_navigationPropertyBindings.TryGetValue(navigationConfiguration, out navigationPropertyBindings))
            {
                return navigationPropertyBindings.Values;
            }

            return null;
        }

        /// <summary>
        /// Finds the binding for the given navigation property and tries to create it if it does not exist.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="bindingPath">The binding path.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration,
            IList<MemberInfo> bindingPath)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            if (bindingPath == null)
            {
                throw Error.ArgumentNullOrEmpty("bindingPath");
            }

            string path = bindingPath.ConvertBindingPath();

            Dictionary<string, NavigationPropertyBindingConfiguration> navigationPropertyBindings;
            if (_navigationPropertyBindings.TryGetValue(navigationConfiguration, out navigationPropertyBindings))
            {
                NavigationPropertyBindingConfiguration bindingConfiguration;
                if (navigationPropertyBindings.TryGetValue(path, out bindingConfiguration))
                {
                    return bindingConfiguration;
                }
            }

            if (_modelBuilder.BindingOptions == NavigationPropertyBindingOption.None)
            {
                return null;
            }

            bool hasSingletonAttribute = navigationConfiguration.PropertyInfo.GetCustomAttributes<SingletonAttribute>().Any();
            Type entityType = navigationConfiguration.RelatedClrType;

            NavigationSourceConfiguration[] matchedNavigationSources;
            if (hasSingletonAttribute)
            {
                matchedNavigationSources = _modelBuilder.Singletons.Where(es => es.EntityType.ClrType == entityType).ToArray();
            }
            else
            {
                matchedNavigationSources = _modelBuilder.EntitySets.Where(es => es.EntityType.ClrType == entityType).ToArray();
            }

            if (matchedNavigationSources.Length >= 1)
            {
                if (matchedNavigationSources.Length == 1 ||
                    _modelBuilder.BindingOptions == NavigationPropertyBindingOption.Auto)
                {
                    return AddBinding(navigationConfiguration, matchedNavigationSources[0], bindingPath);
                }

                throw Error.NotSupported(
                        SRResources.CannotAutoCreateMultipleCandidates,
                        path,
                        navigationConfiguration.DeclaringType.FullName,
                        Name,
                        String.Join(", ", matchedNavigationSources.Select(s => s.Name)));
            }

            return null;
        }

        /// <summary>
        /// Gets the bindings <see cref="NavigationPropertyBindingConfiguration"/> for the navigation property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <returns>The bindings <see cref="NavigationPropertyBindingConfiguration" />.</returns>
        public virtual IEnumerable<NavigationPropertyBindingConfiguration> FindBindings(string propertyName)
        {
            foreach (var navigationPropertyBinding in _navigationPropertyBindings)
            {
                if (navigationPropertyBinding.Key.Name == propertyName)
                {
                    return navigationPropertyBinding.Value.Values;
                }
            }

            return Enumerable.Empty<NavigationPropertyBindingConfiguration>();
        }

        private void VerifyBindingPath(NavigationPropertyConfiguration navigationConfiguration, IList<MemberInfo> bindingPath)
        {
            Contract.Assert(navigationConfiguration != null);
            Contract.Assert(bindingPath != null);

            PropertyInfo navigation = bindingPath.Last() as PropertyInfo;
            if (navigation == null || navigation != navigationConfiguration.PropertyInfo)
            {
                throw Error.Argument("navigationConfiguration", SRResources.NavigationPropertyBindingPathIsNotValid,
                    bindingPath.ConvertBindingPath(), navigationConfiguration.Name);
            }

            bindingPath.Aggregate(EntityType.ClrType, VerifyBindingSegment);
        }

        private static Type VerifyBindingSegment(Type current, MemberInfo info)
        {
            Contract.Assert(current != null);
            Contract.Assert(info != null);

            TypeInfo derivedType = info as TypeInfo;
            if (derivedType != null)
            {
                if (!(derivedType.IsAssignableFrom(current) || current.IsAssignableFrom(derivedType.BaseType)))
                {
                    throw Error.InvalidOperation(SRResources.NavigationPropertyBindingPathNotInHierarchy,
                        derivedType.FullName, info.Name, current.FullName);
                }

                return derivedType.BaseType;
            }

            PropertyInfo propertyInfo = info as PropertyInfo;
            if (propertyInfo == null)
            {
                throw Error.NotSupported(SRResources.NavigationPropertyBindingPathNotSupported, info.Name, info.MemberType);
            }

            Type declaringType = propertyInfo.DeclaringType;
            if (declaringType == null ||
                !(declaringType.IsAssignableFrom(current) || current.IsAssignableFrom(declaringType)))
            {
                throw Error.InvalidOperation(SRResources.NavigationPropertyBindingPathNotInHierarchy,
                    declaringType == null ? "Unknown Type" : declaringType.FullName, info.Name, current.FullName);
            }

            Type elementType;
            if (TypeHelper.IsCollection(propertyInfo.PropertyType, out elementType))
            {
                return elementType;
            }

            return propertyInfo.PropertyType;
        }
    }
}
