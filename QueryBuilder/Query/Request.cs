using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Query
{
    public class Request
    {
        public Request(string uri, IQueryCollection requestQueryCollection)
        {
            // TODO: Validate that the uri and queryCollection are consistent
            Uri = new Uri(uri);
            Query = requestQueryCollection;

        }

        public Request(Uri uri, IQueryCollection requestQueryCollection)
        {
            // TODO: Validate that the uri and queryCollection are consistent
            Uri = uri;
            Query = requestQueryCollection;

        }

        /// <summary>
        /// Gets the request message associated with this instance.
        /// </summary>
        public Uri Uri { get; private set; }

        public IEnumerable<KeyValuePair<string, StringValues>> Query { get; set; } // general IEnumerable key-value pairs
    }
}
