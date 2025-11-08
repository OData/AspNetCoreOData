//-----------------------------------------------------------------------------
// <copyright file="ODataUriFunctions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// OData UriFunctions helper.
/// </summary>
public static class ODataUriFunctions
{
    /// <summary>
    /// This is a shortcut of adding the custom FunctionSignature through 'CustomUriFunctions' class and
    /// binding the function name to it's MethodInfo through 'UriFunctionsBinder' class.
    /// See these classes documentations.
    /// In case of an exception, both operations(adding the signature and binding the function) will be undone.
    /// </summary>
    /// <param name="model">The <see cref="IEdmModel"/> to which the function signature will be added.</param>
    /// <param name="functionName">The uri function name that appears in the OData request uri.</param>
    /// <param name="functionSignature">The new custom function signature.</param>
    /// <param name="methodInfo">The MethodInfo to bind the given function name.</param>
    /// <exception cref="Exception">Any exception thrown by 'CustomUriFunctions.AddCustomUriFunction' and 'UriFunctionBinder.BindUriFunctionName' methods.</exception>
    public static void AddCustomUriFunction(this IEdmModel model, string functionName,
        FunctionSignatureWithReturnType functionSignature, MethodInfo methodInfo)
    {
        try
        {
            // Add to OData.Libs function signature
            model.AddCustomUriFunction(functionName, functionSignature);

            // Bind the method to it's MethoInfo 
            UriFunctionsBinder.BindUriFunctionName(functionName, methodInfo);
        }
        catch
        {
            // Clear in case of exception
            model.RemoveCustomUriFunction(functionName, functionSignature, methodInfo);
            throw;
        }
    }

    /// <summary>
    /// This is a shortcut of removing the FunctionSignature through 'CustomUriFunctions' class and
    /// unbinding the function name from it's MethodInfo through 'UriFunctionsBinder' class.
    /// See these classes documentations.
    /// </summary>
    /// <param name="model">The <see cref="IEdmModel"/> to which the function signature will be removed.</param>
    /// <param name="functionName">The uri function name that appears in the OData request uri.</param>
    /// <param name="functionSignature">The new custom function signature.</param>
    /// <param name="methodInfo">The MethodInfo to bind the given function name.</param>
    /// <exception cref="Exception">Any exception thrown by 'CustomUriFunctions.RemoveCustomUriFunction' and 'UriFunctionsBinder.UnbindUriFunctionName' methods.</exception>
    /// <returns>'True' if the function signature has successfully removed and unbounded. 'False' otherwise.</returns>
    public static bool RemoveCustomUriFunction(this IEdmModel model, string functionName,
        FunctionSignatureWithReturnType functionSignature, MethodInfo methodInfo)
    {
        return
            model.RemoveCustomUriFunction(functionName, functionSignature) &&
            UriFunctionsBinder.UnbindUriFunctionName(functionName, methodInfo);
    }
}
