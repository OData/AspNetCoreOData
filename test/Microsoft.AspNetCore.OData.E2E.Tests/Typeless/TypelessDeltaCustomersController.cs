// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless
{

    public class TypelessDeltaCustomersController : ODataController
    {
        public IEdmEntityType DeltaCustomerType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessCustomer") as IEdmEntityType;
            }
        }

        public IEdmEntityType DeltaOrderType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessOrder") as IEdmEntityType;
            }
        }

        public IEdmComplexType DeltaAddressType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessAddress") as IEdmComplexType;
            }
        }

        public IActionResult Get()
        {
            EdmChangedObjectCollection changedCollection = new EdmChangedObjectCollection(DeltaCustomerType);
            //Changed or Modified objects are represented as EdmDeltaResourceObjects
            for (int i = 0; i < 10; i++)
            {
                dynamic typelessCustomer = new EdmDeltaResourceObject(DeltaCustomerType);
                typelessCustomer.Id = i;
                typelessCustomer.Name = string.Format("Name {0}", i);
                typelessCustomer.FavoriteNumbers = Enumerable.Range(0, i).ToArray();
                changedCollection.Add(typelessCustomer);
            }

            //Deleted objects are represented as EdmDeltaDeletedObjects
            for (int i = 10; i < 15; i++)
            {
                dynamic typelessCustomer = new EdmDeltaDeletedResourceObject(DeltaCustomerType);
                typelessCustomer.Id = new Uri(i.ToString(), UriKind.RelativeOrAbsolute);
                typelessCustomer.Reason = DeltaDeletedEntryReason.Deleted;
                changedCollection.Add(typelessCustomer);
            }

            return Ok(changedCollection);
        }

    }

}
