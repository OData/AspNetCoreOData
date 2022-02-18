//-----------------------------------------------------------------------------
// <copyright file="MockNavigationSourceLinkBuilderAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    internal class MockNavigationSourceLinkBuilderAnnotation : NavigationSourceLinkBuilderAnnotation
    {
        public Func<ResourceContext, IEdmNavigationProperty, ODataMetadataLevel, Uri> NavigationLinkBuilder { get; set; }

        public override Uri BuildEditLink(ResourceContext instanceContext, ODataMetadataLevel metadataLevel, Uri idLink)
        {
            if (EditLinkBuilder != null)
            {
                return EditLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildIdLink(ResourceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            if (IdLinkBuilder != null)
            {
                return IdLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildReadLink(ResourceContext instanceContext, ODataMetadataLevel metadataLevel, Uri editLink)
        {
            if (ReadLinkBuilder != null)
            {
                return ReadLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildNavigationLink(ResourceContext context, IEdmNavigationProperty navigationProperty, ODataMetadataLevel metadataLevel)
        {
            if (NavigationLinkBuilder != null)
            {
                return NavigationLinkBuilder(context, navigationProperty, metadataLevel);
            }

            return null;
        }
    }
}
