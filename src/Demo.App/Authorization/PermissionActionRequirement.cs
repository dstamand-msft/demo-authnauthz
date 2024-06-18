using Microsoft.AspNetCore.Authorization;

namespace Demo.App.Authorization;

internal class PermissionActionRequirement : IAuthorizationRequirement
{
    public string Permission { get; private set; }

    public PermissionActionRequirement(string permission) => Permission = permission;
}