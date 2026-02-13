using System.Security.Claims;
using System.Threading.Tasks;
using UKHO.ADDS.Management.Host.Extensions;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class KeycloakRealmRoleClaimsTransformationTests
{
    [Fact]
    public async Task WhenPrincipalContainsRealmAccessRolesThenRoleClaimsAreAdded()
    {
        var identity = new ClaimsIdentity(authenticationType: "test");
        identity.AddClaim(new Claim("realm_access", "{\"roles\":[\"role-a\",\"role-b\"]}"));
        var principal = new ClaimsPrincipal(identity);

        var transformer = new KeycloakRealmRoleClaimsTransformation();
        var transformed = await transformer.TransformAsync(principal);

        Assert.Contains(transformed.Claims, c => c.Type == ClaimTypes.Role && c.Value == "role-a");
    }
}
