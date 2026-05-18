//-----------------------------------------------------------------------------
// <copyright file="MessageSizeModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

public class MessageSizeItem
{
    public int Id { get; set; }

    public string Payload { get; set; }
}
