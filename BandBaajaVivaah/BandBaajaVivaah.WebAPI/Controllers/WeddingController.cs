using BandBaaajaVivaah.Data.Models;
using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Mvc;

namespace BandBaajaVivaah.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeddingController : ControllerBase
    {
        private readonly IWeddingService _weddingService;

        public WeddingController(IWeddingService weddingService)
        {
            _weddingService = weddingService;
        }

        // GET: api/weddings/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeddingById(int id)
        {
            var wedding = await _weddingService.GetWeddingByIdAsync(id);
            if (wedding == null)
            {
                return NotFound();
            }
            return Ok(wedding);
        }

        // POST: api/weddings
        [HttpPost]
        public async Task<IActionResult> CreateWedding([FromBody] Wedding newWedding)
        {
            if (!ModelState.IsValid || newWedding == null)
            {
                return BadRequest(ModelState);
            }
            var createdWedding = await _weddingService.CreateWeddingAsync(newWedding);
            return CreatedAtAction(nameof(GetWeddingById), new { id = createdWedding.WeddingId }, createdWedding);
        }
    }
}
