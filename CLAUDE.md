# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

DiabetesHelper is a .NET MAUI mobile app (Android, iOS, Windows) for logging blood glucose readings, insulin
doses, and meals/carb counts. Data is stored locally in SQLite today; the repository layer is structured so a
cloud-sync backend can be added later without changing ViewModels or Views.

## Commands

Build (pick the target you're working on — building all target frameworks at once requires every platform SDK
to be installed):

```
dotnet build -f net8.0-windows10.0.19041.0   # fastest inner loop on this Windows dev machine
dotnet build -f net8.0-android                # requires Android SDK + a JDK the Android tooling recognizes
dotnet build -f net8.0-ios                    # requires a Mac build host (Pair to Mac) or macOS
```

Run/debug: use `dotnet build -t:Run -f <tfm>` or launch via an IDE (Visual Studio / VS Code with the MAUI
extension) and pick a target device/emulator — there is no headless `dotnet run` story for MAUI mobile heads.

There are currently no automated tests in this repo.

### Environment notes for this machine

- .NET 8 SDK and the `maui` workload are installed.
- The Android SDK/JDK are **not** configured, so `net8.0-android` builds will fail with `XA5300` until the
  Android SDK path (and a JDK the Android tooling can locate, e.g. via `JavaSdkDirectory`) is set up — install
  via Visual Studio's Android component or the standalone Android SDK per
  https://aka.ms/dotnet-android-install-sdk.
- iOS builds require a networked Mac (Pair to Mac) since there's no iOS toolchain on Windows.
- Windows build (`net8.0-windows10.0.19041.0`) works standalone and is the quickest way to sanity-check that
  shared (non-platform-specific) code compiles.

## Architecture

Single-project MAUI app (`DiabetesHelper.csproj`, `TargetFrameworks`: `net8.0-android`, `net8.0-ios`, and
`net8.0-windows10.0.19041.0` when building on Windows). MacCatalyst and Tizen heads were removed from the
template scaffold since they aren't targeted.

Navigation is a Shell `TabBar` (`AppShell.xaml`) with three tabs, one per logging feature — there is no
flyout menu (`Shell.FlyoutBehavior="Disabled"`).

**Layered structure**, each feature (Glucose, Insulin, Meal) follows the same shape across these layers:

- `Models/` — plain SQLite-mapped POCOs (`GlucoseReading`, `InsulinDose`, `Meal`), decorated with
  `SQLite-net` attributes (`[PrimaryKey, AutoIncrement]`).
- `Data/LocalDatabase.cs` — owns the single `SQLiteAsyncConnection` (file lives in
  `FileSystem.AppDataDirectory`) and lazily creates tables for all three models on first use.
- `Services/IRecordRepository<T>` — the storage seam. `LocalRecordRepository<T>` is the only implementation
  today (thin wrapper over the SQLite connection). **When cloud sync is added, implement this interface
  again (e.g. a `RemoteRecordRepository<T>` or a decorator that syncs then delegates to the local one) rather
  than changing the interface shape** — ViewModels depend only on `IRecordRepository<T>`, not on SQLite.
- `ViewModels/` — one per feature (`GlucoseLogViewModel`, `InsulinLogViewModel`, `MealLogViewModel`), built
  with CommunityToolkit.Mvvm (`ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`). Each exposes an
  `ObservableCollection<T>` of records, bindable properties for a "new entry" form, a `LoadCommand`, and a
  `SaveCommand`. These were kept as three separate concrete classes rather than one generic base — the form
  fields differ per entity (glucose value vs. units+type vs. description+carbs), so a shared base would need
  as much per-type special-casing as just writing three small classes.
- `Views/` — one `ContentPage` + code-behind per feature. Pages call `_viewModel.LoadCommand.ExecuteAsync(null)`
  in `OnAppearing` to refresh from the DB; there's no separate "navigated to" messaging system.

**DI wiring** happens in `MauiProgram.cs`: `LocalDatabase` and the open-generic
`IRecordRepository<> -> LocalRecordRepository<>` mapping are singletons; pages and ViewModels are transient,
constructor-injected (standard MAUI shell-based DI, no service locator pattern in use).

When adding a new loggable record type, follow the existing four-file pattern (model → repository resolves
automatically via the open generic → ViewModel → page), then add a `ShellContent` tab in `AppShell.xaml` and
register the page/ViewModel in `MauiProgram.cs`.
