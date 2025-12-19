using Ciciovan_Bogdan_Ionut_Lab4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;

namespace Ciciovan_Bogdan_Ionut_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Price(PricePredictionModel.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();

            // Create prediction engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PricePredictionModel.ModelInput, PricePredictionModel.ModelOutput>(mlModel);

            // Try model on sample data to predict fair price
            PricePredictionModel.ModelOutput result = predEngine.Predict(input);

            ViewBag.Price = result.Score;

            return View(input);
        }

        [HttpGet]
        public IActionResult Time()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Time(TimePredictionModel.ModelInput input)
        {
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
    }
}
