using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly IFavoriteFoodService _favoriteFoodService;

    public ObservableCollection<FavoriteFood> Favorites { get; } = new();

    public FavoritesViewModel(IFavoriteFoodService favoriteFoodService)
    {
        _favoriteFoodService = favoriteFoodService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Favorites.Clear();
        var items = await _favoriteFoodService.GetAllAsync();
        foreach (var item in items.OrderByDescending(f => f.UseCount).ThenByDescending(f => f.LastUsedUtc))
        {
            Favorites.Add(item);
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(FavoriteFood item)
    {
        await _favoriteFoodService.DeleteAsync(item);
        Favorites.Remove(item);
    }
}
