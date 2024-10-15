//-----------------------------------------------------------------------------
// <copyright file="InstanceAnnotationsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations
{
    public class InsCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public IList<int> Magics { get; set; }

        public InsAddress Location { get; set; }

        public InsFavoriteSports FavoriteSports { get; set; }

        public IODataInstanceAnnotationContainer AnnotationContainer { get; set; }
    }

    public class InsAddress
    {
        public string City { get; set; }

        public string Street { get; set; }

        public IODataInstanceAnnotationContainer AnnotationContainer { get; set; }
    }

    public enum InsSport
    {
        Soccer,

        Badminton,

        Basketball,

        Baseball,

        Swimming,

        Tennis
    }

    public class InsFavoriteSports
    {
        public InsSport LikeMost { get; set; }

        public List<InsSport> Like { get; set; }
    }
}
