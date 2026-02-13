using System.Security.Claims;
using UKHO.ADDS.Management.Shell.Modules;
using Xunit;
using System;

namespace UKHO.ADDS.Management.Shell.Tests;

public class ModulePageRoleAccessTests
{
    [Fact]
    public void WhenRequiredRolesIsNullThenUserHasAccess()
    {
        var page = new ModulePage { RequiredRoles = null };
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.True(page.UserHasAccess(principal));
    }

    [Fact]
    public void WhenRequiredRolesIsEmptyThenUserHasAccess()
    {
        var page = new ModulePage { RequiredRoles = Array.Empty<string>() };
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.True(page.UserHasAccess(principal));
    }

    [Fact]
    public void WhenUserHasAnyRequiredRoleThenUserHasAccess()
    {
        var page = new ModulePage { RequiredRoles = new[] { "role-a", "role-b" } };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "role-b") }, authenticationType: "test"));

        Assert.True(page.UserHasAccess(principal));
    }

    [Fact]
    public void WhenUserHasNoRequiredRolesThenUserHasNoAccess()
    {
        var page = new ModulePage { RequiredRoles = new[] { "role-a", "role-b" } };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "role-c") }, authenticationType: "test"));

        Assert.False(page.UserHasAccess(principal));
    }

    [Fact]
    public void WhenPrincipalIsNullThenUserHasNoAccess()
    {
        var page = new ModulePage { RequiredRoles = new[] { "role-a" } };

        Assert.False(page.UserHasAccess(null!));
    }
}
