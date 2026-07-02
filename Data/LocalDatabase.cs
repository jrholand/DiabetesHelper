using DiabetesHelper.Models;
using SQLite;

namespace DiabetesHelper.Data;

public class LocalDatabase
{
    private readonly SQLiteAsyncConnection _connection;
    private bool _initialized;

    public LocalDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "diabeteshelper.db3");
        _connection = new SQLiteAsyncConnection(dbPath);
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (!_initialized)
        {
            await _connection.CreateTableAsync<GlucoseReading>();
            await _connection.CreateTableAsync<InsulinDose>();
            await _connection.CreateTableAsync<Meal>();
            await _connection.CreateTableAsync<MealItem>();
            await _connection.CreateTableAsync<FavoriteFood>();
            _initialized = true;
        }

        return _connection;
    }
}
