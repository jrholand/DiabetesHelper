namespace DiabetesHelper.Services;

// Storage seam: today this is backed by LiteDB (see LiteDbRecordRepository).
// A future cloud-sync store can implement this same interface without
// touching the ViewModels that consume it.
public interface IRecordRepository<T> where T : class, new()
{
    Task<List<T>> GetAllAsync();

    Task SaveAsync(T item);

    Task DeleteAsync(T item);
}
