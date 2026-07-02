using DiabetesHelper.Data;
using LiteDB;

namespace DiabetesHelper.Services;

public class LiteDbRecordRepository<T> : IRecordRepository<T> where T : class, new()
{
    private readonly LiteDbContext _context;

    public LiteDbRecordRepository(LiteDbContext context)
    {
        _context = context;
    }

    private ILiteCollection<T> Collection => _context.Database.GetCollection<T>(typeof(T).Name);

    public Task<List<T>> GetAllAsync()
    {
        return Task.FromResult(Collection.FindAll().ToList());
    }

    public Task SaveAsync(T item)
    {
        Collection.Upsert(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T item)
    {
        var id = _context.Database.Mapper.ToDocument(item)["_id"];
        Collection.Delete(id);
        return Task.CompletedTask;
    }
}
