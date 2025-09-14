using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BandBaajaVivaah.Api.Controllers
{

    [Authorize] // Protects all endpoints in this controller
    [Route("api/[controller]")]
    [ApiController]
    public class WeddingsController : ControllerBase
    {
        private readonly IWeddingService _weddingService;

        public WeddingsController(IWeddingService weddingService)
        {
            _weddingService = weddingService;
        }

        // Helper method to get the logged-in user's ID from their token
        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // This should not fail if the [Authorize] attribute is working correctly
            return int.Parse(userIdValue);
        }

        // GET: api/weddings
        // Gets all weddings for the currently logged-in user
        [HttpGet]
        public async Task<IActionResult> GetMyWeddings()
        {
            var userId = GetCurrentUserId();
            var weddings = await _weddingService.GetWeddingsForUserAsync(userId);
            return Ok(weddings);
        }

        // GET: api/weddings/5
        // Gets a specific wedding by its ID, but only if it belongs to the logged-in user
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeddingById(int id)
        {
            var userId = GetCurrentUserId();
            var wedding = await _weddingService.GetWeddingByIdAsync(id, userId);

            if (wedding == null)
            {
                // Returns 404 Not Found if the wedding doesn't exist OR the user doesn't own it
                return NotFound();
            }

            return Ok(wedding);
        }

        // POST: api/weddings
        // Creates a new wedding and assigns it to the currently logged-in user
        [HttpPost]
        public async Task<IActionResult> CreateWedding([FromBody] CreateWeddingDto weddingDto)
        {
            var userId = GetCurrentUserId();
            var createdWedding = await _weddingService.CreateWeddingAsync(weddingDto, userId);

            return CreatedAtAction(nameof(GetWeddingById), new { id = createdWedding.WeddingID }, createdWedding);
        }

        // DELETE: api/weddings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWedding(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _weddingService.DeleteWeddingAsync(id, userId);
            if (!success)
            {
                return NotFound(); // Wedding not found or user does not own it
            }
            return NoContent(); // Successfully deleted
        }

        // PUT: api/weddings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWedding(int id, [FromBody] CreateWeddingDto weddingDto)
        {
            var userId = GetCurrentUserId();
            var success = await _weddingService.UpdateWeddingAsync(id, weddingDto, userId);
            if (!success)
            {
                return NotFound(); // Wedding not found or user does not own it
            }
            return NoContent(); // Successfully updated
        }
    }
}