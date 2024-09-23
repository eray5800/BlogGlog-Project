using DAL.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogProjeMVC.Controllers.WriterOnly
{
    [Route("writer")]
    public class WriterController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _writerBasePath;

        public WriterController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _writerBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "writer/";
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var roles = HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(roles) && roles == "User")
            {
                await next();
            }
            else
            {
                context.Result = Unauthorized();
            }
        }

        [HttpPost("addwriterrequest")]
        public async Task<IActionResult> AddWriterRequest(WriterRequestDTO requestDTO)
        {
            string fullPath = GetFullPath(_writerBasePath, "AddWriterRequest");
            var response = await _httpClient.PostAsJsonAsync(fullPath, requestDTO);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your request was sent to Admin. Please wait for approval.";
                return RedirectToAction("Index", "Home");
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("EmailConfirmationSent", "Auth");
            }

            TempData["ErrorMessage"] = "An error occurred while submitting the writer request.";
            return RedirectToAction("Index", "Home");
        }

        private string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
