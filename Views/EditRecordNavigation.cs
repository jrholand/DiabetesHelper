using DiabetesHelper.Models;

namespace DiabetesHelper.Views;

internal static class EditRecordNavigation
{
    public static async Task ShowGlucoseEditAsync(GlucoseReading record)
    {
        var services = Application.Current!.Handler!.MauiContext!.Services;
        var page = services.GetRequiredService<EditGlucoseReadingPage>();
        page.Load(record);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    public static async Task ShowInsulinEditAsync(InsulinDose record)
    {
        var services = Application.Current!.Handler!.MauiContext!.Services;
        var page = services.GetRequiredService<EditInsulinDosePage>();
        page.Load(record);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    public static async Task ShowMealEditAsync(Meal record)
    {
        var services = Application.Current!.Handler!.MauiContext!.Services;
        var page = services.GetRequiredService<EditMealPage>();
        page.Load(record);
        await Shell.Current.Navigation.PushModalAsync(page);
    }
}
