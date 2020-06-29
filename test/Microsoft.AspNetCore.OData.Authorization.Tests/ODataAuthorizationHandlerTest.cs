using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Microsoft.AspNetCore.OData.Authorization.Tests
{
    public class ODataAuthorizationHandlerTest
    {
        [Theory]
        [InlineData(new [] { "Calendar.Write" }, true)]
        [InlineData(new [] { "User.Write", "User.Read" }, true)]
        [InlineData(new [] { "Calendar.Read" }, false)]
        [InlineData(new string[] { }, false)]
        public void ShouldOnlySucceedIfUserHasAnAllowedScope(string[] userScopes, bool shouldSucceed)
        {
            var requirement = new ODataAuthorizationScopesRequirement("Calendar.Write", "User.Write");
            var context = CreateAuthContext("Permission", new[] { requirement }, userScopes);
            var handler = new ODataAuthorizationHandler(FindScopes);
            handler.HandleAsync(context).Wait();
            Assert.Equal(shouldSucceed, context.HasSucceeded);
        }


        [Theory]
        [InlineData(new[] { "User.Write", "User.Read" }, true)]
        [InlineData(new[] { "Calendar.Read" }, false)]
        public void ShouldGetScopesFromClaimsIfNoScopeFinderProvided(string[] userScopes, bool shouldSucceed)
        {
            var requirement = new ODataAuthorizationScopesRequirement("Calendar.Write", "User.Write");
            var context = CreateAuthContext("Scope", new[] { requirement }, userScopes);
            var handler = new ODataAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.Equal(shouldSucceed, context.HasSucceeded);
        }

        private Task<IEnumerable<string>> FindScopes(ScopeFinderContext context)
        {
            var scopes = context.User.FindAll("Permission").Select(c => c.Value);
            return Task.FromResult(scopes);
        }
        

        private AuthorizationHandlerContext CreateAuthContext(string scopeClaimKey, IEnumerable<IAuthorizationRequirement> requirements, IEnumerable<string> userScopes)
        {
            var identity = new System.Security.Principal.GenericIdentity("Me");
            foreach (var scope in userScopes)
            {
                identity.AddClaim(new System.Security.Claims.Claim(scopeClaimKey, scope));
            }

            var principal = new System.Security.Principal.GenericPrincipal(identity, Array.Empty<string>());
            var context = new AuthorizationHandlerContext(requirements, principal, null);
            return context;
        }
    }
}
