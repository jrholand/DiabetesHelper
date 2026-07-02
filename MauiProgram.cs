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

        builder.Services.AddSingleton<LocalDatabase>();
        builder.Services.AddSingleton(typeof(IRecordRepository<>), typeof(LocalRecordRepository<>));
        builder.Services.AddSingleton<IFavoriteFoodService, LocalFavoriteFoodService>();
        builder.Services.AddSingleton<IApiKeyVaultService, LocalApiKeyVaultService>();

        builder.Services.AddHttpClient<AnthropicFoodVisionService>();
        builder.Services.AddHttpClient<OpenAiFoodVisionService>();
        builder.Services.AddHttpClient<GoogleGeminiFoodVisionService>();
        builder.Services.AddHttpClient<MistralFoodVisionService>();
        builder.Services.AddHttpClient<XaiGrokFoodVisionService>();
        builder.Services.AddSingleton<IFoodVisionService, ActiveProviderFoodVisionService>();

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

        return builder.Build();
    }
}
