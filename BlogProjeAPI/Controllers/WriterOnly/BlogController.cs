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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;



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


        [HttpGet]
        [AllowAnonymous]

        public async Task<IActionResult> GetAllActiveBlogs()
        {
            IEnumerable<Blog> blogs = await _blogService.GetAllActiveBlogsAsync();
            return Ok(blogs);
        }

        [HttpGet("{blogID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogByID(Guid blogID)
        {
            var blog = await _blogService.GetBlogByIDAsync(blogID);

            return Ok(blog);
        }

        [HttpGet("{blogID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveBlogByID(Guid blogID)
        {
            var blog = await _blogService.GetActiveBlogByIDAsync(blogID);
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

            var validationResult = ValidateImages(blogDto.BlogImages);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            // Save images to storage
            var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Images");
            Directory.CreateDirectory(imageDirectory);

            foreach (var image in blogDto.BlogImages)
            {
                var fileName = $"{Guid.NewGuid()}{image.BlogImageExtension}";
                var filePath = Path.Combine(imageDirectory, fileName);
                image.BlogImageName = fileName;

                await SaveImageAsync(image.Base64Image, filePath);
            }

            var result = await _blogService.CreateBlogAsync(blogDto, user);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding the blog.");
            }

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

            

            
            var validationResult = ValidateImages(blogDto.BlogImages);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
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
                var fileName = $"{Guid.NewGuid()}{image.BlogImageExtension}";
                var filePath = Path.Combine(imageDirectory, fileName);
                image.BlogImageName = fileName;
                await SaveImageAsync(image.Base64Image, filePath);
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
            string deneme = "123521521521";

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

        private (bool IsValid, string ErrorMessage) ValidateImages(IEnumerable<BlogImageDTO> images)
        {
            const long maxFileSize = 3 * 1024 * 1024; // 3 MB in bytes

            if(images.Count() == 0)
            {
                return (false, "You need to select a image");
            }
            if (images.Count() > 5)
            {
                return (false, "You can upload a maximum of 5 images.");

            }
            foreach (var image in images)
            {
                try
                {
                    var imageBytes = Convert.FromBase64String(image.Base64Image);



                    if (imageBytes.Length > maxFileSize)
                    {
                        return (false, $"Each image must be smaller than 3 MB. An image is too large.");
                    }

                    using (var imageStream = new MemoryStream(imageBytes))
                    {
                        using (var img = Image.Load<Rgba32>(imageStream))
                        {
                            // Additional image processing if necessary
                        }
                    }
                }
                catch
                {
                    return (false, "One or more images are not valid.");
                }
            }

            return (true, null);
        }

        private async Task SaveImageAsync(string base64Image, string filePath)
        {
            var imageBytes = Convert.FromBase64String(base64Image);
            await using (var imageStream = new MemoryStream(imageBytes))
            {
                using (var img = Image.Load<Rgba32>(imageStream))
                {
                    await img.SaveAsync(filePath);
                }
            }
        }
    }
}
