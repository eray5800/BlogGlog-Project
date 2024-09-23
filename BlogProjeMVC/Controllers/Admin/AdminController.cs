using Microsoft.AspNetCore.Mvc;

namespace BlogProjeMVC.Controllers.Admin
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _adminBasePath;

        public AdminController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _adminBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "admin/";
        }

        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Dashboard()
        {
            var roles = _httpContextAccessor.HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(roles) && roles == "Admin")
            {
                return View();
            }

            return Unauthorized();
        }
    }
}
