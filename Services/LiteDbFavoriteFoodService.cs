using DiabetesHelper.Data;
using DiabetesHelper.Models;
using LiteDB;

namespace DiabetesHelper.Services;

public class LiteDbFavoriteFoodService : IFavoriteFoodService
{
    private const int MaxFavorites = 50;

    private readonly LiteDbContext _context;

    public LiteDbFavoriteFoodService(LiteDbContext context)
    {
        _context = context;
    }

    private ILiteCollection<FavoriteFood> Collection => _context.Database.GetCollection<FavoriteFood>(nameof(FavoriteFood));

    public Task<List<FavoriteFood>> GetAllAsync()
    {
        return Task.FromResult(Collection.FindAll().ToList());
    }

    public Task<FavoriteFood> RecordUsageAsync(string name, double carbsGrams, int? glycemicIndex)
    {
        var normalized = name.Trim().ToLowerInvariant();

        var existing = Collection.FindOne(f => f.NormalizedName == normalized);

        if (existing is not null)
        {
            existing.UseCount += 1;
            existing.LastUsedUtc = DateTime.UtcNow;
            Collection.Update(existing);
            return Task.FromResult(existing);
        }

        var favorite = new FavoriteFood
        {
            Name = name.Trim(),
            NormalizedName = normalized,
            CarbsGrams = carbsGrams,
            GlycemicIndex = glycemicIndex,
            UseCount = 1,
            LastUsedUtc = DateTime.UtcNow
        };
        Collection.Insert(favorite);

        TrimToTopFifty();

        return Task.FromResult(favorite);
    }

    public Task DeleteAsync(FavoriteFood item)
    {
        Collection.Delete(item.Id);
        return Task.CompletedTask;
    }

    private void TrimToTopFifty()
    {
        var all = Collection.FindAll().ToList();
        if (all.Count <= MaxFavorites)
        {
            return;
        }

        var toDelete = all
            .OrderByDescending(f => f.UseCount)
            .ThenByDescending(f => f.LastUsedUtc)
            .Skip(MaxFavorites);

        foreach (var favorite in toDelete)
        {
            Collection.Delete(favorite.Id);
        }
    }
}
