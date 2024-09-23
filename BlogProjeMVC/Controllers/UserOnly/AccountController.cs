using DAL.Models;
using DAL.Models.DTO.Account;
using DAL.Models.HelperModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogProjeMVC.Controllers.UserOnly
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseAccountPath;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _baseAccountPath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "Account/";
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var role = HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin" || role == "Writer" || role == "User")
                {
                    await next();
                    return;
                }
            }

            context.Result = Unauthorized();
        }

        public async Task<IActionResult> Profile()
        {
            string fullPath = GetFullPath(_baseAccountPath, "Profile");
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
            string profileFullPath = GetFullPath(_baseAccountPath, "Profile");

            if (!ModelState.IsValid)
            {
                var userWithBlog = await _httpClient.GetFromJsonAsync<UserBlogs>(profileFullPath);
                return View("Profile", userWithBlog);
            }

            string fullPath = GetFullPath(_baseAccountPath, "ChangePassword");
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
            string profileFullPath = GetFullPath(_baseAccountPath, "Profile");
            if (!ModelState.IsValid)
            {
                var userWithBlog = await _httpClient.GetFromJsonAsync<UserBlogs>(profileFullPath);
                return View("Profile", userWithBlog);
            }

            string fullPath = GetFullPath(_baseAccountPath, "RequestEmailChange");
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
