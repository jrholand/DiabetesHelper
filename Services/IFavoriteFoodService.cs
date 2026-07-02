using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// Dedicated seam for the favorites cache - deliberately not IRecordRepository<FavoriteFood>,
// because the operations here (normalized-name upsert, use-count increment, top-50 trim) are
// domain logic specific to this cache, not generic CRUD.
public interface IFavoriteFoodService
{
    Task<List<FavoriteFood>> GetAllAsync();

    // Looks up an existing favorite by case-insensitive/trimmed name match. If found, increments
    // UseCount, updates LastUsedUtc, and returns the EXISTING stored CarbsGrams/GlycemicIndex
    // (ignoring the estimate's values) - this is the reuse behavior. If not found, inserts a new
    // favorite from the estimate with UseCount = 1, trims the table to the top 50, and returns it.
    Task<FavoriteFood> RecordUsageAsync(string name, double carbsGrams, int? glycemicIndex);

    Task DeleteAsync(FavoriteFood item);
}
