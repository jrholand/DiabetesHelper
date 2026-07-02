using DiabetesHelper.Data;
using DiabetesHelper.Services;
using DiabetesHelper.ViewModels;
using DiabetesHelper.Views;
using Microsoft.Extensions.Logging;

namespace DiabetesHelper;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<LiteDbContext>();
        builder.Services.AddSingleton(typeof(IRecordRepository<>), typeof(LiteDbRecordRepository<>));
        builder.Services.AddSingleton<IFavoriteFoodService, LiteDbFavoriteFoodService>();
        builder.Services.AddSingleton<IApiKeyStore, SecureStorageApiKeyStore>();

        builder.Services.AddHttpClient<AnthropicFoodVisionService>();
        builder.Services.AddSingleton<IFoodVisionService>(sp => sp.GetRequiredService<AnthropicFoodVisionService>());

        builder.Services.AddTransient<GlucoseLogViewModel>();
        builder.Services.AddTransient<GlucoseLogPage>();
        builder.Services.AddTransient<InsulinLogViewModel>();
        builder.Services.AddTransient<InsulinLogPage>();
        builder.Services.AddTransient<MealLogViewModel>();
        builder.Services.AddTransient<MealLogPage>();
        builder.Services.AddTransient<MealPhotoViewModel>();
        builder.Services.AddTransient<MealPhotoPage>();
        builder.Services.AddTransient<FavoritesViewModel>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AboutViewModel>();
        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<EditGlucoseReadingViewModel>();
        builder.Services.AddTransient<EditGlucoseReadingPage>();
        builder.Services.AddTransient<EditInsulinDoseViewModel>();
        builder.Services.AddTransient<EditInsulinDosePage>();
        builder.Services.AddTransient<EditMealViewModel>();
        builder.Services.AddTransient<EditMealPage>();

        return builder.Build();
    }
}
