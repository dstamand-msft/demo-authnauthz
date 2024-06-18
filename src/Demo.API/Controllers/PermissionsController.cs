using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[Microsoft.Identity.Web.AuthorizeForScopes(Scopes = new[] { "user_access" })]
    public class PermissionsController : ControllerBase
    {
        public PermissionsController()
        {
        }

        [HttpPost]
        public IActionResult CheckPermission([FromBody] PermissionRequest permission)
        {
            var user = HttpContext.User;

            if (permission.Permission == "read")
            {
                return Ok();
            }

            return Forbid();
        }

        public record PermissionRequest(string Permission);
    }
}
