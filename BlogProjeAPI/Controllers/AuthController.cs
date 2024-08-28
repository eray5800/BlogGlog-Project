using BAL;
using DAL.Models;
using DAL.Models.HelperModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private readonly RoleService _roleService;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, EmailService emailService, RoleService roleService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailService = emailService;
        _roleService = roleService;
    }
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterDTO model)
    {
        if (ModelState.IsValid)
        {
            // Check if the email is already in use
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                var errors = new List<ErrorModel>
            {
                new ErrorModel
                {
                    Code = "DuplicateEmail",
                    Description = "This email address is already in use. Please choose another one."
                }
            };
                return BadRequest(errors);
            }

            var user = new AppUser { UserName = model.UserName, Email = model.Email, IsActive = true };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _roleService.AssignRoleAsync(user.Id, "User");
                    return RedirectToAction("SendEmailConfirmationMail", "Account", new { userID = user.Id });
                }

                var errors = result.Errors
                    .Select(e => new ErrorModel { Code = e.Code, Description = e.Description })
                    .ToList();

                return BadRequest(errors);
            }
            catch (Exception ex)
            {
                var errors = new List<ErrorModel>
            {
                new ErrorModel
                {
                    Code = "ServerError",
                    Description = "An unexpected error occurred. Please try again later."
                }
            };
                return BadRequest(errors);
            }
        }

        var modelErrors = ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => new ErrorModel { Code = "ModelStateError", Description = e.ErrorMessage })
            .ToList();

        return BadRequest(modelErrors);
    }




    [HttpGet("verifyemail")]
    
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return BadRequest("Invalid email verification request");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok("Email verified successfully");
        }

        return BadRequest("Email verification failed");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginDTO model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            return Unauthorized("Invalid login attempt");
        }
        return BadRequest("Invalid model state");
    }

    private async Task<string> GenerateJwtToken(AppUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, user.UserName)
        };

        // Rolleri claim'lere ekle
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }




}
