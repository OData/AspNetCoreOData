//-----------------------------------------------------------------------------
// <copyright file="ResultHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Results
{
    internal static class ResultHelpers
    {
        public const string EntityIdHeaderName = "OData-EntityId";

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public static Uri GenerateODataLink(HttpRequest request, object entity, bool isEntityId)
        {
            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw new InvalidOperationException(SRResources.RequestMustHaveModel);
            }

            ODataPath path = request.ODataFeature().Path;
            if (path == null)
            {
                throw new InvalidOperationException(SRResources.ODataPathMissing);
            }

            IEdmNavigationSource navigationSource = path.GetNavigationSource();
            if (navigationSource == null)
            {
                throw new InvalidOperationException(SRResources.NavigationSourceMissingDuringSerialization);
            }

            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                NavigationSource = navigationSource,
                Model = model,
                MetadataLevel = ODataMetadataLevel.Full, // Used internally to always calculate the links.
                Request = request,
                Path = path
            };

            IEdmEntityTypeReference entityType = GetEntityType(model, entity);
            ResourceContext resourceContext = new ResourceContext(serializerContext, entityType, entity);

            return GenerateODataLink(resourceContext, isEntityId);
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public static void AddEntityId(HttpResponse response, Func<Uri> entityId)
        {
            if (response.StatusCode == (int)HttpStatusCode.NoContent)
            {
                response.Headers.Add(EntityIdHeaderName, entityId().AbsoluteUri);
            }
        }

        public static void AddServiceVersion(HttpResponse response, Func<string> version)
        {
            if (response.StatusCode == (int)HttpStatusCode.NoContent)
            {
                response.Headers[ODataVersionConstraint.ODataServiceVersionHeader] = version();
            }
        }

        public static Uri GenerateODataLink(ResourceContext resourceContext, bool isEntityId)
        {
            Contract.Assert(resourceContext != null);

            // Generate location or entityId header from request Uri and key, if Post to a containment.
            // Link builder is not used, since it is also for generating ID, Edit, Read links, etc. scenarios, where
            // request Uri is not used.
            if (resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet)
            {
                return GenerateContainmentODataPathSegments(resourceContext, isEntityId);
            }

            NavigationSourceLinkBuilderAnnotation linkBuilder =
                resourceContext.EdmModel.GetNavigationSourceLinkBuilder(resourceContext.NavigationSource);
            Contract.Assert(linkBuilder != null);

            Uri idLink = linkBuilder.BuildIdLink(resourceContext);
            if (isEntityId)
            {
                if (idLink == null)
                {
                    throw Error.InvalidOperation(
                        SRResources.IdLinkNullForEntityIdHeader,
                        resourceContext.NavigationSource.Name);
                }

                return idLink;
            }

            Uri editLink = linkBuilder.BuildEditLink(resourceContext);
            if (editLink == null)
            {
                if (idLink != null)
                {
                    return idLink;
                }

                throw Error.InvalidOperation(
                    SRResources.EditLinkNullForLocationHeader,
                    resourceContext.NavigationSource.Name);
            }

            return editLink;
        }

        private static Uri GenerateContainmentODataPathSegments(ResourceContext resourceContext, bool isEntityId)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(
                resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet);
            Contract.Assert(resourceContext.Request != null);

            ODataPath path = resourceContext.Request.ODataFeature().Path;
            if (path == null)
            {
                throw Error.InvalidOperation(SRResources.ODataPathMissing);
            }

            path = new ContainmentPathBuilder().TryComputeCanonicalContainingPath(path);

            List<ODataPathSegment> odataPath = path.ToList();

            // create a template entity set if it's contained entity set
            IEdmEntitySet entitySet = resourceContext.NavigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
                entitySet = new EdmEntitySet(container, resourceContext.NavigationSource.Name, resourceContext.NavigationSource.EntityType());
            }

            odataPath.Add(new EntitySetSegment(entitySet));
            odataPath.Add(new KeySegment(ConventionsHelpers.GetEntityKey(resourceContext),
            resourceContext.StructuredType as IEdmEntityType, resourceContext.NavigationSource));

            if (!isEntityId)
            {
                bool isSameType = resourceContext.StructuredType == resourceContext.NavigationSource.EntityType();
                if (!isSameType)
                {
                    odataPath.Add(new TypeSegment(resourceContext.StructuredType, resourceContext.NavigationSource));
                }
            }

            string odataLink = resourceContext.Request.CreateODataLink(odataPath);
            return odataLink == null ? null : new Uri(odataLink);
        }


        private static IEdmEntityTypeReference GetEntityType(IEdmModel model, object entity)
        {
            Type entityType = entity.GetType();
            IEdmTypeReference edmType;
            if (entity is IEdmObject edmObject)
            {
                edmType = edmObject.GetEdmType();
            }
            else
            {
                edmType = model.GetEdmTypeReference(entityType);
            }
            if (edmType == null)
            {
                throw Error.InvalidOperation(SRResources.ResourceTypeNotInModel, entityType.FullName);
            }
            if (!edmType.IsEntity())
            {
                throw Error.InvalidOperation(SRResources.TypeMustBeEntity, edmType.FullName());
            }

            return edmType.AsEntity();
        }
    }
}
