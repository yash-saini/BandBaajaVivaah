using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BandBaajaVivaah.WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWeddingService _weddingService;

        public AdminController(IUserService userService, IWeddingService weddingService)
        {
            _userService = userService;
            _weddingService = weddingService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new InvalidOperationException("User ID claim is missing.");
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateRoleDto updateRoleDto)
        {
            var currentUserId = GetCurrentUserId();
            if (userId == currentUserId)
            {
                return BadRequest(new { Message = "Cannot change your own role." });
            }

            var success = await _userService.UpdateUserRoleAsync(userId, updateRoleDto.Role);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (userId == currentUserId)
            {
                return BadRequest(new { Message = "Cannot delete your own account." });
            }

            var success = await _userService.DeleteUserAsync(userId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("weddings")]
        public async Task<IActionResult> GetAllWeddings()
        {
            var weddings = await _weddingService.GetAllWeddingsAsync();
            return Ok(weddings);
        }
    }

    public class UpdateRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}

