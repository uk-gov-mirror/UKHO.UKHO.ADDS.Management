using System.Security.Claims;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace UKHO.ADDS.Management.Host.Extensions;

public sealed class KeycloakRealmRoleClaimsTransformation : IClaimsTransformation
{
    private const string RealmAccessClaimType = "realm_access";
    private const string RolesClaimType = "roles";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(principal);
        }

        if (!TryGetRoles(identity, out var roles))
        {
            return Task.FromResult(principal);
        }

        foreach (var role in roles)
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return Task.FromResult(principal);
    }

    private static bool TryGetRoles(ClaimsIdentity identity, out IReadOnlyList<string> roles)
    {
        roles = Array.Empty<string>();

        var flatRoles = identity.FindAll(RolesClaimType).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
        if (flatRoles.Length > 0)
        {
            roles = flatRoles;
            return true;
        }

        var realmAccess = identity.FindFirst(RealmAccessClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return false;
        }

        return TryReadRealmRoles(realmAccess, out roles);
    }

    private static bool TryReadRealmRoles(string realmAccessJson, out IReadOnlyList<string> roles)
    {
        roles = Array.Empty<string>();

        try
        {
            using var document = JsonDocument.Parse(realmAccessJson);

            if (!document.RootElement.TryGetProperty("roles", out var rolesElement) || rolesElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var list = new List<string>();
            foreach (var item in rolesElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    list.Add(value);
                }
            }

            if (list.Count == 0)
            {
                return false;
            }

            roles = list;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
