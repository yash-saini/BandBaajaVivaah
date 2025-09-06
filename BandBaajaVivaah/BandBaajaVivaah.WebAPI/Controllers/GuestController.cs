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

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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
                var userId = GetCurrentUserId();
                var createdGuest = await _guestService.CreateGuestAsync(guestDto, userId);
                return Ok(createdGuest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}