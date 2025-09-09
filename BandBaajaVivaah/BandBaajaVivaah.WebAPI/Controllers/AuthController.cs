using BandBaaajaVivaah.Data.Models;
using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BandBaajaVivaah.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration) // Add IConfiguration here
        {
            _userService = userService;
            _configuration = configuration; // And here
        }

        [HttpPost("register")] // POST: api/auth/register
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            // First, check if a user with this email already exists
            var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                // Return a 409 Conflict status with a specific message
                return Conflict(new { Message = "A user with this email already exists." });
            }

            var newUser = await _userService.RegisterUserAsync(
                registerDto.FullName,
                registerDto.Email,
                registerDto.Password);

            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")] // POST: api/auth/login
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var user = await _userService.LoginAsync(loginDto.Email, loginDto.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            var token = GenerateJwtToken(user); 
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}