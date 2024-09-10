using BAL.BlogServices;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogProjeAPI.Controllers.UserOnly
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BlogLikeController : ControllerBase
    {

        public readonly IBlogLikeService _blogLikeService;
        public readonly IBlogService _blogService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;


        public BlogLikeController(IBlogLikeService blogLikeService, IBlogService blogService, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _blogLikeService = blogLikeService;
            _blogService = blogService;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet("{blogID}")]
        [AllowAnonymous]

        public async Task<IActionResult> GetBlogLikes(Guid blogID)
        {
            var blogLikes = await _blogLikeService.GetBlogLikeCountAsync(blogID);
            return Ok(blogLikes);
        }

        [HttpGet("{blogID}")]

        public async Task<IActionResult> GetUserBlogLike(Guid blogID)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }
            var blogLike = await _blogLikeService.GetBlogLikeAsync(blogID,Guid.Parse(userId));
            return Ok(blogLike);
        }

        [HttpPost]
        public async Task<IActionResult> AddBlogLike(SaveBlogLikeDTO saveBlogLikeDTO)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            Blog blog = await _blogService.GetBlogByIDAsync(saveBlogLikeDTO.BlogID);
            AppUser user = await GetUserByIdAsync(userId);
            BlogLike blogLike = new BlogLike
            {
                Blog = blog,
                User = user
            };
            var likeResult = await _blogLikeService.AddBlogLikeAsync(blogLike);
            if (likeResult == -1)
            {
                return BadRequest("An error occurred while liking the blog.");
            }

            return Ok("Blog liked successfully.");
        }


        [HttpDelete("{blogID}")]

        public async Task<IActionResult> RemoveBlogLike(Guid blogID)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }
            var likeResult = await _blogLikeService.RemoveBlogLikeAsync(blogID,Guid.Parse(userId));
            if (likeResult == -1)
            {
                return BadRequest("An error occurred while removing the like from the blog.");
            }

            return Ok("Blog like removed successfully.");
        }

        private async Task<AppUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        private string GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

    }
}
