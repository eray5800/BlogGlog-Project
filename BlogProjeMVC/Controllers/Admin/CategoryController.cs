using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogProjeMVC.Controllers.Admin
{
    [Route("admin/category")]
    public class CategoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string adminCategoryBasePath = "https://localhost:7181/api/admin/category/";

        public CategoryController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Session'dan rolleri al
            var roles = HttpContext.Session.GetString("UserRoles");

            if (!string.IsNullOrEmpty(roles) && roles== "Admin")
            {
                // Admin rolü varsa aksiyona devam et
                await next();
            }
            else
            {
                // Admin rolü yoksa yetkili değil sayfasına yönlendir
                context.Result = Unauthorized();
            }
        }

        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            string fullPath = GetFullPath(adminCategoryBasePath, "GetAllCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(fullPath);
            return View(categories);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CategoryDTO categoryDto)
        {
            string fullPath = GetFullPath(adminCategoryBasePath, "AddCategory");
            var response = await _httpClient.PostAsJsonAsync(fullPath, categoryDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while adding the category.");
            return View(categoryDto);
        }

        [HttpGet("update")]
        public async Task<IActionResult> Update([FromQuery] Guid categoryID)
        {
            string fullPath = GetFullPath(adminCategoryBasePath, $"GetCategoryByID/{categoryID}");
            CategoryDTO category = await _httpClient.GetFromJsonAsync<CategoryDTO>(fullPath);

            return View(category);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(CategoryDTO categoryDTO)
        {
            if (ModelState.IsValid)
            {
                string fullPath = GetFullPath(adminCategoryBasePath, $"UpdateCategory/{categoryDTO.CategoryID}");
                var response = await _httpClient.PutAsJsonAsync(fullPath, categoryDTO);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
                return View(categoryDTO);
            }
            return View(categoryDTO);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(Guid categoryID)
        {
            string fullPath = GetFullPath(adminCategoryBasePath, $"DeleteCategory/{categoryID}");
            var response = await _httpClient.DeleteAsync(fullPath);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while deleting the category.");
            return RedirectToAction("Index");
        }

        private string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
