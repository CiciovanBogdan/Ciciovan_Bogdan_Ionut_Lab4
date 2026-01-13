using Ciciovan_Bogdan_Ionut_Lab4;
using Ciciovan_Bogdan_Ionut_Lab4.Data;
using Ciciovan_Bogdan_Ionut_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        public async Task<IActionResult> History(
            string? paymentType,
            float? minPrice,
            float? maxPrice,
            string? sortOrder,
            string? startDate,
            string? endDate,
            string? sortDate)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
            {
                query = query.Where(p => p.CreatedAt >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
            {
                query = query.Where(p => p.CreatedAt <= end);
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                _ => query.OrderBy(p=> p.PredictedPrice) //sortare default
            };

            query = sortDate switch
            {
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.CreatedAt) //sortare default
            };

            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortOrder = sortOrder;

            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentSortDate = sortDate;

            var result = await query.ToListAsync();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);

            // 1. Numărul total de predicții
            var totalPredictions = await query.CountAsync();

            // 2. Preț mediu per tip de plată + număr de predicții per tip
            var paymentTypeStats = await query
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    PaymentType = g.Key,
                    AveragePrice = g.Average(x => x.PredictedPrice),
                    Count = g.Count()
                })
                .ToListAsync();

            // 3. Distribuția prețurilor pe intervale (buckets)
            var allPredictions = await query
                .Select(p => p.PredictedPrice)
                .ToListAsync();

            var buckets = new List<PriceBucketStat>
            {
                new PriceBucketStat { Label = "0 - 10" },
                new PriceBucketStat { Label = "10 - 20" },
                new PriceBucketStat { Label = "20 - 30" },
                new PriceBucketStat { Label = "30 - 50" },
                new PriceBucketStat { Label = "> 50" }
            };

            foreach (var price in allPredictions)
            {
                if (price < 10)
                    buckets[0].Count++;
                else if (price < 20)
                    buckets[1].Count++;
                else if (price < 30)
                    buckets[2].Count++;
                else if (price < 50)
                    buckets[3].Count++;
                else
                    buckets[4].Count++;
            }

            // 4. Construim ViewModel-ul
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(vm);
        }
    }
}