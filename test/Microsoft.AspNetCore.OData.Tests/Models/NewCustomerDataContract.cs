using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.Tests.Models;

[DataContract]
public class NewCustomerDataContract
{
    [Key]
    public int Id { get; set; }
    [IgnoreDataMember]
    [DataMember]
    public string Name { get; set; }
    [IgnoreDataMember]
    public int Age { get; set; }

    [DataMember]
    public string Street { get; set; }

    public string City { get; set; }
}
