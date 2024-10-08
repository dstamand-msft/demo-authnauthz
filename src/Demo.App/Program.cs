using Demo.App.Authorization;
using Demo.App.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
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
                    options.Prompt = "select_account";

                    options.Events.OnTokenValidated = context =>
                    {
                        var token = context.SecurityToken.RawData;
                        
                        System.Diagnostics.Debug.WriteLine($"===> OnTokenValidated ID TOKEN: {RemoveTokenSignature(token)}");
                        System.Diagnostics.Debug.WriteLine($"===> OnTokenValidated idtoken.home_oid: {context?.Principal.GetHomeObjectId() ?? "null"}");
                        System.Diagnostics.Debug.WriteLine($"===> OnTokenValidated idtoken.home_tid: {context?.Principal.GetHomeTenantId() ?? "null"}");

                        context.Success();
                        return Task.CompletedTask;
                    };
                    options.Events.OnAuthorizationCodeReceived = context =>
                    {
                        string client_info = context.ProtocolMessage.GetParameter("client_info");
                        System.Diagnostics.Debug.WriteLine($"===> OnAuthorizationCodeReceived client_info: {client_info}");
                        return Task.CompletedTask;
                    };

                   
                },                
                cookieOptions =>
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


        // Remove token signature for debugging and logging purposes. Tokens without signature are not valid but will still contain private details.
        private static string RemoveTokenSignature(string token)
        {
            var parts = token.Split('.');
            return $"{parts[0]}.{parts[1]}.";
        }
    }

}
