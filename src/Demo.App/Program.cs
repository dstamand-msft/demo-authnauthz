using Demo.App.Authorization;
using Demo.App.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;

namespace Demo.App
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var entraId = builder.Configuration.GetSection("EntraId");

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.Instance = "https://login.microsoftonline.com/";
                    options.TenantId = entraId.GetValue<string>("TenantId");
                    options.ClientId = entraId.GetValue<string>("ClientId");
                    options.ClientSecret = entraId.GetValue<string>("ClientSecret");
                    options.CallbackPath = "/signin-oidc";
                    options.SignedOutCallbackPath = "/signout-oidc";
                    options.AccessDeniedPath = "/Account/Denied";
                    options.Scope.Add(entraId.GetValue<string>("Scope"));
                    options.Events.OnTokenValidated = context =>
                    {
                        var token = context.SecurityToken.RawData;
                        System.Diagnostics.Debug.WriteLine($"===> ID TOKEN: {token}");
                        context.Success();
                        return Task.CompletedTask;
                    };
                    options.Events.OnTokenResponseReceived = context =>
                    {
                        var accessToken = context.TokenEndpointResponse.AccessToken;
                        var refreshToken = context.TokenEndpointResponse.RefreshToken;
                        System.Diagnostics.Debug.WriteLine($"===> ACCESS TOKEN: {accessToken}");
                        System.Diagnostics.Debug.WriteLine($"===> REFRESH TOKEN: {(string.IsNullOrEmpty(refreshToken) ? "N/A" : refreshToken)}");
                        return Task.CompletedTask;
                    };
                }, cookieOptions =>
                {
                    cookieOptions.AccessDeniedPath = "/Account/Denied";
                })
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            builder.Services.AddTokenAcquisition(true);

            // Replace the default authorization policy provider with our own
            // custom provider which can return authorization policies for given
            // policy names (instead of using the default policy provider)
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionActionPolicyProvider>();

            // As always, handlers must be provided for the requirements of the authorization policies
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionActionAuthorizationHandler>();

            builder.Services.AddOptions<APIOptions>()
                .Bind(builder.Configuration.GetSection("API"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = entraId.GetValue<bool>("ShowPii");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
