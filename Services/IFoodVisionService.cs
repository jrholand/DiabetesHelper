using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

public interface IFoodVisionService
{
    // Throws FoodVisionException on a missing/invalid API key, an HTTP failure, or an
    // unparseable response. An empty result is a valid outcome (no food identified), not an error.
    Task<List<FoodItemEstimate>> AnalyzeMealPhotoAsync(byte[] imageBytes, string mediaType, CancellationToken cancellationToken);
}
