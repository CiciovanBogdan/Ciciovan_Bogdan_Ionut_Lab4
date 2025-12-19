using Ciciovan_Bogdan_Ionut_Lab4;
using Ciciovan_Bogdan_Ionut_Lab4.Data;
using Ciciovan_Bogdan_Ionut_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Ciciovan_Bogdan_Ionut_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;

        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Price(PricePredictionModel.ModelInput input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            // Load the model
            MLContext mlContext = new MLContext();

            // Create prediction engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PricePredictionModel.ModelInput, PricePredictionModel.ModelOutput>(mlModel);

            // Try model on sample data to predict fair price
            PricePredictionModel.ModelOutput result = predEngine.Predict(input);

            ViewBag.Price = result.Score;

            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };

            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            return View(input);
        }

        [HttpGet]
        public IActionResult Time()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Time(TimePredictionModel.ModelInput input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            // Load the model
            MLContext mlContext = new MLContext();

            // Create prediction engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"TimePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<TimePredictionModel.ModelInput, TimePredictionModel.ModelOutput>(mlModel);

            // Try model on sample data to predict trip time
            TimePredictionModel.ModelOutput result = predEngine.Predict(input);

            ViewBag.Time = result.Score;

            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            
            return View(history);
        }
    }
}