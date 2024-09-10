using BlogProjeMVC.HtmlHelpers;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
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

        private readonly string categoryBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/admin/category/";
        private readonly string blogBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Blog/";
        private readonly string BlogImageBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/Blog/GetImage/";
        private readonly string BlogLikeBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/BlogLike/";


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

            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(GetFullPath(categoryBasePath, "GetAllActiveCategories"));
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

            ValidateImageFiles(imageFiles);

            if (!await ProcessImagesAsync(blogDto, imageFiles))
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "Please choose correct image files.");
                return View(blogDto);
            }

            var response = await _httpClient.PostAsJsonAsync(GetFullPath(blogBasePath, "AddBlog"), blogDto);

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
            var fullPath = GetFullPath(blogBasePath, $"GetActiveBlogByID/{blogID}");

            try
            {
                var response = await _httpClient.GetAsync(fullPath);

                if (response.IsSuccessStatusCode)
                {
                    var blog = await response.Content.ReadFromJsonAsync<Blog>();
                    ViewBag.BlogImageBasePath = BlogImageBasePath;

                    var BlogImagesResponse = await _httpClient.GetAsync(GetFullPath(BlogLikeBasePath, $"GetBlogLikes/{blogID}"));
                    ViewBag.BlogLikeCount = await BlogImagesResponse.Content.ReadAsStringAsync();

                    if (IsUserInRole("Admin") || IsUserInRole("Writer") || IsUserInRole("User"))
                    {
                        var UserLike = await _httpClient.GetFromJsonAsync<BlogLike>(GetFullPath(BlogLikeBasePath, $"GetUserBlogLike/{blogID}"));
                        ViewBag.UserLike = UserLike.BlogLikeID != Guid.Empty ? true : false;
                    }
                    else
                    {
                        ViewBag.UserLike = null;
                    }

                    blog.Content = HtmlSanitizer.HtmlEncodeScriptTags(blog.Content);
                    return View(blog);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound("Blog not found.");
                }

                return StatusCode((int)response.StatusCode, "Error retrieving blog.");
            }
            catch (HttpRequestException)
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

            var blog = await _httpClient.GetFromJsonAsync<Blog>(GetFullPath(blogBasePath, $"GetBlogByID/{blogID}"));

            if (blog == null)
            {
                return NotFound();
            }

            var blogDTO = new BlogDTO
            {
                BlogID = blog.BlogId,
                BlogTitle = blog.BlogTitle,
                Content = blog.Content,
                SelectedCategoryId = blog.Category?.CategoryID ?? Guid.Empty,
                BlogTags = string.Join(", ", blog.BlogTags.Select(tag => tag.TagName)),
                IsActive = blog.IsActive,
            };

            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(GetFullPath(categoryBasePath, "GetAllActiveCategories"));
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

            ValidateImageFiles(imageFiles);


            if (!await ProcessImagesAsync(blogDto, imageFiles))
            {
                await LoadCategoriesAsync();
                ModelState.AddModelError("", "Please choose correct image files.");
                return View(blogDto);
            }

            var response = await _httpClient.PutAsJsonAsync(GetFullPath(blogBasePath, $"UpdateBlog/{blogDto.BlogID}"), blogDto);

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
                    var imageExtension = Path.GetExtension(imageFile.FileName).ToLower();
                    if (imageExtension != ".png" && imageExtension != ".jpeg" && imageExtension != ".jpg" && imageExtension != ".gif")
                    {
                        return false;
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        blogDto.BlogImages.Add(new BlogImageDTO
                        {
                            BlogImageName = Path.GetFileNameWithoutExtension(imageFile.FileName),
                            BlogImageExtension = imageExtension,
                            Base64Image = Convert.ToBase64String(imageBytes)
                        });
                    }
                }
            }
            return true;
        }

        private void ValidateImageFiles(List<IFormFile> imageFiles)
        {
            if (imageFiles == null || !imageFiles.Any())
            {
                ModelState.AddModelError("", "You must upload at least one image.");
            }

            if (imageFiles.Count > 5)
            {
                ModelState.AddModelError("", "You can upload a maximum of 5 images.");
            }

            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes
            foreach (var file in imageFiles)
            {
                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError("", $"Each image must be smaller than 3 MB. '{file.FileName}' is too large.");
                }
            }
        }
        private async Task LoadCategoriesAsync()
        {
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(GetFullPath(categoryBasePath, "GetAllActiveCategories"));
            ViewBag.Categories = categories;
        }

        public async Task<IActionResult> Search([FromQuery] string textSearch)
        {
            if (string.IsNullOrEmpty(textSearch))
            {
                TempData["ErrorMessage"] = "Please add search input";
                return RedirectToAction("Index", "Home");
            }

            var blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(GetFullPath(blogBasePath, $"Search?Text={textSearch}"));
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

            var blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(GetFullPath(blogBasePath, $"CategorySearch?Text={categoryName}"));
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

            var response = await _httpClient.DeleteAsync(GetFullPath(blogBasePath, $"DeleteBlog/{blogID}"));

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
