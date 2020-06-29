using System.Collections.Generic;

namespace AspNetCore3ODataPermissionsSample.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual Address HomeAddress { get; set; }

        public virtual IList<Address> FavoriteAddresses { get; set; }

        public virtual Order Order { get; set; }

        public virtual IList<Order> Orders { get; set; }
    }

}
