using DAL.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BlogProjeMVC.Controllers.WriterOnly
{
    [Route("writer")]
    public class WriterController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string writerBasePath = "https://localhost:7181/api/writer/";

        public WriterController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        [HttpPost("addwriterrequest")]
        public async Task<IActionResult> AddWriterRequest(WriterRequestDTO requestDTO)
        {
            string fullPath = GetFullPath(writerBasePath, "AddWriterRequest");
            var response = await _httpClient.PostAsJsonAsync(fullPath, requestDTO);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your request was sent to Admin. Please wait for approval.";
                return RedirectToAction("Index", "Home");
            }
            if(response.StatusCode == System.Net.HttpStatusCode.Forbidden)
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
