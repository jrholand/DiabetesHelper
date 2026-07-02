using DiabetesHelper.Data;
using DiabetesHelper.Models;
using SQLite;

namespace DiabetesHelper.Services;

public class LocalApiKeyVaultService : IApiKeyVaultService
{
    private const string ActiveEntryIdPreferenceKey = "active_api_key_entry_id";
    private const int NoActiveEntry = -1;

    private readonly LocalDatabase _database;

    public LocalApiKeyVaultService(LocalDatabase database)
    {
        _database = database;
    }

    public async Task<List<ApiKeyEntry>> GetHistoryAsync(string provider)
    {
        var connection = await _database.GetConnectionAsync();
        var entries = await connection.Table<ApiKeyEntry>().Where(e => e.Provider == provider).ToListAsync();

        var activeId = GetActiveEntryId();
        foreach (var entry in entries)
        {
            entry.IsActive = entry.Id == activeId;
        }

        return entries.OrderByDescending(e => e.EnteredUtc).ToList();
    }

    public async Task<ApiKeyEntry?> GetActiveEntryAsync()
    {
        var activeId = GetActiveEntryId();
        if (activeId == NoActiveEntry)
        {
            return null;
        }

        var connection = await _database.GetConnectionAsync();
        var entry = await connection.Table<ApiKeyEntry>().Where(e => e.Id == activeId).FirstOrDefaultAsync();
        if (entry is not null)
        {
            entry.IsActive = true;
        }

        return entry;
    }

    public async Task<string?> GetActiveKeyAsync(string provider)
    {
        var active = await GetActiveEntryAsync();
        return active is not null && active.Provider == provider ? active.Key : null;
    }

    public async Task<ApiKeyEntry> AddKeyAsync(string provider, string key)
    {
        var connection = await _database.GetConnectionAsync();
        var newEntry = new ApiKeyEntry
        {
            Provider = provider,
            Key = key,
            EnteredUtc = DateTime.UtcNow
        };
        await connection.InsertAsync(newEntry);
        return newEntry;
    }

    public async Task ActivateAsync(int entryId)
    {
        var connection = await _database.GetConnectionAsync();
        var entry = await connection.Table<ApiKeyEntry>().Where(e => e.Id == entryId).FirstOrDefaultAsync();
        if (entry is null)
        {
            throw new InvalidOperationException($"No stored key with id {entryId}.");
        }

        Preferences.Default.Set(ActiveEntryIdPreferenceKey, entryId);
    }

    private static int GetActiveEntryId() => Preferences.Default.Get(ActiveEntryIdPreferenceKey, NoActiveEntry);
}
