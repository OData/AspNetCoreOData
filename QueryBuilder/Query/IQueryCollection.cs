using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace QueryBuilder.Query
{
    //
    // Summary:
    //     Represents the HttpRequest query string collection
    // TODO: Fix attribute compilation below:
    //[DefaultMember("Item")]
    public interface IQueryCollection : IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
    {
        //
        // Summary:
        //     Gets the value with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the value to get.
        //
        // Returns:
        //     The element with the specified key, or StringValues.Empty if the key is not present.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        //
        // Remarks:
        //     Microsoft.AspNetCore.Http.IQueryCollection has a different indexer contract than
        //     System.Collections.Generic.IDictionary`2, as it will return StringValues.Empty
        //     for missing entries rather than throwing an Exception.
        StringValues this[string key] { get; }

        //
        // Summary:
        //     Gets the number of elements contained in the Microsoft.AspNetCore.Http.IQueryCollection.
        //
        // Returns:
        //     The number of elements contained in the Microsoft.AspNetCore.Http.IQueryCollection.
        int Count { get; }
        //
        // Summary:
        //     Gets an System.Collections.Generic.ICollection`1 containing the keys of the Microsoft.AspNetCore.Http.IQueryCollection.
        //
        // Returns:
        //     An System.Collections.Generic.ICollection`1 containing the keys of the object
        //     that implements Microsoft.AspNetCore.Http.IQueryCollection.
        ICollection<string> Keys { get; }

        //
        // Summary:
        //     Determines whether the Microsoft.AspNetCore.Http.IQueryCollection contains an
        //     element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the Microsoft.AspNetCore.Http.IQueryCollection.
        //
        // Returns:
        //     true if the Microsoft.AspNetCore.Http.IQueryCollection contains an element with
        //     the key; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        bool ContainsKey(string key);
        //
        // Summary:
        //     Gets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the value to get.
        //
        //   value:
        //     The key of the value to get. When this method returns, the value associated with
        //     the specified key, if the key is found; otherwise, the default value for the
        //     type of the value parameter. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the object that implements Microsoft.AspNetCore.Http.IQueryCollection
        //     contains an element with the specified key; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        bool TryGetValue(string key, out StringValues value);
    }
}