// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.Abstractions.Annotations
{
    /// <summary>
    /// 
    /// </summary>
    public class OperationTitleAnnotation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        public OperationTitleAnnotation(string title)
        {
            Title = title;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; }
    }
}
