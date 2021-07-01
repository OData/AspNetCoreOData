using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless
{
    public class TypelessCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<TypelessOrder> Orders { get; set; }
        public virtual IList<TypelessAddress> Addresses { get; set; }
        public virtual IList<int> FavoriteNumbers { get; set; }
    }

    public class TypelessOrder
    {
        public int Id { get; set; }
        public TypelessAddress ShippingAddress { get; set; }
    }

    public class TypelessAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

}
