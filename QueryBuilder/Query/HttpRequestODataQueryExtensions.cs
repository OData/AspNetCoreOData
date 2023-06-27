using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
//using Microsoft.AspNetCore.Http;
using QueryBuilder.Abstracts;
using QueryBuilder.Edm;
//using QueryBuilder.Extensions;
//using QueryBuilder.Routing;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;

namespace QueryBuilder.Query
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpRequestODataQueryExtensions
    {
        /// <summary>
        /// Gets the OData <see cref="ETag"/> from the given request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entityTagHeaderValue">The entity tag header value.</param>
        /// <returns>The parsed <see cref="ETag"/>.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
        public static ETag GetETag(EntityTagHeaderValue entityTagHeaderValue, IETagHandler etagHandler, IEdmModel model, IEdmNavigationSource source)
        {
            if (entityTagHeaderValue != null && etagHandler != null && model != null && source != null)
            {
                if (entityTagHeaderValue.Equals(EntityTagHeaderValue.Any))
                {
                    return new ETag { IsAny = true };
                }

                // get the etag handler, and parse the etag
                //IETagHandler etagHandler = request.GetRouteServices().GetRequiredService<IETagHandler>();
                IDictionary<string, object> properties = etagHandler.ParseETag(entityTagHeaderValue) ?? new Dictionary<string, object>();
                IList<object> parsedETagValues = properties.Select(property => property.Value).ToList();

                // get property names from request
                //ODataPath odataPath = request.ODataFeature().Path;
                //IEdmModel model = request.GetModel();
                //IEdmNavigationSource source = odataPath.GetNavigationSource();
                if (model != null && source != null)
                {
                    IList<IEdmStructuralProperty> concurrencyProperties = model.GetConcurrencyProperties(source).ToList();
                    IList<string> concurrencyPropertyNames = concurrencyProperties.OrderBy(c => c.Name).Select(c => c.Name).ToList();
                    ETag etag = new ETag();

                    if (parsedETagValues.Count != concurrencyPropertyNames.Count)
                    {
                        etag.IsWellFormed = false;
                    }

                    IEnumerable<KeyValuePair<string, object>> nameValues = concurrencyPropertyNames.Zip(
                        parsedETagValues,
                        (name, value) => new KeyValuePair<string, object>(name, value));
                    foreach (var nameValue in nameValues)
                    {
                        IEdmStructuralProperty property = concurrencyProperties.SingleOrDefault(e => e.Name == nameValue.Key);
                        Contract.Assert(property != null);

                        Type clrType = model.GetClrType(property.Type);
                        Contract.Assert(clrType != null);

                        if (nameValue.Value != null)
                        {
                            Type valueType = nameValue.Value.GetType();
                            etag[nameValue.Key] = valueType != clrType
                                ? Convert.ChangeType(nameValue.Value, clrType, CultureInfo.InvariantCulture)
                                : nameValue.Value;
                        }
                        else
                        {
                            etag[nameValue.Key] = nameValue.Value;
                        }
                    }

                    return etag;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from the given request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entityTagHeaderValue">The entity tag header value.</param>
        /// <returns>The parsed <see cref="ETag{TEntity}"/>.</returns>
        public static ETag<TEntity> GetETag<TEntity>(EntityTagHeaderValue entityTagHeaderValue, IETagHandler etagHandler, IEdmModel model, IEdmNavigationSource source)
        {
            ETag etag = GetETag(entityTagHeaderValue, etagHandler, model, source);
            return etag != null
                ? new ETag<TEntity>
                {
                    ConcurrencyProperties = etag.ConcurrencyProperties,
                    IsWellFormed = etag.IsWellFormed,
                    IsAny = etag.IsAny,
                }
                : null;
        }
    }
}
