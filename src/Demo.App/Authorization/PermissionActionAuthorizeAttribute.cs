using Microsoft.AspNetCore.Authorization;

namespace Demo.App.Authorization;

// This attribute derives from the [Authorize] attribute, adding 
// the ability for a user to specify a 'permission' parameter. Since authorization
// policies are looked up from the policy provider only by string, this
// authorization attribute creates is a policy name based on a constant prefix
// and the user-supplied permission parameter. A custom authorization policy provider
// (`PermissionActionPolicyProvider`) can then produce an authorization policy with 
// the necessary requirements based on this policy name.
// see https://github.com/dotnet/aspnetcore/blob/v8.0.6/src/Security/samples/CustomPolicyProvider/Authorization/MinimumAgeAuthorizeAttribute.cs
internal class PermissionActionAuthorizeAttribute : AuthorizeAttribute
{
    const string POLICY_PREFIX = "PermissionAction";
    public PermissionActionAuthorizeAttribute(string permission) => Permission = permission;
    public string Permission
    {
        get => Policy.Substring(POLICY_PREFIX.Length);
        set => Policy = $"{POLICY_PREFIX}{value}";
    }
}