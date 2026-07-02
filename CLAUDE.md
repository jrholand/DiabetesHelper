# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

DiabetesHelper is a .NET MAUI mobile app (Android, iOS, Windows) for logging blood glucose readings, insulin
doses, and meals/carb counts. Data is stored locally in LiteDB (an embedded, single-file NoSQL document
database) today; the repository layer is structured so a cloud-sync backend can be added later without
changing ViewModels or Views. **Never introduce a SQL/relational data store here** — local persistence must
stay NoSQL (LiteDB) going forward.

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

## Session workflow

When starting work in a new session, ask the user for a branch name. If that branch doesn't already
exist locally or on the remote, create it (off the current branch) and switch to it; do all work for the
session on that branch rather than directly on `master`.

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

- `Models/` — plain POCOs (`GlucoseReading`, `InsulinDose`, `Meal`, `MealItem`, `FavoriteFood`). LiteDB's
  default convention maps a property named `Id` as the primary key, so no mapping attributes are needed.
  The three loggable record types (`GlucoseReading`, `InsulinDose`, `Meal`) each carry two dates:
  `CreatedAtUtc` (`init`-only — set once at creation and never reassigned, so it's immutable everywhere in
  app code) and `EffectiveDateUtc` (mutable — defaults to `CreatedAtUtc` at creation, editable later via the
  edit-record flow). `MealItem` has no timestamp; `FavoriteFood.LastUsedUtc` is a cache-freshness field, not
  a loggable-record date, so neither got the Created/Effective split. `ApiKeyEntry` (see below) also has no
  split — it's a secret with an entry date, not a loggable diabetes record a user would backdate.
- `Data/LiteDbContext.cs` — owns the single `LiteDatabase` (file lives in `FileSystem.AppDataDirectory`).
  LiteDB creates collections lazily on first `GetCollection<T>()` access, so no explicit table/collection
  creation step is needed. Also owns schema-version migrations (see "Schema evolution" below).
- `Services/IRecordRepository<T>` — the storage seam. `LiteDbRecordRepository<T>` is the only implementation
  today (thin wrapper over `ILiteCollection<T>`, using `Upsert` for both insert and update). **When cloud sync
  is added, implement this interface again (e.g. a `RemoteRecordRepository<T>` or a decorator that syncs then
  delegates to the local one) rather than changing the interface shape** — ViewModels depend only on
  `IRecordRepository<T>`, not on LiteDB. `ApiKeyEntry`/`IApiKeyVaultService`/`LiteDbApiKeyVaultService` are a
  separate, non-generic vault for AI provider API keys (history-preserving, single globally-active entry
  tracked via `Preferences`) — deliberately outside `IRecordRepository<T>` since keys are secrets, not data
  to ever sync to a future cloud backend.
- `ViewModels/` — one per feature (`GlucoseLogViewModel`, `InsulinLogViewModel`, `MealLogViewModel`), built
  with CommunityToolkit.Mvvm (`ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`). Each exposes an
  `ObservableCollection<T>` of records sorted newest-first by `EffectiveDateUtc`, bindable properties for a
  "new entry" form, a `LoadCommand`, and a `SaveCommand`. These were kept as three separate concrete classes
  rather than one generic base — the form fields differ per entity (glucose value vs. units+type vs.
  description+carbs), so a shared base would need as much per-type special-casing as just writing three small
  classes. Each feature also has a matching `EditXViewModel` (`EditGlucoseReadingViewModel`,
  `EditInsulinDoseViewModel`, `EditMealViewModel`) that edits only `EffectiveDateUtc` on the same live record
  instance shown in the list, following the same one-class-per-type convention.
- `Views/` — one `ContentPage` + code-behind per feature. Pages call `_viewModel.LoadCommand.ExecuteAsync(null)`
  in `OnAppearing` to refresh from the DB; there's no separate "navigated to" messaging system. Tapping a row
  in the list's `CollectionView` opens the matching `EditXPage` as a modal (via `Views/EditRecordNavigation.cs`,
  modeled on the existing `Views/AboutNavigation.cs` pattern); popping the modal re-fires `OnAppearing` on the
  page underneath, which reloads and re-sorts the list automatically.

**DI wiring** happens in `MauiProgram.cs`: `LiteDbContext` and the open-generic
`IRecordRepository<> -> LiteDbRecordRepository<>` mapping are singletons; pages and ViewModels are transient,
constructor-injected (standard MAUI shell-based DI, no service locator pattern in use).

When adding a new loggable record type, follow the existing four-file pattern (model → repository resolves
automatically via the open generic → ViewModel → page), then add a `ShellContent` tab in `AppShell.xaml` and
register the page/ViewModel in `MauiProgram.cs`.

### Schema evolution (keeping upgrades backwards compatible)

LiteDB documents are schema-less BSON, so most changes are safe by default:

- **Safe, no migration needed**: adding a new nullable/optional property to a model. Existing documents
  written before the change simply deserialize the new property as its default value.
- **Not safe silently**: renaming a property, removing one whose data still matters, changing a property's
  type, or changing what an existing value means (e.g. redefining an enum). Any of these needs a migration
  step added to the `Migrations` array in `Data/LiteDbContext.cs`, gated on `LiteDatabase.UserVersion`, so
  existing users' data is transformed forward instead of being silently misread or dropped on the next
  update.

No data should ever be lost across an app update — that's the whole point of the migration hook existing.
This also means the on-disk `diabeteshelper.litedb` version-bump mechanics in `DiabetesHelper.csproj`
(Windows MSIX packaging) and this schema-migration hook are solving two different halves of the same "don't
lose local data" requirement: one keeps a redeploy from wiping the file, the other keeps a future model
change from corrupting what's in it.
