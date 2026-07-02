namespace DiabetesHelper.Views;

internal static class AboutNavigation
{
    public static async Task ShowAsync()
    {
        var services = Application.Current!.Handler!.MauiContext!.Services;
        var aboutPage = services.GetRequiredService<AboutPage>();
        await Shell.Current.Navigation.PushModalAsync(aboutPage);
    }
}
