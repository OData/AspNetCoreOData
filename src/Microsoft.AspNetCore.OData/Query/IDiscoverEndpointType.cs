using System;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// The default behavior for determining the EDM type is based on the return type of the Get method. Controllers implementing this interface can override this behavior and specify types created on-the-fly.
/// </summary>
public interface IDiscoverEndpointType
{
    /// <summary>
    /// Query the controller to determine the actual return type, as it is required for the dynamic schema of data types.
    /// </summary>
    Type GetEndpointType();
}