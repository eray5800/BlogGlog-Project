using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogProjeMVC.Controllers.WriterOnly
{
    [Route("admin/writerrequest")]
    public class WriterRequestController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _adminWriterRequestBasePath;

        public WriterRequestController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _adminWriterRequestBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "admin/WriterRequest/";
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var roles = HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(roles) && roles == "Admin")
            {
                await next();
            }
            else
            {
                context.Result = Unauthorized();
            }
        }

        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            string fullPath = GetFullPath(_adminWriterRequestBasePath, "GetAllWriterRequests");
            var writerRequests = await _httpClient.GetFromJsonAsync<IEnumerable<WriterRequest>>(fullPath);
            return View(writerRequests);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve(Guid writerRequestId)
        {
            string fullPath = GetFullPath(_adminWriterRequestBasePath, $"ApproveWriterRequest/{writerRequestId}");
            var response = await _httpClient.PostAsync(fullPath, null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while approving the writer request.");
            return RedirectToAction("Index");
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject(Guid writerRequestId)
        {
            string fullPath = GetFullPath(_adminWriterRequestBasePath, $"RejectWriterRequest/{writerRequestId}");
            var response = await _httpClient.PostAsync(fullPath, null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while rejecting the writer request.");
            return RedirectToAction("Index");
        }

        private string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
