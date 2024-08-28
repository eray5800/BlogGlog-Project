using DAL.Models.HelperModels;
using Microsoft.AspNetCore.Mvc;

namespace BlogProjeMVC.Controllers.Admin
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Dashboard()
        {
            // Session'dan rolleri al
            var roles = _httpContextAccessor.HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(roles))
            {
                // Rolleri kontrol et
                

                if (roles == "Admin" )
                {
                    return View(); 
                }
            }

            return Unauthorized(); 
        }
    }
}
