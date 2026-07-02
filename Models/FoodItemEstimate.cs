namespace DiabetesHelper.Models;

// Not a SQLite model - a plain result DTO returned by IFoodVisionService before
// any favorites-cache resolution or persistence happens.
public class FoodItemEstimate
{
    public string Name { get; set; } = string.Empty;

    public double CarbsGrams { get; set; }

    public int? GlycemicIndex { get; set; }
}
