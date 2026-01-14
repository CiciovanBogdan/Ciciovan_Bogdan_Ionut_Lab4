namespace Ciciovan_Bogdan_Ionut_Lab4.Models
{
    public class BatchPredictionViewModel
    {
        public IFormFile? File { get; set; }
        // Rezultatele (predicțiile) pentru fiecare rând din fișier
        public List<PricePredictionModel.ModelOutput>? Predictions { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
