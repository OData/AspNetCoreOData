//-----------------------------------------------------------------------------
// <copyright file="UnqualifiedCallAndAlternateKeyResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// The OData uri resolver wrapper for alternate key and unqualified function call.
    /// </summary>
    internal class UnqualifiedCallAndAlternateKeyResolver : ODataUriResolver
    {
        private readonly AlternateKeysODataUriResolver _alternateKey;
        private readonly UnqualifiedODataUriResolver _unqualified;

        private bool _enableCaseInsensitive;

        public UnqualifiedCallAndAlternateKeyResolver(IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            _alternateKey = new AlternateKeysODataUriResolver(model);
            _unqualified = new UnqualifiedODataUriResolver();
        }

        /// <inheritdoc/>
        public override bool EnableCaseInsensitive
        {
            get
            {
                return _enableCaseInsensitive;
            }
            set
            {
                _enableCaseInsensitive = value;
                _alternateKey.EnableCaseInsensitive = this._enableCaseInsensitive;
                _unqualified.EnableCaseInsensitive = this._enableCaseInsensitive;
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IEdmOperation> ResolveUnboundOperations(IEdmModel model, string identifier)
        {
            return _unqualified.ResolveUnboundOperations(model, identifier);
        }

        /// <inheritdoc/>
        public override IEnumerable<IEdmOperation> ResolveBoundOperations(IEdmModel model, string identifier,
            IEdmType bindingType)
        {
            return _unqualified.ResolveBoundOperations(model, identifier, bindingType);
        }

        /// <inheritdoc/>
        public override IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type, IDictionary<string, string> namedValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            return _alternateKey.ResolveKeys(type, namedValues, convertFunc);
        }
    }
}
