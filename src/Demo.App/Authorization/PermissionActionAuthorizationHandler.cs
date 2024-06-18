﻿using System.Net.Http.Headers;
using Demo.App.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Demo.App.Authorization;

// see https://github.com/dotnet/aspnetcore/blob/v8.0.6/src/Security/samples/CustomPolicyProvider/Authorization/MinimumAgeAuthorizationHandler.cs
internal class PermissionActionAuthorizationHandler : AuthorizationHandler<PermissionActionRequirement>
{
    private readonly ILogger<PermissionActionAuthorizationHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly APIOptions _apiOptions;

    public PermissionActionAuthorizationHandler(
        ILogger<PermissionActionAuthorizationHandler> logger,
        IOptions<APIOptions> apiOptions,
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
        _apiOptions = apiOptions.Value;
    }

    // Check whether a given PermissionActionRequirement is satisfied or not for a particular context
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionActionRequirement requirement)
    {
        // Log as a warning so that it's very clear in sample output which authorization policies
        // (and requirements/handlers) are in use
        _logger.LogWarning("Evaluating authorization requirement for permission >= {permission}", requirement.Permission);
        try
        {
            string accessToken;
            // ITokensAcquisition is scoped, so we need to create a new scope here as the handler is a singleton
            using (var scope = _serviceProvider.CreateScope())
            {
                var a = scope.ServiceProvider.GetRequiredService<ITokenAcquisition>();
                var x = await a.GetAuthenticationResultForUserAsync(_apiOptions.Scopes, user: context.User);
                accessToken = x.AccessToken;
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var obj = new
            {
                permission = requirement.Permission
            };
            var content = JsonContent.Create(obj);
            var response = await httpClient.PostAsync($"{_apiOptions.BaseUrl}/api/permissions", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Permission authorization requirement {permission} satisfied", requirement.Permission);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Current user's permissions does not satisfy the permission action authorization requirement {permission}", requirement.Permission);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate the permission for User {userObjectId} for permission {permission}",
                context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value,
                requirement.Permission);
        }
    }
}