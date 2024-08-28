using BAL;
using DAL.Models;
using DAL.Models.DTO.Account;
using DAL.Models.HelperModels;
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
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly BlogService _blogService;
        private readonly RoleService _roleService;

        public AccountController(UserManager<AppUser> userManager, IConfiguration configuration, EmailService emailService,BlogService blogService,RoleService roleService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _blogService = blogService;
            _roleService = roleService;

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            UserBlogs userWithBlogs = new UserBlogs            {
                User = user
            };

            // Include blogs if the user is an Admin or Writer
            if (await _roleService.CheckUserRole(userId,"Admin") || await _roleService.CheckUserRole(userId, "Writer"))
            {
                IEnumerable<Blog> blogs = await _blogService.GetAllUserBlogsAsync(user.Id);
                userWithBlogs.Blogs = blogs;
            }

            return Ok(userWithBlogs);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserIdFromToken();

            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { Errors = errors });
            }

            return Ok("Password updated successfully.");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestEmailChange([FromBody] ChangeEmailDto dto)
        {
            var userId = GetUserIdFromToken();
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordCheck)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid password." } });
            }

            // Check if the new email is already in use
            var existingUser = await _userManager.FindByEmailAsync(dto.NewEmail);
            if (existingUser != null)
            {
                return BadRequest(new { Errors = new List<string> { "The new email address is already in use." } });
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);
            await _emailService.SendEmail(token, user.Email, "emailChange", dto.NewEmail);

            return Ok("Email change request sent.");
        }


        [HttpGet]
        public async Task<IActionResult> ChangeEmail([FromQuery] string token, [FromQuery] string oldEmail, [FromQuery] string newEmail)
        {
            var user = await _userManager.FindByEmailAsync(oldEmail);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description).ToList() });
            }

            return Ok("Email updated successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> SendEmailConfirmationMail(string userId = null)
        {
            try
            {
                userId ??= GetUserIdFromToken();
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new ApiResponse { Success = false, Message = "User not found." });
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmail(token, user.Email, "emailConfirm");
                return Ok(new ApiResponse { Success = true, Message = "Email confirmation sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while sending the email." });
            }
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
