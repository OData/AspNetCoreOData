// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Results
{
    internal static partial class ResultHelpers
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
                response.Headers.Add(EntityIdHeaderName, entityId().ToString());
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

            //NavigationSourceLinkBuilderAnnotation linkBuilder =
            //    resourceContext.EdmModel.GetNavigationSourceLinkBuilder(resourceContext.NavigationSource);
            //Contract.Assert(linkBuilder != null);

            //Uri idLink = linkBuilder.BuildIdLink(resourceContext);
            //if (isEntityId)
            //{
            //    if (idLink == null)
            //    {
            //        throw Error.InvalidOperation(
            //            SRResources.IdLinkNullForEntityIdHeader,
            //            resourceContext.NavigationSource.Name);
            //    }

            //    return idLink;
            //}

            //Uri editLink = linkBuilder.BuildEditLink(resourceContext);
            //if (editLink == null)
            //{
            //    if (idLink != null)
            //    {
            //        return idLink;
            //    }

            //    throw Error.InvalidOperation(
            //        SRResources.EditLinkNullForLocationHeader,
            //        resourceContext.NavigationSource.Name);
            //}

            //return editLink;
            return null;
        }

        private static Uri GenerateContainmentODataPathSegments(ResourceContext resourceContext, bool isEntityId)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(
                resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet);
            Contract.Assert(resourceContext.Request != null);

            //ODataPath path = resourceContext.Context.Path;
            //if (path == null)
            //{
            //    throw Error.InvalidOperation(SRResources.ODataPathMissing);
            //}

            //path = new ContainmentPathBuilder().TryComputeCanonicalContainingPath(path);

            //List<ODataPathSegment> odataPath = path.Segments.ToList();

            //// create a template entity set if it's contained entity set
            //IEdmEntitySet entitySet = resourceContext.NavigationSource as IEdmEntitySet;
            //if (entitySet == null)
            //{
            //    EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            //    entitySet = new EdmEntitySet(container, resourceContext.NavigationSource.Name, resourceContext.NavigationSource.EntityType());
            //}

            //odataPath.Add(new EntitySetSegment(entitySet));
            //odataPath.Add(new KeySegment(ConventionsHelpers.GetEntityKey(resourceContext),
            //    resourceContext.StructuredType as IEdmEntityType, resourceContext.NavigationSource));

            //if (!isEntityId)
            //{
            //    bool isSameType = resourceContext.StructuredType == resourceContext.NavigationSource.EntityType();
            //    if (!isSameType)
            //    {
            //        odataPath.Add(new TypeSegment(resourceContext.StructuredType, resourceContext.NavigationSource));
            //    }
            //}

            //string odataLink = resourceContext.CreateODataLink(odataPath);
            //return odataLink == null ? null : new Uri(odataLink);

            return null;
        }

        internal static ODataVersion GetODataResponseVersion(HttpRequest request)
        {
            Contract.Assert(request != null, "GetODataResponseVersion called with a null request");
            return request.ODataMaxServiceVersion() ??
                request.ODataMinServiceVersion() ??
                request.ODataServiceVersion() ??
                ODataVersionConstraint.DefaultODataVersion;
        }

        private static IEdmEntityTypeReference GetEntityType(IEdmModel model, object entity)
        {
            Type entityType = entity.GetType();
            IEdmTypeReference edmType = model.GetTypeMappingCache().GetEdmType(entityType, model);
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
