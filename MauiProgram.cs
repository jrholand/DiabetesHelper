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

        builder.Services.AddTransient<GlucoseLogViewModel>();
        builder.Services.AddTransient<GlucoseLogPage>();
        builder.Services.AddTransient<InsulinLogViewModel>();
        builder.Services.AddTransient<InsulinLogPage>();
        builder.Services.AddTransient<MealLogViewModel>();
        builder.Services.AddTransient<MealLogPage>();

        return builder.Build();
    }
}
