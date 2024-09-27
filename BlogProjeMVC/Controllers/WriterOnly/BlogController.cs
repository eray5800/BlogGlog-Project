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
    [AutoValidateAntiforgeryToken]

    public class BlogController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _categoryBasePath;
        private readonly string _blogBasePath;
        private readonly string _blogImageBasePath;
        private readonly string _blogLikeBasePath;

        public BlogController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _httpContextAccessor = httpContextAccessor;
            _categoryBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "admin/category/";
            _blogBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "Blog/";
            _blogImageBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "Blog/GetImage/";
            _blogLikeBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "BlogLike/";
        }

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

            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(GetFullPath(_categoryBasePath, "GetAllActiveCategories"));
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogDTO blogDto, List<IFormFile> imageFiles)
        {
            if (!IsUserAuthorized())
            {
                return UnauthorizedResult();
            }

            if (!IsModelValid(blogDto))
            {
                return await InvalidModelState(blogDto);
            }

            if (!ValidateImageFiles(imageFiles))
            {
                return await ImageValidationFailed(blogDto);
            }

            if (!await ProcessImagesAsync(blogDto, imageFiles))
            {
                return await ImageProcessingFailed(blogDto);
            }

            var response = await SendPostRequestAsync("CreateBlog", blogDto);

            return response.IsSuccessStatusCode
                ? RedirectToAction("Index", "Home")
                : await HandleApiError(response, blogDto);
        }

        [HttpGet("{blogID}")]
        public async Task<IActionResult> Detail(Guid blogID)
        {
            var fullPath = GetFullPath(_blogBasePath, $"GetActiveBlogByID/{blogID}");

            try
            {
                var response = await _httpClient.GetAsync(fullPath);

                if (response.IsSuccessStatusCode)
                {
                    var blog = await response.Content.ReadFromJsonAsync<Blog>();
                    ViewBag.BlogImageBasePath = _blogImageBasePath;

                    var BlogImagesResponse = await _httpClient.GetAsync(GetFullPath(_blogLikeBasePath, $"GetBlogLikes/{blogID}"));
                    ViewBag.BlogLikeCount = await BlogImagesResponse.Content.ReadAsStringAsync();

                    if (IsUserInRole("Admin") || IsUserInRole("Writer") || IsUserInRole("User"))
                    {
                        var UserLike = await _httpClient.GetFromJsonAsync<BlogLike>(GetFullPath(_blogLikeBasePath, $"GetUserBlogLike/{blogID}"));
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

            var blog = await _httpClient.GetFromJsonAsync<Blog>(GetFullPath(_blogBasePath, $"GetBlogByID/{blogID}"));

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

            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(GetFullPath(_categoryBasePath, "GetAllActiveCategories"));
            ViewBag.Categories = categories;

            return View(blogDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Update(BlogDTO blogDto, List<IFormFile> imageFiles)
        {
            if (!IsUserAuthorized())
            {
                return UnauthorizedResult();
            }

            if (!IsModelValid(blogDto))
            {
                return await InvalidModelState(blogDto);
            }

            if (!ValidateImageFiles(imageFiles))
            {
                return await ImageValidationFailed(blogDto);
            }

            if (!await ProcessImagesAsync(blogDto, imageFiles))
            {
                return await ImageProcessingFailed(blogDto);
            }
            var response = await SendPutRequestAsync($"UpdateBlog/{blogDto.BlogID}", blogDto);

            return response.IsSuccessStatusCode
                ? RedirectToAction("Index", "Home")
                : await HandleApiError(response, blogDto);
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
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateImageFiles(List<IFormFile> imageFiles)
        {
            if (imageFiles == null || !imageFiles.Any())
            {
                ModelState.AddModelError("", "You must upload at least one image.");
                return false;
            }

            if (imageFiles.Count > 5)
            {
                ModelState.AddModelError("", "You can upload a maximum of 5 images.");
                return false;
            }

            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes
            foreach (var file in imageFiles)
            {
                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError("", $"Each image must be smaller than 3 MB. '{file.FileName}' is too large.");
                    return false;
                }
            }
            return true;
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(GetFullPath(_categoryBasePath, "GetAllActiveCategories"));
            ViewBag.Categories = categories;
        }

        public async Task<IActionResult> Search([FromQuery] string textSearch)
        {
            if (string.IsNullOrEmpty(textSearch))
            {
                TempData["ErrorMessage"] = "Please add search input";
                return RedirectToAction("Index", "Home");
            }

            var blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(GetFullPath(_blogBasePath, $"Search?Text={textSearch}"));

            HttpContext.Session.SetString("SearchResults", JsonConvert.SerializeObject(blogs));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CategorySearch([FromQuery] string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                TempData["ErrorMessage"] = "Please choose a category";
                return RedirectToAction("Index", "Home");
            }

            var blogs = await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>(GetFullPath(_blogBasePath, $"CategorySearch?Text={categoryName}"));

            HttpContext.Session.SetString("SearchResults", JsonConvert.SerializeObject(blogs));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("{blogID}")]
        public async Task<IActionResult> DeleteBlog(Guid blogID)
        {
            if (!IsUserAuthorized())
            {
                return UnauthorizedResult();
            }

            var response = await SendDeleteRequestAsync($"DeleteBlog/{blogID}");

            return response.IsSuccessStatusCode
                ? RedirectToAction("Index", "Home")
                : await HandleApiError(response, new BlogDTO { BlogID = blogID });
        }

        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }

        private bool IsUserAuthorized()
        {
            return IsUserInRole("Admin") || IsUserInRole("Writer");
        }

        private IActionResult UnauthorizedResult()
        {
            return Unauthorized();
        }

        private bool IsModelValid(BlogDTO blogDto)
        {
            return ModelState.IsValid;
        }

        private async Task<IActionResult> InvalidModelState(BlogDTO blogDto)
        {
            await LoadCategoriesAsync();
            return View(blogDto);
        }

        private async Task<IActionResult> ImageValidationFailed(BlogDTO blogDto)
        {
            await LoadCategoriesAsync();
            ModelState.AddModelError("", "Please choose correct image files.");
            return View(blogDto);
        }

        private async Task<IActionResult> ImageProcessingFailed(BlogDTO blogDto)
        {
            await LoadCategoriesAsync();
            return View(blogDto);
        }

        private async Task<HttpResponseMessage> SendPutRequestAsync(string endpoint, BlogDTO blogDto)
        {
            return await _httpClient.PutAsJsonAsync(GetFullPath(_blogBasePath, endpoint), blogDto);
        }

        private async Task<HttpResponseMessage> SendPostRequestAsync(string endpoint, BlogDTO blogDto)
        {
            return await _httpClient.PostAsJsonAsync(GetFullPath(_blogBasePath, endpoint), blogDto);
        }

        private async Task<HttpResponseMessage> SendDeleteRequestAsync(string endpoint)
        {
            return await _httpClient.DeleteAsync(GetFullPath(_blogBasePath, endpoint));
        }

        private async Task<IActionResult> HandleApiError(HttpResponseMessage response, BlogDTO blogDto)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"An error occurred: {errorMessage}");

            await LoadCategoriesAsync();
            return View(blogDto);
        }
    }
}
