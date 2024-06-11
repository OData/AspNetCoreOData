//-----------------------------------------------------------------------------
// <copyright file="ODataReaderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Extension methods for <see cref="ODataReader"/>.
    /// </summary>
    public static class ODataReaderExtensions
    {
        /// <summary>
        /// Reads a <see cref="ODataResource"/> or <see cref="ODataResourceSet"/> object.
        /// </summary>
        /// <param name="reader">The OData reader to read from.</param>
        /// <returns>The read resource or resource set.</returns>
        public static ODataItemWrapper ReadResourceOrResourceSet(this ODataReader reader)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull(nameof(reader));
            }

            ODataItemWrapper topLevelItem = null;
            Stack<ODataItemWrapper> itemsStack = new Stack<ODataItemWrapper>();

            while (reader.Read())
            {
                ReadODataItem(reader, itemsStack, ref topLevelItem);
            }

            Contract.Assert(reader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level resource or resource set should have been read by now.");
            return topLevelItem;
        }

        /// <summary>
        /// Reads a <see cref="ODataResource"/> or <see cref="ODataResourceSet"/> object.
        /// </summary>
        /// <param name="reader">The OData reader to read from.</param>
        /// <returns>The read resource or resource set.</returns>
        public static async Task<ODataItemWrapper> ReadResourceOrResourceSetAsync(this ODataReader reader)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull(nameof(reader));
            }

            ODataItemWrapper topLevelItem = null;
            Stack<ODataItemWrapper> itemsStack = new Stack<ODataItemWrapper>();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                ReadODataItem(reader, itemsStack, ref topLevelItem);
            }

            Contract.Assert(reader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level resource or resource set should have been read by now.");
            return topLevelItem;
        }

        /// <summary>
        /// Read OData item.
        /// </summary>
        /// <param name="reader">The OData reader.</param>
        /// <param name="itemsStack">The item stack.</param>
        /// <param name="topLevelItem">The top level item.</param>
        private static void ReadODataItem(ODataReader reader, Stack<ODataItemWrapper> itemsStack, ref ODataItemWrapper topLevelItem)
        {
            Contract.Assert(reader != null);
            Contract.Assert(itemsStack != null);

            switch (reader.State)
            {
                case ODataReaderState.ResourceStart:
                    ReadResource(reader, itemsStack, ref topLevelItem);
                    break;

                case ODataReaderState.DeletedResourceStart:
                    ReadDeletedResource(reader, itemsStack);
                    break;

                case ODataReaderState.ResourceEnd:
                    Contract.Assert(itemsStack.Count > 0, "The resource which is ending should be on the top of the items stack.");
                    ODataResourceWrapper resourceWrapper = itemsStack.Peek() as ODataResourceWrapper;
                    if (resourceWrapper != null)
                    {
                        // Resource could be null
                        Contract.Assert(resourceWrapper.Resource == reader.Item, "The resource should be the same item in the reader.");
                    }

                    itemsStack.Pop();
                    break;

                case ODataReaderState.DeletedResourceEnd:
                    Contract.Assert(itemsStack.Count > 0, "The deleted resource which is ending should be on the top of the items stack.");
                    ODataResourceWrapper deletedResourceWrapper = itemsStack.Peek() as ODataResourceWrapper;
                    Contract.Assert(deletedResourceWrapper != null, "The top object in the stack should be delete resource wrapper.");
                    Contract.Assert(deletedResourceWrapper.Resource == reader.Item, "The deleted resource should be the same item in the reader.");
                    itemsStack.Pop();
                    break;

                case ODataReaderState.NestedResourceInfoStart:
                    ODataNestedResourceInfo nestedResourceInfo = (ODataNestedResourceInfo)reader.Item;
                    Contract.Assert(nestedResourceInfo != null, "nested resource info should never be null.");

                    ODataNestedResourceInfoWrapper nestedResourceInfoWrapper = new ODataNestedResourceInfoWrapper(nestedResourceInfo);
                    Contract.Assert(itemsStack.Count > 0, "nested resource info can't appear as top-level item.");
                    {
                        ODataResourceWrapper parentResource = (ODataResourceWrapper)itemsStack.Peek();
                        parentResource.NestedResourceInfos.Add(nestedResourceInfoWrapper);
                    }

                    itemsStack.Push(nestedResourceInfoWrapper);
                    break;

                case ODataReaderState.NestedResourceInfoEnd:
                    Contract.Assert(itemsStack.Count > 0, "The nested resource info which is ending should be on the top of the items stack.");
                    ODataNestedResourceInfoWrapper nestedInfoWrapper = itemsStack.Peek() as ODataNestedResourceInfoWrapper;
                    Contract.Assert(nestedInfoWrapper != null, "The top object in the stack should be nested resource info wrapper.");
                    Contract.Assert(nestedInfoWrapper.NestedResourceInfo == reader.Item, "The nested resource info should be the same item in the reader.");
                    itemsStack.Pop();
                    break;

                case ODataReaderState.ResourceSetStart: // resource set
                    ReadResourceSet(reader, itemsStack, ref topLevelItem);
                    break;

                case ODataReaderState.DeltaResourceSetStart: // delta resource set
                    ReadDeltaResourceSet(reader, itemsStack, ref topLevelItem);
                    break;

                case ODataReaderState.ResourceSetEnd:
                    Contract.Assert(itemsStack.Count > 0, "The resource set which is ending should be on the top of the items stack.");
                    ODataResourceSetWrapper resourceSetWrapper = itemsStack.Peek() as ODataResourceSetWrapper;
                    Contract.Assert(resourceSetWrapper != null, "The top object in the stack should be resource set wrapper.");
                    Contract.Assert(resourceSetWrapper.ResourceSet == reader.Item, "The resource set should be the same item in the reader.");
                    itemsStack.Pop();
                    break;

                case ODataReaderState.DeltaResourceSetEnd:
                    Contract.Assert(itemsStack.Count > 0, "The delta resource set which is ending should be on the top of the items stack.");
                    ODataDeltaResourceSetWrapper deltaResourceSetWrapper = itemsStack.Peek() as ODataDeltaResourceSetWrapper;
                    Contract.Assert(deltaResourceSetWrapper != null, "The top object in the stack should be delta resource set wrapper.");
                    Contract.Assert(deltaResourceSetWrapper.DeltaResourceSet == reader.Item, "The delta resource set should be the same item in the reader.");
                    itemsStack.Pop();
                    break;

                case ODataReaderState.EntityReferenceLink:
                    ODataEntityReferenceLink entityReferenceLink = (ODataEntityReferenceLink)reader.Item;
                    Contract.Assert(entityReferenceLink != null, "Entity reference link should never be null.");
                    ODataEntityReferenceLinkWrapper entityReferenceLinkWrapper = new ODataEntityReferenceLinkWrapper(entityReferenceLink);

                    Contract.Assert(itemsStack.Count > 0, "Entity reference link should never be reported as top-level item.");
                    {
                        ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                        parentNestedResource.NestedItems.Add(entityReferenceLinkWrapper);
                    }

                    break;

                case ODataReaderState.DeltaLink: // added link
                case ODataReaderState.DeltaDeletedLink: // deleted link
                    ODataDeltaLinkBaseWrapper linkBaseWrapper;
                    if (ODataReaderState.DeltaLink == reader.State)
                    {
                        ODataDeltaLink deltaLink = (ODataDeltaLink)reader.Item;
                        Contract.Assert(deltaLink != null, "Delta link should never be null.");
                        linkBaseWrapper = new ODataDeltaLinkWrapper(deltaLink);
                    }
                    else
                    {
                        ODataDeltaDeletedLink deltaDeletedLink = (ODataDeltaDeletedLink)reader.Item;
                        Contract.Assert(deltaDeletedLink != null, "Delta deleted link should never be null.");
                        linkBaseWrapper = new ODataDeltaDeletedLinkWrapper(deltaDeletedLink);
                    }

                    Contract.Assert(itemsStack.Count > 0, "Delta link should never be reported as top-level item.");
                    // Should never add a delta link to a non-delta resource set.
                    ODataDeltaResourceSetWrapper linkResourceSetWrapper = (ODataDeltaResourceSetWrapper)itemsStack.Peek();
                    Contract.Assert(linkResourceSetWrapper != null, "ODataDeltaResourceSetWrapper for delta link should not be null.");
                    linkResourceSetWrapper.DeltaItems.Add(linkBaseWrapper);
                    break;

                case ODataReaderState.Primitive:
                    Contract.Assert(itemsStack.Count > 0, "The primitive should be a non-null primitive value within an untyped collection.");
                    // Be noted:
                    // 1) if a 'null' value or a resource/object in the untyped collection goes to ODataResource flow
                    // 2) if a collection value in the untyped collection goes to ODataResourceSet flow
                    // 3) Since it's untyped, there's no logic for 'Enum' value, it means it's treated as primitive value.
                    ODataResourceSetWrapper resourceSetParentWrapper = (ODataResourceSetWrapper)itemsStack.Peek();
                    resourceSetParentWrapper.Items.Add(new ODataPrimitiveWrapper((ODataPrimitiveValue)reader.Item));
                    break;

                case ODataReaderState.NestedProperty:
                    Contract.Assert(itemsStack.Count > 0, "The nested property info should be a non-null primitive value within resource wrapper.");
                    ODataResourceWrapper resourceParentWrapper = (ODataResourceWrapper)itemsStack.Peek();
                    resourceParentWrapper.NestedPropertyInfos.Add((ODataPropertyInfo)reader.Item);
                    break;

                default:
                    Contract.Assert(false, "We should never get here, it means the ODataReader reported a wrong state.");
                    break;
            }
        }

        /// <summary>
        /// Read the normal resource.
        /// </summary>
        /// <param name="reader">The OData reader.</param>
        /// <param name="itemsStack">The item stack.</param>
        /// <param name="topLevelItem">the top level item.</param>
        private static void ReadResource(ODataReader reader, Stack<ODataItemWrapper> itemsStack, ref ODataItemWrapper topLevelItem)
        {
            Contract.Assert(reader != null);
            Contract.Assert(itemsStack != null);
            Contract.Assert(ODataReaderState.ResourceStart == reader.State);

            ODataResource resource = (ODataResource)reader.Item;
            ODataResourceWrapper resourceWrapper = null;
            if (resource != null)
            {
                resourceWrapper = new ODataResourceWrapper(resource);
            }

            if (itemsStack.Count == 0)
            {
                Contract.Assert(resource != null, "The top-level resource can never be null.");
                topLevelItem = resourceWrapper;
            }
            else
            {
                ODataItemWrapper parentItem = itemsStack.Peek();
                ODataResourceSetWrapper parentResourceSet = parentItem as ODataResourceSetWrapper;
                ODataDeltaResourceSetWrapper parentDeleteResourceSet = parentItem as ODataDeltaResourceSetWrapper;
                if (parentResourceSet != null)
                {
                    parentResourceSet.Resources.Add(resourceWrapper);
                    parentResourceSet.Items.Add(resourceWrapper);// in the next major release, we should only use 'Items'.
                }
                else if (parentDeleteResourceSet != null)
                {
                    // Delta resource set could have the normal resource
                    parentDeleteResourceSet.DeltaItems.Add(resourceWrapper);
                }
                else
                {
                    ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)parentItem;
                    Contract.Assert(parentNestedResource.NestedResourceInfo.IsCollection == false, "Only singleton nested properties can contain resource as their child.");
                    Contract.Assert(parentNestedResource.NestedItems.Count == 0, "Each nested property can contain only one resource as its direct child.");
                    parentNestedResource.NestedItems.Add(resourceWrapper);
                }
            }

            itemsStack.Push(resourceWrapper);
        }

        /// <summary>
        /// Read the deleted resource.
        /// </summary>
        /// <param name="reader">The OData reader.</param>
        /// <param name="itemsStack">The item stack.</param>
        private static void ReadDeletedResource(ODataReader reader, Stack<ODataItemWrapper> itemsStack)
        {
            Contract.Assert(reader != null);
            Contract.Assert(itemsStack != null);
            Contract.Assert(ODataReaderState.DeletedResourceStart == reader.State);

            ODataDeletedResource deletedResource = (ODataDeletedResource)reader.Item;
            Contract.Assert(deletedResource != null, "Deleted resource should not be null");

            ODataResourceWrapper deletedResourceWrapper = new ODataResourceWrapper(deletedResource);

            // top-level resource should never be deleted.
            Contract.Assert(itemsStack.Count != 0, "Deleted Resource should not be top level item");

            ODataItemWrapper parentItem = itemsStack.Peek();
            ODataDeltaResourceSetWrapper parentDeletaResourceSet = parentItem as ODataDeltaResourceSetWrapper;
            if (parentDeletaResourceSet != null)
            {
                parentDeletaResourceSet.DeltaItems.Add(deletedResourceWrapper);
            }
            else
            {
                ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                Contract.Assert(parentNestedResource.NestedResourceInfo.IsCollection == false, "Only singleton nested properties can contain resource as their child.");
                Contract.Assert(parentNestedResource.NestedItems.Count == 0, "Each nested property can contain only one deleted resource as its direct child.");
                parentNestedResource.NestedItems.Add(deletedResourceWrapper);
            }

            itemsStack.Push(deletedResourceWrapper);
        }

        /// <summary>
        /// Read the resource set.
        /// </summary>
        /// <param name="reader">The OData reader.</param>
        /// <param name="itemsStack">The item stack.</param>
        /// <param name="topLevelItem">The top level item.</param>
        private static void ReadResourceSet(ODataReader reader, Stack<ODataItemWrapper> itemsStack, ref ODataItemWrapper topLevelItem)
        {
            Contract.Assert(reader != null);
            Contract.Assert(itemsStack != null);
            Contract.Assert(ODataReaderState.ResourceSetStart == reader.State);

            ODataResourceSet resourceSet = (ODataResourceSet)reader.Item;
            Contract.Assert(resourceSet != null, "ResourceSet should never be null.");

            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);
            if (itemsStack.Count > 0)
            {
                ODataItemWrapper peekedWrapper = itemsStack.Peek();
                if (peekedWrapper is ODataNestedResourceInfoWrapper parentNestedResourceInfo)
                {
                    Contract.Assert(parentNestedResourceInfo.NestedResourceInfo.IsCollection == true, "Only collection nested properties can contain resource set as their child.");
                    Contract.Assert(parentNestedResourceInfo.NestedItems.Count == 0, "Each nested property can contain only one resource set as its direct child.");
                    parentNestedResourceInfo.NestedItems.Add(resourceSetWrapper);
                }
                else
                {
                    ODataResourceSetWrapper parentResourceSet = (ODataResourceSetWrapper)peekedWrapper;
                    parentResourceSet.Items.Add(resourceSetWrapper);
                }
            }
            else
            {
                topLevelItem = resourceSetWrapper;
            }

            itemsStack.Push(resourceSetWrapper);
        }

        /// <summary>
        /// Read the delta resource set.
        /// </summary>
        /// <param name="reader">The OData reader.</param>
        /// <param name="itemsStack">The item stack.</param>
        /// <param name="topLevelItem">The top level item.</param>
        private static void ReadDeltaResourceSet(ODataReader reader, Stack<ODataItemWrapper> itemsStack, ref ODataItemWrapper topLevelItem)
        {
            Contract.Assert(reader != null);
            Contract.Assert(itemsStack != null);
            Contract.Assert(ODataReaderState.DeltaResourceSetStart == reader.State);

            ODataDeltaResourceSet deltaResourceSet = (ODataDeltaResourceSet)reader.Item;
            Contract.Assert(deltaResourceSet != null, "Delta ResourceSet should never be null.");

            ODataDeltaResourceSetWrapper deltaResourceSetWrapper = new ODataDeltaResourceSetWrapper(deltaResourceSet);
            if (itemsStack.Count > 0)
            {
                ODataNestedResourceInfoWrapper parentNestedResourceInfo = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                Contract.Assert(parentNestedResourceInfo != null, "this has to be an inner delta resource set. inner delta resource sets always have a nested resource info.");
                Contract.Assert(parentNestedResourceInfo.NestedResourceInfo.IsCollection == true, "Only collection nested properties can contain delta resource set as their child.");
                Contract.Assert(parentNestedResourceInfo.NestedItems.Count == 0, "Each nested property can contain only one delta resource set as its direct child.");
                parentNestedResourceInfo.NestedItems.Add(deltaResourceSetWrapper);
            }
            else
            {
                topLevelItem = deltaResourceSetWrapper;
            }

            itemsStack.Push(deltaResourceSetWrapper);
        }
    }
}
