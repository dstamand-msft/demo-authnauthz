using Demo.App.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Demo.App.Controllers
{
    [Authorize]
    public class ProtectedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [PermissionActionAuthorize("read")]
        public IActionResult ActionWithSpecificPermissionsRead()
        {
            ViewBag.Success = true;
            ViewBag.Message = "This is a protected action with specific permissions (read)";
            return View("Index");
        }

        [PermissionActionAuthorize("write")]
        public IActionResult ActionWithSpecificPermissionsWrite()
        {
            ViewBag.Success = false;
            ViewBag.Message = "(should never come here)... This is a protected action with specific permissions (write)";
            return View("Index");
        }

        [Authorize(Roles = "Writers")]
        public IActionResult ActionWithSpecificRoleWriters()
        {
            ViewBag.Success = true;
            ViewBag.Message = "This is a protected action with specific role based access (writers)";
            return View("Index");
        }

        //[Microsoft.Identity.Web.AuthorizeForScopes(Scopes = new[] { "user_impersonation" })]
        [RequiredScope(new []{ "user_impersonation" })]
        public IActionResult ActionWithSpecificScope()
        {
            ViewBag.Success = true;
            ViewBag.Message = "This is a protected action with specific scope (user_impersonation)";
            return View("Index");
        }
    }
}
