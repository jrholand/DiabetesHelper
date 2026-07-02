namespace DiabetesHelper.Views;

internal static class SettingsNavigation
{
    public static async Task ShowAsync()
    {
        var services = Application.Current!.Handler!.MauiContext!.Services;
        var settingsPage = services.GetRequiredService<SettingsPage>();
        await Shell.Current.Navigation.PushModalAsync(settingsPage);
    }
}
