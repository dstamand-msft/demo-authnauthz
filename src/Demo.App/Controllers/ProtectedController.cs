using Demo.App.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
