using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class MealPhotoViewModel : ObservableObject
{
    private readonly IFoodVisionService _visionService;
    private readonly IApiKeyVaultService _apiKeyVault;
    private readonly IRecordRepository<Meal> _mealRepository;
    private readonly IRecordRepository<MealItem> _mealItemRepository;
    private readonly IFavoriteFoodService _favoriteFoodService;

    public ObservableCollection<EditableFoodItem> Items { get; } = new();

    [ObservableProperty]
    private string? photoPath;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public bool IsCaptureSupported => MediaPicker.Default.IsCaptureSupported;

    public bool HasPhoto => !string.IsNullOrEmpty(PhotoPath);

    partial void OnPhotoPathChanged(string? value) => OnPropertyChanged(nameof(HasPhoto));

    public MealPhotoViewModel(
        IFoodVisionService visionService,
        IApiKeyVaultService apiKeyVault,
        IRecordRepository<Meal> mealRepository,
        IRecordRepository<MealItem> mealItemRepository,
        IFavoriteFoodService favoriteFoodService)
    {
        _visionService = visionService;
        _apiKeyVault = apiKeyVault;
        _mealRepository = mealRepository;
        _mealItemRepository = mealItemRepository;
        _favoriteFoodService = favoriteFoodService;
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            await StorePhotoAsync(photo);
        }
        catch (FeatureNotSupportedException)
        {
            ErrorMessage = "Camera capture isn't supported on this device.";
        }
        catch (PermissionException)
        {
            ErrorMessage = "Camera permission was denied.";
        }
    }

    [RelayCommand]
    private async Task ChoosePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            await StorePhotoAsync(photo);
        }
        catch (PermissionException)
        {
            ErrorMessage = "Photo library permission was denied.";
        }
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (string.IsNullOrEmpty(PhotoPath))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        var providerLabel = await GetActiveProviderLabelAsync();
        try
        {
            var imageBytes = await File.ReadAllBytesAsync(PhotoPath);
            var mediaType = PhotoPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";

            var estimates = await _visionService.AnalyzeMealPhotoAsync(imageBytes, mediaType, CancellationToken.None);

            Items.Clear();
            foreach (var estimate in estimates)
            {
                Items.Add(new EditableFoodItem
                {
                    Name = estimate.Name,
                    CarbsGrams = estimate.CarbsGrams,
                    GlycemicIndex = estimate.GlycemicIndex
                });
            }

            if (estimates.Count == 0)
            {
                ErrorMessage = $"[{providerLabel}] No food items found in that photo. Try another.";
            }
        }
        catch (FoodVisionException ex)
        {
            ErrorMessage = $"[{providerLabel}] {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<string> GetActiveProviderLabelAsync()
    {
        var providerId = await _apiKeyVault.GetSelectedProviderAsync();
        return AiProviders.All.FirstOrDefault(p => p.Id == providerId)?.DisplayName ?? providerId;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Items.Count == 0)
        {
            return;
        }

        var resolved = new List<FavoriteFood>();
        foreach (var item in Items)
        {
            var favorite = await _favoriteFoodService.RecordUsageAsync(item.Name, item.CarbsGrams, item.GlycemicIndex);
            resolved.Add(favorite);
        }

        var meal = new Meal
        {
            TimestampUtc = DateTime.UtcNow,
            Description = string.Join(", ", resolved.Select(r => r.Name)),
            CarbsGrams = resolved.Sum(r => r.CarbsGrams),
            PhotoPath = PhotoPath
        };
        await _mealRepository.SaveAsync(meal);

        foreach (var favorite in resolved)
        {
            await _mealItemRepository.SaveAsync(new MealItem
            {
                MealId = meal.Id,
                Name = favorite.Name,
                CarbsGrams = favorite.CarbsGrams,
                GlycemicIndex = favorite.GlycemicIndex
            });
        }

        PhotoPath = null;
        Items.Clear();
        ErrorMessage = null;
    }

    private async Task StorePhotoAsync(FileResult? photo)
    {
        if (photo is null)
        {
            return;
        }

        var folder = Path.Combine(FileSystem.CacheDirectory, "meal-photos");
        Directory.CreateDirectory(folder);
        var extension = Path.GetExtension(photo.FileName) is { Length: > 0 } ext ? ext : ".jpg";
        var destinationPath = Path.Combine(folder, $"{Guid.NewGuid()}{extension}");

        await using var sourceStream = await photo.OpenReadAsync();
        await using var destinationStream = File.Create(destinationPath);
        await sourceStream.CopyToAsync(destinationStream);

        PhotoPath = destinationPath;
        Items.Clear();
        ErrorMessage = null;
    }
}
