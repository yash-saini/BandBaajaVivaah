using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BandBaajaVivaah.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _guestService;

        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
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

        [HttpGet("wedding/{weddingId}")]
        public async Task<IActionResult> GetGuestsForWedding(int weddingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var guests = await _guestService.GetGuestsByWeddingIdAsync(weddingId, userId);
                return Ok(guests);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGuest([FromBody] CreateGuestDto guestDto)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    var createdGuest = await _guestService.CreateGuestAsAdminAsync(guestDto);
                    return Ok(createdGuest);
                }
                else
                {
                    var userId = GetCurrentUserId();
                    var createdGuest = await _guestService.CreateGuestAsync(guestDto, userId);
                    return Ok(createdGuest);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{guestId}")]
        public async Task<IActionResult> UpdateGuest(int guestId, [FromBody] CreateGuestDto guestDto)
        {
            bool success;
            if (User.IsInRole("Admin"))
            {
                success = await _guestService.UpdateGuestAsAdminAsync(guestId, guestDto);
            }
            else
            {
                var userId = GetCurrentUserId();
                success = await _guestService.UpdateGuestAsync(guestId, guestDto, userId);
            }

            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{guestId}")]
        public async Task<IActionResult> DeleteGuest(int guestId)
        {
            bool success;
            if (User.IsInRole("Admin"))
            {
                success = await _guestService.DeleteGuestAsAdminAsync(guestId);
            }
            else
            {
                var userId = GetCurrentUserId();
                success = await _guestService.DeleteGuestAsync(guestId, userId);
            }

            return success ? NoContent() : NotFound();
        }
    }
}