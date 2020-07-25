// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// The extension methods for the <see cref="ControllerModel"/>.
    /// </summary>
    public static class ControllerModelExtensions
    {
        /// <summary>
        /// Test whether the controller is not suitable for OData controller.
        /// </summary>
        /// <param name="controller">The given controller model.</param>
        /// <returns>True/False.</returns>
        public static bool IsNonODataController(this ControllerModel controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            return controller.Attributes.Any(a => a is NonODataControllerAttribute);
        }

        /// <summary>
        /// Test whether the controller has the specified attribute defined
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="controller">The given controller model.</param>
        /// <returns>True/False.</returns>
        public static bool HasAttribute<T>(this ControllerModel controller)
            where T : Attribute
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            return controller.Attributes.Any(a => a is T);
        }

        /// <summary>
        /// Gets the attribute from the controller model.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="controller">The given controller model.</param>
        /// <returns>The attribute or null.</returns>
        public static T GetAttribute<T>(this ControllerModel controller)
            where T : Attribute
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            return controller.Attributes.OfType<T>().FirstOrDefault();
        }
    }
}
