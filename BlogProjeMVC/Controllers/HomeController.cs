using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;
using X.PagedList.Extensions;

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

        private string basePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Blog/";
        private string categoryBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/admin/category/";

        public async Task<IActionResult> Index(int page = 1)
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            IEnumerable<Blog> blogList;

            // Check if session has search results
            string searchResults = HttpContext.Session.GetString("SearchResults");

            if (!string.IsNullOrEmpty(searchResults))
            {
                blogList = JsonConvert.DeserializeObject<IEnumerable<Blog>>(searchResults);
            }
            else
            {
                // If no search results in session, get all active blogs
                string fullPath = GetFullPath(basePath, $"GetAllActiveBlogs");
                blogList = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(fullPath);
            }

            // Set page size and total item count
            int pageSize = 1; // Sayfa ba��na g�sterilecek blog say�s�
            int totalItemCount = blogList.Count();
            IPagedList<Blog> pagedBlogs = blogList.ToPagedList(page, pageSize);

            // Load categories
            string categoryPath = GetFullPath(categoryBasePath, "GetAllCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryPath);
            ViewBag.Categories = categories;
            ViewBag.BlogImageBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Blog/GetImage/";

            return View(pagedBlogs);
        }


        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}