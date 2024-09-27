using DAL.Models.DTO.BlogDTO;
using Microsoft.AspNetCore.Mvc;

namespace BlogProjeMVC.Controllers.UserOnly
{
    [Route("/[controller]/[action]")]
    [AutoValidateAntiforgeryToken]

    public class BlogLikeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _blogLikeBasePath;

        public BlogLikeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
            _blogLikeBasePath = configuration.GetValue<string>("ApiSettings:BaseUrl") + "BlogLike/";
        }

        [HttpPost]
        public async Task<IActionResult> AddBlogLike(Guid blogID)
        {
            SaveBlogLikeDTO saveBlogLikeDTO = new SaveBlogLikeDTO()
            {
                BlogID = blogID
            };
            var response = await _httpClient.PostAsJsonAsync(GetFullPath(_blogLikeBasePath, "AddBlogLike"), saveBlogLikeDTO);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Detail", "Blog", new { id = blogID });
            }
            else
            {
                return BadRequest("An error occurred while liking the blog.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBlogLike(Guid blogID)
        {
            var response = await _httpClient.DeleteAsync(GetFullPath(_blogLikeBasePath, $"RemoveBlogLike/{blogID}"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Detail", "Blog", new { id = blogID });
            }
            else
            {
                return BadRequest("An error occurred while removing the like from the blog.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBlogLike(Guid blogID)
        {
            var response = await _httpClient.GetAsync(GetFullPath(_blogLikeBasePath, $"GetUserBlogLike/{blogID}"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            else
            {
                return BadRequest("An error occurred while getting the blog like.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogLikes(Guid blogID)
        {
            var response = await _httpClient.GetAsync(GetFullPath(_blogLikeBasePath, $"GetBlogLikes/{blogID}"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            else
            {
                return BadRequest("An error occurred while getting the blog likes.");
            }
        }

        public string GetFullPath(string basePath, string actionName)
        {
            return string.Concat(basePath, actionName);
        }
    }
}
