using DiabetesHelper.Data;
using DiabetesHelper.Models;
using SQLite;

namespace DiabetesHelper.Services;

public class LocalFavoriteFoodService : IFavoriteFoodService
{
    private const int MaxFavorites = 50;

    private readonly LocalDatabase _database;

    public LocalFavoriteFoodService(LocalDatabase database)
    {
        _database = database;
    }

    public async Task<List<FavoriteFood>> GetAllAsync()
    {
        var connection = await _database.GetConnectionAsync();
        return await connection.Table<FavoriteFood>().ToListAsync();
    }

    public async Task<FavoriteFood> RecordUsageAsync(string name, double carbsGrams, int? glycemicIndex)
    {
        var connection = await _database.GetConnectionAsync();
        var normalized = name.Trim().ToLowerInvariant();

        var existing = await connection.Table<FavoriteFood>()
            .Where(f => f.NormalizedName == normalized)
            .FirstOrDefaultAsync();

        if (existing is not null)
        {
            existing.UseCount += 1;
            existing.LastUsedUtc = DateTime.UtcNow;
            await connection.UpdateAsync(existing);
            return existing;
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
        await connection.InsertAsync(favorite);

        await TrimToTopFiftyAsync(connection);

        return favorite;
    }

    public async Task DeleteAsync(FavoriteFood item)
    {
        var connection = await _database.GetConnectionAsync();
        await connection.DeleteAsync(item);
    }

    private static async Task TrimToTopFiftyAsync(SQLiteAsyncConnection connection)
    {
        var all = await connection.Table<FavoriteFood>().ToListAsync();
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
            await connection.DeleteAsync(favorite);
        }
    }
}
