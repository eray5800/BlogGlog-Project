using BlogProjeMVC.HtmlHelpers;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace BlogProjeMVC.Controllers.WriterOnly
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

        private string categoryBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/admin/category/";
        private string blogBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Blog/";
        private string BlogImageBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Blog/GetImage/";

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
        public async Task<IActionResult> Create(BlogDTO blogDto, List<IFormFile> imageFiles)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(blogDto);
            }

            // Ensure at least one image is uploaded
            if (imageFiles == null || !imageFiles.Any())
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "You must upload at least one image.");
                return View(blogDto);
            }

            // Validate image files count and size
            if (imageFiles.Count > 5)
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "You can upload a maximum of 5 images.");
                return View(blogDto);
            }

            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes
            foreach (var file in imageFiles)
            {
                if (file.Length > maxFileSize)
                {
                    await LoadCategoriesAsync();
                    ModelState.AddModelError("", $"Each image must be smaller than 3 MB. '{file.FileName}' is too large.");
                    return View(blogDto);
                }
            }

            if (!await ProcessImagesAsync(blogDto, imageFiles))
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "Please choose correct image files.");
                return View(blogDto);
            }

            string blogFullPath = GetFullPath(blogBasePath, "AddBlog");
            var response = await _httpClient.PostAsJsonAsync(blogFullPath, blogDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "An error occurred while creating the blog.");
                return View(blogDto);
            }
        }





        [HttpGet("{blogID}")]
        public async Task<IActionResult> Detail(Guid blogID)
        {
            string fullPath = GetFullPath(blogBasePath, $"GetActiveBlogByID/{blogID}");

            try
            {
                var response = await _httpClient.GetAsync(fullPath);

                if (response.IsSuccessStatusCode)
                {
                    var blog = await response.Content.ReadFromJsonAsync<Blog>();
                    ViewBag.BlogImageBasePath = BlogImageBasePath;
                    blog.Content = HtmlSanitizer.HtmlEncodeScriptTags(blog.Content);

                    return View(blog);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound("Blog not found.");
                }

                return StatusCode((int)response.StatusCode, "Error retrieving blog.");
            }
            catch (HttpRequestException ex)
            {

                return StatusCode(500, "Internal server error.");
            }
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
        public async Task<IActionResult> Update(BlogDTO blogDto, List<IFormFile> imageFiles)
        {
            if (!IsUserInRole("Admin") && !IsUserInRole("Writer"))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(blogDto);
            }

            // Ensure at least one image is uploaded
            if (imageFiles == null || !imageFiles.Any())
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "You must upload at least one image.");
                return View(blogDto);
            }

            // Validate image files count and size
            if (imageFiles.Count > 5)
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "You can upload a maximum of 5 images.");
                return View(blogDto);
            }

            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes
            foreach (var file in imageFiles)
            {
                if (file.Length > maxFileSize)
                {
                    await LoadCategoriesAsync();
                    ModelState.AddModelError("", $"Each image must be smaller than 3 MB. '{file.FileName}' is too large.");
                    return View(blogDto);
                }
            }

            if (!await ProcessImagesAsync(blogDto, imageFiles))
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "Please choose correct image files.");
                return View(blogDto);
            }

            string blogFullPath = GetFullPath(blogBasePath, $"UpdateBlog/{blogDto.BlogID}");
            var response = await _httpClient.PutAsJsonAsync(blogFullPath, blogDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"An error occurred while updating the blog: {errorMessage}");

                await LoadCategoriesAsync();
                return View(blogDto);
            }
        }



        private async Task<bool> ProcessImagesAsync(BlogDTO blogDto, List<IFormFile> imageFiles)
        {
            foreach (var imageFile in imageFiles)
            {
                if (imageFile.Length > 0)
                {
                    string imageExtension = Path.GetExtension(imageFile.FileName).ToLower();
                    if (imageExtension != ".png" && imageExtension != ".jpeg" && imageExtension != ".jpg")
                    {
                        return false; // Invalid image file
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        blogDto.BlogImages.Add(new BlogImageDTO
                        {
                            BlogImageName = Path.GetFileNameWithoutExtension(imageFile.FileName),
                            BlogImageExtension = imageExtension,
                            Base64Image = Convert.ToBase64String(imageBytes)
                        });
                    }
                }
            }
            return true; // Successfully processed all images
        }


        private async Task LoadCategoriesAsync()
        {
            string fullPath = GetFullPath(categoryBasePath, "GetAllActiveCategories");
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(fullPath);
            ViewBag.Categories = categories;
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
