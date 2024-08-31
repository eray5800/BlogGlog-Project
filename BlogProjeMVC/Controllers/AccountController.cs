using DAL.Models;
using DAL.Models.DTO.Account;
using DAL.Models.HelperModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogProjeMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        private string baseAccountPath = "https://localhost:7181/api/Account/";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Session'dan rolleri al
            var role = HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(role))
            {
                // Rolleri kontrol et
                if (role == "Admin" || role == "Writer" || role == "User")
                {
                    // Rolü uygun olan kullanıcı aksiyona devam edebilir
                    await next();
                    return;
                }
            }

            // Uygun bir rol yoksa yetkisiz sayfasına yönlendir
            context.Result = Unauthorized();
        }

        public async Task<IActionResult> Profile()
        {
            string fullPath = GetFullPath(baseAccountPath, "Profile");
            var userWithBlogs = await _httpClient.GetFromJsonAsync<UserBlogs>(fullPath);
            if (!userWithBlogs.User.EmailConfirmed)
            {
                return RedirectToAction("EmailConfirmationSent", "Auth");
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(userWithBlogs);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            string profileFullPath = GetFullPath(baseAccountPath, "Profile");

            if (!ModelState.IsValid)
            {
                var userWithBlog = await _httpClient.GetFromJsonAsync<UserBlogs>(profileFullPath);

                return View("Profile", userWithBlog);
            }

            string fullPath = GetFullPath(baseAccountPath, "ChangePassword");
            var response = await _httpClient.PostAsJsonAsync(fullPath, dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Password updated successfully.";
                return RedirectToAction("Profile");
            }
            var userWithBlogs = await _httpClient.GetFromJsonAsync<UserBlogs>(profileFullPath);

            await HandleErrorsAsync(response, "Profile", profileFullPath);
            return View("Profile", userWithBlogs);
        }

        [HttpPost]
        public async Task<IActionResult> RequestEmailChange(ChangeEmailDto dto)
        {
            string profileFullPath = GetFullPath(baseAccountPath, "Profile");
            if (!ModelState.IsValid)
            {
                var userWithBlog = await _httpClient.GetFromJsonAsync<UserBlogs>(profileFullPath);
                return View("Profile", userWithBlog);
            }

            string fullPath = GetFullPath(baseAccountPath, "RequestEmailChange");
            var response = await _httpClient.PostAsJsonAsync(fullPath, dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Email change request sent successfully.";
                return RedirectToAction("Profile");
            }
            var userWithBlogs = await _httpClient.GetFromJsonAsync<UserBlogs>(profileFullPath);

            await HandleErrorsAsync(response, "Profile", profileFullPath);
            return View("Profile", userWithBlogs);
        }

        private async Task HandleErrorsAsync(HttpResponseMessage response, string viewName, string profileFullPath)
        {
            var errors = await response.Content.ReadFromJsonAsync<ErrorListModel>();
            if (errors?.Errors != null)
            {
                foreach (var error in errors.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
            else
            {
                ModelState.AddModelError("", "Operation failed.");
            }

            var userProfile = await _httpClient.GetFromJsonAsync<AppUser>(profileFullPath);
            View(viewName, userProfile);
        }

        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
