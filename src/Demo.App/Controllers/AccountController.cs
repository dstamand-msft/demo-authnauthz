using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Demo.App.Controllers;

public class AccountController : Controller
{
    public async Task Signin(string? returnUrl = null)
    {
        var authenticationProperties = new AuthenticationProperties();
        authenticationProperties.RedirectUri = returnUrl ?? "/";
        await HttpContext.ChallengeAsync(authenticationProperties);
    }

    public async Task<IActionResult> Signout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    public IActionResult Denied(string returnUrl = null)
    {
        return View();
    }
}