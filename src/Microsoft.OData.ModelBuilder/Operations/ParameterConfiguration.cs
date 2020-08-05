// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Represents a parameter to an Operation
    /// </summary>
    public abstract class ParameterConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfiguration"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterType">The EDM type of the parameter.</param>
        protected ParameterConfiguration(string name, IEdmTypeConfiguration parameterType)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            TypeConfiguration = parameterType ?? throw new ArgumentNullException(nameof(parameterType));

            Type elementType;
            Nullable = TypeHelper.IsCollection(parameterType.ClrType, out elementType)
                ? TypeHelper.IsNullable(elementType)
                : TypeHelper.IsNullable(parameterType.ClrType);
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The type of the parameter
        /// </summary>
        public IEdmTypeConfiguration TypeConfiguration { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter is nullable or not.
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter is optional or not.
        /// </summary>
        public bool IsOptional { get; protected set; }

        /// <summary>
        /// Gets or sets a default value for optional parameter.
        /// </summary>
        public string DefaultValue { get; protected set; }

        /// <summary>
        /// Sets the optional value as true.
        /// </summary>
        public ParameterConfiguration Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the optional value as false.
        /// </summary>
        public ParameterConfiguration Required()
        {
            IsOptional = false;
            return this;
        }

        /// <summary>
        /// Sets the optional value as true, default value as given value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public ParameterConfiguration HasDefaultValue(string defaultValue)
        {
            IsOptional = true;
            DefaultValue = defaultValue;
            return this;
        }
    }
}
