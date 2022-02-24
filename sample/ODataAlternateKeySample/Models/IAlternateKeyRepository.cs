//-----------------------------------------------------------------------------
// <copyright file="IAlternateKeyRepository.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataAlternateKeySample.Models
{
    public interface IAlternateKeyRepository
    {
        IEnumerable<Customer> GetCustomers();

        IEnumerable<Order> GetOrders();

        IEnumerable<Person> GetPeople();
    }
}
