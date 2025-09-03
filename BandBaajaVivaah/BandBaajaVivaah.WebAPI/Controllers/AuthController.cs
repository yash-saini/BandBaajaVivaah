using BandBaajaVivaah.Services;
using BandBaajaVivaah.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BandBaajaVivaah.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")] // POST: api/auth/register
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            // The [ApiController] attribute handles model state validation automatically
            // so we don't need to check if(ModelState.IsValid)

            var newUser = await _userService.RegisterUserAsync(
                registerDto.FullName,
                registerDto.Email,
                registerDto.Password);

            // We don't return the full user object, especially the password hash
            // For now, returning a simple Ok is fine.
            return Ok(new { Message = "User registered successfully" });
        }
    }
}