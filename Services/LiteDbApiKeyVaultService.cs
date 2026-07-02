using DiabetesHelper.Data;
using DiabetesHelper.Models;
using LiteDB;

namespace DiabetesHelper.Services;

public class LiteDbApiKeyVaultService : IApiKeyVaultService
{
    private const string ActiveEntryIdPreferenceKey = "active_api_key_entry_id";
    private const int NoActiveEntry = -1;

    private readonly LiteDbContext _context;

    public LiteDbApiKeyVaultService(LiteDbContext context)
    {
        _context = context;
    }

    private ILiteCollection<ApiKeyEntry> Collection => _context.Database.GetCollection<ApiKeyEntry>(nameof(ApiKeyEntry));

    public Task<List<ApiKeyEntry>> GetHistoryAsync(string provider)
    {
        var entries = Collection.Find(e => e.Provider == provider).ToList();

        var activeId = GetActiveEntryId();
        foreach (var entry in entries)
        {
            entry.IsActive = entry.Id == activeId;
        }

        return Task.FromResult(entries.OrderByDescending(e => e.EnteredUtc).ToList());
    }

    public Task<ApiKeyEntry?> GetActiveEntryAsync()
    {
        var activeId = GetActiveEntryId();
        if (activeId == NoActiveEntry)
        {
            return Task.FromResult<ApiKeyEntry?>(null);
        }

        var entry = Collection.FindById(activeId);
        if (entry is not null)
        {
            entry.IsActive = true;
        }

        return Task.FromResult(entry);
    }

    public async Task<string?> GetActiveKeyAsync(string provider)
    {
        var active = await GetActiveEntryAsync();
        return active is not null && active.Provider == provider ? active.Key : null;
    }

    public Task<ApiKeyEntry> AddKeyAsync(string provider, string key)
    {
        var newEntry = new ApiKeyEntry
        {
            Provider = provider,
            Key = key,
            EnteredUtc = DateTime.UtcNow
        };
        Collection.Insert(newEntry);
        return Task.FromResult(newEntry);
    }

    public Task ActivateAsync(int entryId)
    {
        var entry = Collection.FindById(entryId);
        if (entry is null)
        {
            throw new InvalidOperationException($"No stored key with id {entryId}.");
        }

        Preferences.Default.Set(ActiveEntryIdPreferenceKey, entryId);
        return Task.CompletedTask;
    }

    private static int GetActiveEntryId() => Preferences.Default.Get(ActiveEntryIdPreferenceKey, NoActiveEntry);
}
