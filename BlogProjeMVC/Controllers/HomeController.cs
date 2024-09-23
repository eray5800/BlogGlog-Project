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
        private readonly string _baseApiUrl;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _baseApiUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

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

            string searchResults = HttpContext.Session.GetString("SearchResults");

            if (!string.IsNullOrEmpty(searchResults))
            {
                blogList = JsonConvert.DeserializeObject<IEnumerable<Blog>>(searchResults);
            }
            else
            {
                string fullPath = GetFullPath($"{_baseApiUrl}Blog/", "GetAllActiveBlogs");
                blogList = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(fullPath);
            }

            int pageSize = 10;
            int totalItemCount = blogList.Count();
            IPagedList<Blog> pagedBlogs = blogList.ToPagedList(page, pageSize);

            string categoryPath = GetFullPath($"{_baseApiUrl}admin/category/", "GetAllCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryPath);
            ViewBag.Categories = categories;
            ViewBag.BlogImageBasePath = $"{_baseApiUrl}Blog/GetImage/";

            return View(pagedBlogs);
        }

        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
