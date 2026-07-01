using DiabetesHelper.Data;

namespace DiabetesHelper.Services;

public class LocalRecordRepository<T> : IRecordRepository<T> where T : class, new()
{
    private readonly LocalDatabase _database;

    public LocalRecordRepository(LocalDatabase database)
    {
        _database = database;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var connection = await _database.GetConnectionAsync();
        return await connection.Table<T>().ToListAsync();
    }

    public async Task SaveAsync(T item)
    {
        var connection = await _database.GetConnectionAsync();
        await connection.InsertOrReplaceAsync(item);
    }

    public async Task DeleteAsync(T item)
    {
        var connection = await _database.GetConnectionAsync();
        await connection.DeleteAsync(item);
    }
}
