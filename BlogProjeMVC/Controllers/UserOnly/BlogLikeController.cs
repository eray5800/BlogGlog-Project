using DAL.Models.DTO.BlogDTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlogProjeMVC.Controllers.UserOnly
{
    [Route("/[controller]/[action]")]
    public class BlogLikeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string BlogLikeBasePath = "https://blogprojeapi20240904220317.azurewebsites.net/api/BlogLike/";

        public BlogLikeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        [HttpPost]
        public async Task<IActionResult> AddBlogLike(Guid blogID)
        {
            SaveBlogLikeDTO saveBlogLikeDTO = new SaveBlogLikeDTO()
            {
                BlogID = blogID
            };
            var response = await _httpClient.PostAsJsonAsync(GetFullPath(BlogLikeBasePath, $"AddBlogLike"), saveBlogLikeDTO);
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
            var response = await _httpClient.DeleteAsync(GetFullPath(BlogLikeBasePath, $"RemoveBlogLike/{blogID}"));

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
            var response = await _httpClient.GetAsync(GetFullPath(BlogLikeBasePath, $"GetUserBlogLike/{blogID}"));

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
            var response = await _httpClient.GetAsync(GetFullPath(BlogLikeBasePath, $"GetBlogLikes/{blogID}"));

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
