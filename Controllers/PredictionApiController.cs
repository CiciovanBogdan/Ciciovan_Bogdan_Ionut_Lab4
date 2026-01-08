using Ciciovan_Bogdan_Ionut_Lab4.Data;
using Ciciovan_Bogdan_Ionut_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ciciovan_Bogdan_Ionut_Lab4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredictionHistory>>> GetAllPredictions()
        {
            var predictions = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (predictions == null || !predictions.Any())
            {
                return NotFound(new { message = "Nu exista predictii in baza de date." });
            }

            return Ok(predictions);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrediction(int id)
        {
            var prediction = await _context.PredictionHistories.FindAsync(id);

            if (prediction == null)
            {
                return NotFound(new { message = $"Predictia cu ID-ul {id} nu a fost gasita." });
            }

            _context.PredictionHistories.Remove(prediction);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Predictia cu ID-ul {id} a fost stearsa cu succes." });
        }
    }
}