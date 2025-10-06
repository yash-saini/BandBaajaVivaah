using BandBaajaVivaah.Contracts.DTOs;
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

        [HttpGet("users/{userId}/weddings")]
        public async Task<IActionResult> GetWeddingsForUser(int userId)
        {
            var weddings = await _weddingService.GetWeddingsForUserAsync(userId);
            return Ok(weddings);
        }

        [HttpDelete("weddings/{weddingId}")]
        public async Task<IActionResult> DeleteWedding(int weddingId)
        {
            // We can pass null for ownerUserId to bypass the ownership check for admin
            var success = await _weddingService.DeleteWeddingAsync(weddingId, ownerUserId: null);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("users/{userId}/weddings")]
        public async Task<IActionResult> AddWeddingForUser(int userId, [FromBody] CreateWeddingDto weddingDto)
        {
            // First check if the user exists
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Create the wedding for the specified user
            var wedding = await _weddingService.CreateWeddingAsync(weddingDto, userId);

            return CreatedAtAction(nameof(GetWeddingById), new { weddingId = wedding.WeddingID }, wedding);
        }

        [HttpGet("weddings/{weddingId}")]
        public async Task<IActionResult> GetWeddingById(int weddingId)
        {
            // Admin can view any wedding regardless of ownership
            var wedding = await _weddingService.GetWeddingByIdAsync(weddingId, null);
            if (wedding == null)
            {
                return NotFound();
            }
            return Ok(wedding);
        }

        [HttpPut("weddings/{weddingId}")]
        public async Task<IActionResult> UpdateWedding(int weddingId, [FromBody] CreateWeddingDto weddingDto)
        {
            // Admin can update any wedding
            var success = await _weddingService.UpdateWeddingAsync(weddingId, weddingDto, null);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }

    public class UpdateRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}

