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
    private readonly string _baseApiUrl;
    private readonly string _baseAccountApiUrl;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("BlogClient");
        _baseApiUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") + "Auth/";
        _baseAccountApiUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") + "Account/";
    }

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
            var response = await _httpClient.PostAsync(GetFullPath(_baseApiUrl, "register"), content);

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
            string fullPath = GetFullPath(_baseAccountApiUrl, "SendEmailConfirmationMail");
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

            var response = await _httpClient.PostAsync(GetFullPath(_baseApiUrl, "login"), content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                HttpContext.Session.SetString("JWToken", tokenResponse.Token);

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenResponse.Token);

                var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

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
