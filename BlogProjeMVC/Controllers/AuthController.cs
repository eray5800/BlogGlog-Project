using DAL.Models.HelperModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthController : Controller
{

    private readonly HttpClient _httpClient;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BlogClient");
    }

    private string basePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Auth/";
    private string baseAccountPath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Account/";


    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(AuthRegisterDTO model)
    {
        if (ModelState.IsValid)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{basePath}register", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("EmailConfirmationSent", "Auth", new { onlyView = "onlyView" });
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<List<ErrorModel>>(responseContent);

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> EmailConfirmationSent(string onlyView = null)
    {
        if (!string.IsNullOrEmpty(onlyView))
        {
            return View();
        }
        else
        {
            string fullPath = GetFullPath(baseAccountPath, "SendEmailConfirmationMail");
            var response = await _httpClient.GetFromJsonAsync<ApiResponse>(fullPath);

            if (response != null && response.Success)
            {
                return View();
            }

            return BadRequest(response?.Message ?? "Failed to send confirmation email.");
        }
    }




    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthLoginDTO model)
    {
        if (ModelState.IsValid)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(GetFullPath(basePath, "login"), content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                // JWT token'ı session'a ekle
                HttpContext.Session.SetString("JWToken", tokenResponse.Token);

                // JWT token'ı çözümle ve rolleri session'a ekle
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenResponse.Token);

                var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                // Rolleri session'a kaydet
                HttpContext.Session.SetString("UserRoles", string.Join(",", roles));

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid login attempt");
        }
        return View(model);
    }


    [HttpGet]

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JWToken");
        HttpContext.Session.Remove("UserRoles");
        return RedirectToAction("Index", "Home");
    }

    public string GetFullPath(string basePath, string actionName)
    {

        return string.Concat(basePath, actionName);
    }

}
