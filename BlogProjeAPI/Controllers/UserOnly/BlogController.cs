using DAL.Models;
using DAL.Models.DTO.Blog;
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
    [Authorize(Roles ="Admin,Writer")]
    public class BlogController : ControllerBase
    {
        private readonly BlogService _blogService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public BlogController(BlogService blogService, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _blogService = blogService;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _blogService.GetAllBlogsAsync();
            return Ok(blogs);
        }

        [HttpGet("{blogID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogByID(Guid blogID)
        {
            var blog = await _blogService.GetBlogByIDAsync(blogID);
            if (blog == null)
            {
                return NotFound("Blog not found.");
            }
            return Ok(blog);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string Text)
        {
            var blogs = await _blogService.Search(Text);
            
            if (blogs == null)
            {
                return NotFound("Blog not found.");
            }
            return Ok(blogs);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CategorySearch([FromQuery] string Text)
        {
            var blogs = await _blogService.SearchBlogCategoryAsync(Text);
            if (blogs == null)
            {
                return NotFound("Blog not found.");
            }
            return Ok(blogs);
        }

        [HttpPost]
        
        public async Task<IActionResult> AddBlog([FromBody] BlogDTO blogDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            // Token'dan kullanıcıyı al
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            if (!string.IsNullOrEmpty(blogDto.BlogImage))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                byte[] imageBytes = Convert.FromBase64String(blogDto.BlogImage);

                string fileName = Guid.NewGuid().ToString() + blogDto.BlogImageExtansion; // Uygun uzantıyı belirleyin
                string filePath = Path.Combine(uploadsFolder, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                blogDto.BlogImage = fileName; // DTO'ya dosya adını ekleyin
            }

            var result = await _blogService.CreateBlogAsync(blogDto, user);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding the blog.");
            }

            return Ok("Blog added successfully.");
        }

        [HttpGet("{fileName}")]
        [AllowAnonymous]
        public IActionResult GetImage(string fileName)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");
            string filePath = Path.Combine(uploadsFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Image not found.");
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType);
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
                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Kullanıcı ID'sini döndür
            }
            catch
            {
                return null;
            }
        }

        [HttpPut("{blogID}")]
        public async Task<IActionResult> UpdateBlog([FromRoute] Guid blogID, [FromBody] BlogDTO blogDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Blog'un mevcut durumunu veritabanından al
            var existingBlog = await _blogService.GetBlogByIDAsync(blogID);
            if (existingBlog == null)
            {
                return NotFound("Blog not found.");
            }

            // Token'dan kullanıcıyı al ve blogun sahibinin aynı kullanıcı olup olmadığını kontrol et
            var userId = GetUserIdFromToken();
            if (existingBlog.User.Id != userId)
            {
                return Unauthorized("You are not authorized to update this blog.");
            }

            // Yeni bir resim yüklendiyse önce eski resmi sil
            if (!string.IsNullOrEmpty(blogDto.BlogImage) && !string.IsNullOrEmpty(existingBlog.BlogImage))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");
                string existingFilePath = Path.Combine(uploadsFolder, existingBlog.BlogImage);
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }

                byte[] imageBytes = Convert.FromBase64String(blogDto.BlogImage);
                string fileName = Guid.NewGuid().ToString() + blogDto.BlogImageExtansion;
                string filePath = Path.Combine(uploadsFolder, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                blogDto.BlogImage = fileName; // DTO'ya dosya adını ekleyin
            }

            // Blog güncelleme işlemini gerçekleştir
            var result = await _blogService.UpdateBlogAsync(blogID, blogDto);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating the blog.");
            }

            return Ok("Blog updated successfully.");
        }


        [HttpDelete("{blogID}")]
        public async Task<IActionResult> DeleteBlog(Guid blogID)
        {
            // Blog'u veritabanından al
            var blog = await _blogService.GetBlogByIDAsync(blogID);
            string userID = GetUserIdFromToken();
            if(blog.User.Id != userID)
            {
                return BadRequest("This blog is not yours ");
            }
            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            // Blog'un görselini sil
            if (!string.IsNullOrEmpty(blog.BlogImage))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");
                string filePath = Path.Combine(uploadsFolder, blog.BlogImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Blog'u veritabanından sil
            var result = await _blogService.DeleteBlogAsync(blogID);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting the blog.");
            }

            return Ok("Blog and associated image deleted successfully.");
        }
    }
}
