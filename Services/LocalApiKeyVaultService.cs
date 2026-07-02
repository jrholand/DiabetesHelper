using DiabetesHelper.Data;
using DiabetesHelper.Models;
using SQLite;

namespace DiabetesHelper.Services;

public class LocalApiKeyVaultService : IApiKeyVaultService
{
    private const string SelectedProviderPreferenceKey = "selected_ai_provider";

    private readonly LocalDatabase _database;

    public LocalApiKeyVaultService(LocalDatabase database)
    {
        _database = database;
    }

    public async Task<List<ApiKeyEntry>> GetHistoryAsync(string provider)
    {
        var connection = await _database.GetConnectionAsync();
        var entries = await connection.Table<ApiKeyEntry>().Where(e => e.Provider == provider).ToListAsync();
        return entries.OrderByDescending(e => e.EnteredUtc).ToList();
    }

    public async Task<ApiKeyEntry?> GetActiveEntryAsync(string provider)
    {
        var connection = await _database.GetConnectionAsync();
        return await connection.Table<ApiKeyEntry>()
            .Where(e => e.Provider == provider && e.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetActiveKeyAsync(string provider) =>
        (await GetActiveEntryAsync(provider))?.Key;

    public async Task<ApiKeyEntry> AddKeyAsync(string provider, string key)
    {
        var connection = await _database.GetConnectionAsync();

        var currentlyActive = await connection.Table<ApiKeyEntry>()
            .Where(e => e.Provider == provider && e.IsActive)
            .ToListAsync();
        foreach (var entry in currentlyActive)
        {
            entry.IsActive = false;
            await connection.UpdateAsync(entry);
        }

        var newEntry = new ApiKeyEntry
        {
            Provider = provider,
            Key = key,
            EnteredUtc = DateTime.UtcNow,
            IsActive = true
        };
        await connection.InsertAsync(newEntry);
        return newEntry;
    }

    public async Task ActivateAsync(string provider, int entryId)
    {
        var connection = await _database.GetConnectionAsync();
        var entries = await connection.Table<ApiKeyEntry>().Where(e => e.Provider == provider).ToListAsync();

        if (entries.All(e => e.Id != entryId))
        {
            throw new InvalidOperationException($"No stored key with id {entryId} for provider '{provider}'.");
        }

        foreach (var entry in entries)
        {
            entry.IsActive = entry.Id == entryId;
            await connection.UpdateAsync(entry);
        }
    }

    // Not a secret, so this stays in Preferences rather than the ApiKeyEntry table.
    public Task<string> GetSelectedProviderAsync() =>
        Task.FromResult(Preferences.Default.Get(SelectedProviderPreferenceKey, AiProviders.Anthropic));

    public Task SetSelectedProviderAsync(string provider)
    {
        Preferences.Default.Set(SelectedProviderPreferenceKey, provider);
        return Task.CompletedTask;
    }
}
