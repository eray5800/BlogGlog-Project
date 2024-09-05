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

namespace BlogProjeAPI.Controllers.WriterOnly
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin,Writer")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public BlogController(IBlogService blogService, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _blogService = blogService;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBlogs()
        {
            IEnumerable<Blog> blogs = await _blogService.GetAllBlogsAsync();
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
        public async Task<IActionResult> Search([FromQuery] string text)
        {
            var blogs = await _blogService.Search(text);
            if (blogs == null || !blogs.Any())
            {
                return NotFound("No blogs found.");
            }
            return Ok(blogs);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CategorySearch([FromQuery] string text)
        {
            var blogs = await _blogService.SearchBlogCategoryAsync(text);
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

            // Validate image files count and size
            if (blogDto.BlogImages.Count > 5)
            {
                return BadRequest("You can upload a maximum of 5 images.");
            }

            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes
            foreach (var image in blogDto.BlogImages)
            {
                var imageBytes = Convert.FromBase64String(image.Base64Image);
                if (imageBytes.Length > maxFileSize)
                {
                    return BadRequest($"Each image must be smaller than 3 MB. An image is too large.");
                }
            }

            var userId = GetUserIdFromToken();
            System.Diagnostics.Debug.WriteLine($"User ID from token: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Save images to storage
            var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");

            // Create directory if it doesn't exist
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            foreach (var image in blogDto.BlogImages)
            {
                var imageBytes = Convert.FromBase64String(image.Base64Image);
                var fileName = string.Concat(Guid.NewGuid(), image.BlogImageExtension);
                var filePath = Path.Combine(imageDirectory, fileName);
                image.BlogImageName = fileName;
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
            }

            var result = await _blogService.CreateBlogAsync(blogDto, user);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding the blog.");
            }

            System.Diagnostics.Debug.WriteLine("Blog added successfully.");
            return Ok("Blog added successfully.");
        }



        [HttpPut("{blogID}")]
        public async Task<IActionResult> UpdateBlog([FromRoute] Guid blogID, [FromBody] BlogDTO blogDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            var existingBlog = await _blogService.GetBlogByIDAsync(blogID);
            if (existingBlog == null)
            {
                return NotFound("Blog not found.");
            }

            var userId = GetUserIdFromToken();
            if (existingBlog.User.Id != userId)
            {
                return Unauthorized("You are not authorized to update this blog.");
            }

            // Validate image files count and size
            if (blogDto.BlogImages.Count > 5)
            {
                return BadRequest("You can upload a maximum of 5 images.");
            }

            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes
            foreach (var image in blogDto.BlogImages)
            {
                var imageBytes = Convert.FromBase64String(image.Base64Image);
                if (imageBytes.Length > maxFileSize)
                {
                    return BadRequest($"Each image must be smaller than 3 MB. An image is too large.");
                }
            }

            // Save images to storage
            var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");

            // Create directory if it doesn't exist
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }
            foreach (var image in existingBlog.BlogImages)
            {
                var filePath = Path.Combine(imageDirectory, image.BlogImageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Save new images to storage
            foreach (var image in blogDto.BlogImages)
            {
                var imageBytes = Convert.FromBase64String(image.Base64Image);
                var fileName = $"{Guid.NewGuid()}.{image.BlogImageExtension}";
                var filePath = Path.Combine(imageDirectory, fileName);
                image.BlogImageName = fileName;
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
            }

            var result = await _blogService.UpdateBlogAsync(blogID, blogDto);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating the blog.");
            }

            return Ok("Blog updated successfully.");
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

        [HttpDelete("{blogID}")]
        public async Task<IActionResult> DeleteBlog(Guid blogID)
        {
            var blog = await _blogService.GetBlogByIDAsync(blogID);
            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            var userId = GetUserIdFromToken();
            if (blog.User.Id != userId)
            {
                return Unauthorized("You are not authorized to delete this blog.");
            }

            // Delete images from storage
            var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");
            foreach (var image in blog.BlogImages)
            {
                var filePath = Path.Combine(imageDirectory, image.BlogImageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Delete the blog entry
            var result = await _blogService.DeleteBlogAsync(blogID);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting the blog.");
            }

            return Ok("Blog and associated images deleted successfully.");
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
                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Return user ID
            }
            catch
            {
                return null;
            }
        }
    }
}
