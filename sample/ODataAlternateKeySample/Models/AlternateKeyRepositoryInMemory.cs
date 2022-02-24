//-----------------------------------------------------------------------------
// <copyright file="IAlternateKeyRepository.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataAlternateKeySample.Models
{
    public class AlternateKeyRepositoryInMemory : IAlternateKeyRepository
    {
        private static IList<Customer> _customers;
        private static IList<Order> _orders;
        private static IList<Person> _people;

        static AlternateKeyRepositoryInMemory()
        {
            // Customers
            var names = new[] { "Tom", "Jerry", "Mike", "Ben", "Sam", "Peter" };
            _customers = Enumerable.Range(1, 5).Select(e => new Customer
            {
                Id = e,
                Name = names[e - 1],
                SSN = "SSN-" + e + "-" + (100 + e)
            }).ToList();

            // Orders
            Guid[] tokes =
            {
                new Guid("196B3584-EF3D-41FD-90B4-76D59F9B929C"),
                new Guid("6CED5600-28BA-40EE-A2DF-E80AFADBE6C7"),
                new Guid("75036B94-C836-4946-8CC8-054CF54060EC"),
                new Guid("B3FF5460-6E77-4678-B959-DCC1C4937FA7"),
                new Guid("ED773C85-4E3C-4FC4-A3E9-9F1DA0A626DA"),
                new Guid("E9CC3D9F-BC80-4D43-8C3E-ED38E8C9A8B6")
            };

            _orders = Enumerable.Range(1, 6).Select(e => new Order
            {
                Id = e,
                Name = string.Format("Order-{0}", e),
                Token = tokes[e - 1],
                Amount = 10 * (e + 1) - e,
                Price = 8 * e
            }).ToList();

            // People
            var cs = new[] { "EN", "CN", "USA", "RU", "JP", "KO" };
            var ps = new[] { "1001", "2010", "9999", "3199992", "00001", "8110" };
            _people = Enumerable.Range(1, 6).Select(e => new Person
            {
                Id = e,
                Name = names[e - 1],
                CountryOrRegion = cs[e - 1],
                Passport = ps[e - 1]
            }).ToList();
        }

        public IEnumerable<Customer> GetCustomers() => _customers;

        public IEnumerable<Order> GetOrders() => _orders;

        public IEnumerable<Person> GetPeople() => _people;
    }
}
