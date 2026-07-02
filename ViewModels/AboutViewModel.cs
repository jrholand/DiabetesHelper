using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DiabetesHelper.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public string VersionString { get; }
    public string BuildNumber { get; }
    public string BuildDateDisplay { get; }

    public AboutViewModel()
    {
        VersionString = AppInfo.Current.VersionString;
        BuildNumber = AppInfo.Current.BuildString;
        BuildDateDisplay = GetBuildDateDisplay();
    }

    private static string GetBuildDateDisplay()
    {
        var raw = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "BuildDateUtc")?.Value;

        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var buildDateUtc)
            ? buildDateUtc.ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
            : "Unknown";
    }

    [RelayCommand]
    private async Task CloseAsync() => await Shell.Current.Navigation.PopModalAsync();
}
