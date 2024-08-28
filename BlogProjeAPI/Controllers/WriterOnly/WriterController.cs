using BAL;
using DAL.Models;
using DAL.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BlogProjeAPI.Controllers.WriterOnly
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WriterController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly WriterRequestService _writerRequestService;

        public WriterController(IConfiguration configuration, UserManager<AppUser> userManager, WriterRequestService writerRequestService)
        {
            _configuration = configuration;
            _userManager = userManager;
            _writerRequestService = writerRequestService;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddWriterRequest(WriterRequestDTO requestDTO)
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!user.EmailConfirmed)
            {
                // Return HTTP 403 Forbidden with a custom message
                return StatusCode(403, "Please confirm your email");
            }

            var result = await _writerRequestService.AddWriterRequestAsync(requestDTO, user);
            if (result)
            {
                return Ok("Writer request submitted successfully.");
            }

            return BadRequest("Failed to submit writer request.");
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
