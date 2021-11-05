using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.Tests.Models
{
    public class NewCustomerUnmapped
    {
        [Key]
        public int Id { get; set; }
        [IgnoreDataMember]
        public string Name { get; set; }
        [NotMapped]
        public int Age { get; set; }

        [DataMember]
        [IgnoreDataMember]
        public string Street { get; set; }

        [DataMember]
        public string City { get; set; }

        public string State { get; set; }
    }
}
