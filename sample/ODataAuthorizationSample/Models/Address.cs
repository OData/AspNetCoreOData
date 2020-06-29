using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ODataAuthorizationSample.Models
{
    [Owned, ComplexType]
    public class Address
    {
        public string City { get; set; }

        public string Street { get; set; }
    }
}
