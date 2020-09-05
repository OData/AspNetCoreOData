// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    public class WebODataControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>, IApplicationFeatureProvider
    {
        private Type[] _controllers;

        public WebODataControllerFeatureProvider(params Type[] controllers)
        {
            _controllers = controllers;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            if (_controllers == null)
            {
                return;
            }

            feature.Controllers.Clear();
            foreach (var type in _controllers)
            {
                feature.Controllers.Add(type.GetTypeInfo());
            }
        }
    }
}
