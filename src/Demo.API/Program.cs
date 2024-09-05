using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Demo.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                //.AddMicrosoftIdentityWebApi(builder.Configuration, "EntraId")
                .AddMicrosoftIdentityWebApi(options =>
                {
                    options.Audience = builder.Configuration["EntraId:ClientId"];
                }, options =>
                {
                    options.Instance = "https://login.microsoftonline.com/";
                    options.TenantId = builder.Configuration["EntraId:TenantId"];
                    options.ClientId = builder.Configuration["EntraId:ClientId"];
                    options.ClientSecret = builder.Configuration["EntraId:ClientSecret"];
                })
                .EnableTokenAcquisitionToCallDownstreamApi(options =>
                {
                    options.Instance = "https://login.microsoftonline.com/";
                    options.ClientSecret = builder.Configuration["EntraId:ClientSecret"];
                })
                .AddInMemoryTokenCaches();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}
;