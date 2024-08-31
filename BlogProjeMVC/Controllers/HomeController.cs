using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BlogProjeMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        private string basePath = "https://localhost:7181/api/Blog/";
        private string categoryBasePath = "https://localhost:7181/api/admin/category/";

        public async Task<IActionResult> Index()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }


            IEnumerable<Blog> blogs;

            if (TempData["SearchBlogs"] != null)
            {
                blogs = JsonConvert.DeserializeObject<IEnumerable<Blog>>(TempData["SearchBlogs"].ToString());
            }
            else
            {
                string fullPath = GetFullPath(basePath, "GetAllBlogs");
                blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(fullPath);
            }

            string categoryPath = GetFullPath(categoryBasePath, "GetAllCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryPath);
            ViewBag.Categories = categories;
            ViewBag.BlogImageBasePath = "https://localhost:7181/api/Blog/GetImage/";
            return View(blogs);
        }

        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}