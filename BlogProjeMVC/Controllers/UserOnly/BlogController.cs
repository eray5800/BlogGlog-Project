using DAL.Models;
using DAL.Models.DTO.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace BlogProjeMVC.Controllers.UserOnly
{
    [Route("/[controller]/[action]")]

    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _httpContextAccessor = httpContextAccessor;
        }

        private string categoryBasePath = "https://localhost:7181/api/admin/category/";
        private string blogBasePath = "https://localhost:7181/api/Blog/";

        private bool IsUserInRole(string role)
        {
            var sessionRole = _httpContextAccessor.HttpContext.Session.GetString("UserRoles");
            return role == sessionRole;
        }

        public async Task<IActionResult> Create()
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            string fullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(fullPath);
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogDTO blogDto, IFormFile imageFile)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                string fullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                var categories1 = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(fullPath);
                ViewBag.Categories = categories1;
                return View(blogDto);
            }

            string imageExtansion = Path.GetExtension(imageFile.FileName).ToLower();
            if (imageExtansion != ".png" && imageExtansion != ".jpeg" && imageExtansion != ".jpg")
            {
                string fullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                var categories1 = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(fullPath);
                ViewBag.Categories = categories1;
                ModelState.AddModelError("", "Please choose a correct image file");
                return View(blogDto);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();

                    string base64String = Convert.ToBase64String(imageBytes);
                    blogDto.BlogImage = base64String;
                    // Resmi base64 string olarak DTO'ya ekliyor burada geçici olarak tutulacak
                }
            }
            blogDto.BlogImageExtansion = imageExtansion;
            string blogFullPath = GetFullPath(blogBasePath, "AddBlog");
            var response = await _httpClient.PostAsJsonAsync(blogFullPath, blogDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string fullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                var categories1 = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(fullPath);
                ViewBag.Categories = categories1;
                ModelState.AddModelError("", "An error occurred while creating the blog.");
                return View(blogDto);
            }
        }

        [HttpGet("{blogID}")]
        public async Task<IActionResult> Detail(Guid blogID)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            string fullPath = GetFullPath(blogBasePath, $"GetBlogByID/{blogID}");
            var blog = await _httpClient.GetFromJsonAsync<Blog>(fullPath);
            ViewBag.BlogImageBasePath = "https://localhost:7181/api/Blog/GetImage/";

            return View(blog);
        }

        public async Task<IActionResult> Update(Guid blogID)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            // Blog'u API'den al
            string fullPath = GetFullPath(blogBasePath, $"GetBlogByID/{blogID}");
            var blog = await _httpClient.GetFromJsonAsync<Blog>(fullPath);

            if (blog == null)
            {
                return NotFound();
            }

            // DTO'ya dönüştür
            var blogDTO = new BlogDTO
            {
                BlogID = blog.BlogId,
                BlogTitle = blog.BlogTitle,
                Content = blog.Content,
                SelectedCategoryId = blog.Category?.CategoryID ?? Guid.Empty,
                BlogTags = string.Join(", ", blog.BlogTags.Select(tag => tag.TagName)),
                IsActive = blog.IsActive,
            };

            // Kategorileri al
            string categoryFullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryFullPath);
            ViewBag.Categories = categories;

            return View(blogDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Update(BlogDTO blogDto, IFormFile imageFile)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                // Kategorileri yeniden al
                string categoryFullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryFullPath);
                ViewBag.Categories = categories;
                return View(blogDto);
            }

            // Resim dosyası var mı?
            if (imageFile != null && imageFile.Length > 0)
            {
                string imageExtension = Path.GetExtension(imageFile.FileName).ToLower();
                if (imageExtension != ".png" && imageExtension != ".jpeg" && imageExtension != ".jpg")
                {
                    ModelState.AddModelError("", "Please choose a correct image file");

                    string categoryFullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                    var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryFullPath);
                    ViewBag.Categories = categories;
                    return View(blogDto);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    blogDto.BlogImage = Convert.ToBase64String(imageBytes);
                    blogDto.BlogImageExtansion = imageExtension;
                }
            }
            else
            {
                // Resim dosyası yoksa mevcut resmi koru
                if (string.IsNullOrEmpty(blogDto.BlogImage))
                {
                    ModelState.AddModelError("", "Image file is required.");
                    string categoryFullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                    var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryFullPath);
                    ViewBag.Categories = categories;
                    return View(blogDto);
                }
            }

            // Blog'u güncelle
            string blogFullPath = GetFullPath(blogBasePath, $"UpdateBlog/{blogDto.BlogID}");
            var response = await _httpClient.PutAsJsonAsync(blogFullPath, blogDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Hata mesajını al
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"An error occurred while updating the blog: {errorMessage}");

                string categoryFullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
                var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(categoryFullPath);
                ViewBag.Categories = categories;
                return View(blogDto);
            }
        }

        public async Task<IActionResult> Search([FromQuery] string textSearch)
        {
            if (string.IsNullOrEmpty(textSearch))
            {
                TempData["ErrorMessage"] = "Please add search input";
                return RedirectToAction("Index", "Home");
            }

            string fullPath = GetFullPath(blogBasePath, $"Search?Text={textSearch}");
            var blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(fullPath);

            TempData["SearchBlogs"] = JsonConvert.SerializeObject(blogs);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CategorySearch([FromQuery] string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                TempData["ErrorMessage"] = "Please choose category";
                return RedirectToAction("Index", "Home");
            }

            string fullPath = GetFullPath(blogBasePath, $"CategorySearch?Text={categoryName}");
            var blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(fullPath);

            TempData["SearchBlogs"] = JsonConvert.SerializeObject(blogs);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("{blogID}")]
        public async Task<IActionResult> DeleteBlog(Guid blogID)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            string blogFullPath = GetFullPath(blogBasePath, $"DeleteBlog/{blogID}");
            var response = await _httpClient.DeleteAsync(blogFullPath);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully deleted the blog";
                return RedirectToAction("Profile", "Account");
            }
            else
            {
                return BadRequest();
            }
        }

        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
