// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Base class for all property configurations.
    /// </summary>
    public abstract class PropertyConfiguration
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        protected PropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
        {
            PropertyInfo = property ?? throw new ArgumentNullException(nameof(property));

            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));

            AddedExplicitly = true;
            _name = property.Name;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _name = value;
            }
        }

        /// <summary>
        /// Gets the declaring type.
        /// </summary>
        public StructuralTypeConfiguration DeclaringType { get; private set; }

        /// <summary>
        /// Gets the mapping CLR <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the CLR <see cref="Type"/> of the property.
        /// </summary>
        public abstract Type RelatedClrType { get; }

        /// <summary>
        /// Gets the <see cref="PropertyKind"/> of the property.
        /// </summary>
        public abstract PropertyKind Kind { get; }

        /// <summary>
        /// Gets or sets a value that is <c>true</c> if the property was added by the user; <c>false</c> if it was inferred through conventions.
        /// </summary>
        /// <remarks>The default value is <c>true</c></remarks>
        public bool AddedExplicitly { get; set; }

        /// <summary>
        /// Gets whether the property is restricted, i.e. not filterable, not sortable, not navigable,
        /// not expandable, not countable, or automatically expand.
        /// </summary>
        public bool IsRestricted
        {
            get { return NotFilterable || NotSortable || NotNavigable || NotExpandable || NotCountable || AutoExpand; }
        }

        /// <summary>
        /// Gets or sets whether the property is not filterable. default is false.
        /// </summary>
        public bool NotFilterable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is automatically expanded. default is false.
        /// </summary>
        public bool AutoExpand { get; set; }

        /// <summary>
        /// Gets or sets whether the automatic expand will be disabled if there is a $select specify by client.
        /// </summary>
        public bool DisableAutoExpandWhenSelectIsPresent { get; set; }

        /// <summary>
        /// Gets or sets whether the property is nonfilterable. default is false.
        /// </summary>
        public bool NonFilterable
        {
            get { return NotFilterable; }
            set { NotFilterable = value; }
        }

        /// <summary>
        /// Gets or sets whether the property is not sortable. default is false.
        /// </summary>
        public bool NotSortable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is unsortable. default is false.
        /// </summary>
        public bool Unsortable
        {
            get { return NotSortable; }
            set { NotSortable = value; }
        }

        /// <summary>
        /// Gets or sets whether the property is not navigable. default is false.
        /// </summary>
        public bool NotNavigable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not expandable. default is false.
        /// </summary>
        public bool NotExpandable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not countable. default is false.
        /// </summary>
        public bool NotCountable { get; set; }

        /// <summary>
        /// Get or sets order in "order by"  expression.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Sets the property as not filterable.
        /// </summary>
        public PropertyConfiguration IsNotFilterable()
        {
            NotFilterable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as nonfilterable.
        /// </summary>
        public PropertyConfiguration IsNonFilterable()
        {
            return IsNotFilterable();
        }

        /// <summary>
        /// Sets the property as filterable.
        /// </summary>
        public PropertyConfiguration IsFilterable()
        {
            NotFilterable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not sortable.
        /// </summary>
        public PropertyConfiguration IsNotSortable()
        {
            NotSortable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as unsortable.
        /// </summary>
        public PropertyConfiguration IsUnsortable()
        {
            return IsNotSortable();
        }

        /// <summary>
        /// Sets the property as sortable.
        /// </summary>
        public PropertyConfiguration IsSortable()
        {
            NotSortable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not navigable.
        /// </summary>
        public PropertyConfiguration IsNotNavigable()
        {
            IsNotSortable();
            IsNotFilterable();
            NotNavigable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as navigable.
        /// </summary>
        public PropertyConfiguration IsNavigable()
        {
            NotNavigable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not expandable.
        /// </summary>
        public PropertyConfiguration IsNotExpandable()
        {
            NotExpandable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as expandable.
        /// </summary>
        public PropertyConfiguration IsExpandable()
        {
            NotExpandable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not countable.
        /// </summary>
        public PropertyConfiguration IsNotCountable()
        {
            NotCountable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as countable.
        /// </summary>
        public PropertyConfiguration IsCountable()
        {
            NotCountable = false;
            return this;
        }
    }
}
