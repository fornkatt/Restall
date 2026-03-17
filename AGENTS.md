# Restall – AI Agent Guide

## What This Project Is
A Windows desktop app (Avalonia UI) for managing **RenoDX** and **ReShade** mods across games from Steam, Epic, GOG, Ubisoft, and EA. It auto-detects installed games, checks for mod updates, and handles download/install/uninstall.

## Architecture (Clean Architecture, 4 layers)
```
Domain       – Entities only: Game, RenoDX, ReShade (no dependencies)
Application  – Interfaces, DTOs (records), UseCases, Facades, Services
Infrastructure – Implementations: scanners, services, stores, persistence
UI           – Avalonia ViewModels/Views, DI wiring
```
**Dependency rule**: UI → Infrastructure → Application → Domain.  
`Restall.Application` references only `Restall.Domain`. `Restall.UI` references all three.

## Dependency Injection
All registrations live in two extension methods:
- `Restall.Infrastructure/Extensions/InfrastructureServiceCollectionExtensions.cs` – services, use cases, facade
- `Restall.UI/Extensions/UIServiceCollectionExtensions.cs` – ViewModels and UI services

Platform scanners (`IPlatformScannerService`) are all registered as **Singletons** and injected as `IEnumerable<IPlatformScannerService>` into `GameDetectionService`.

## MVVM Conventions
- ViewModels extend `ViewModelBase` (which is `ObservableRecipient`).
- Use `[ObservableProperty]` (CommunityToolkit.Mvvm source gen) for bindable props; `partial` class required.
- `[NotifyPropertyChangedFor(nameof(X))]` chains are used heavily on `[ObservableProperty]` fields.
- Cross-VM communication uses `IRecipient<SelectedGameChangedMessage>` + `Messenger.Send(...)`. A `_suppressMessage` bool prevents echo loops. See `GameListViewModel`, `ModViewModel`, `MainWindowViewModel`.
- Commands use `[RelayCommand(CanExecute = nameof(...))]` and are invalidated via `NotifyCanExecuteChanged()`.

## Use Case & Facade Pattern
- Each operation has an `IXxxUseCase` interface and concrete class (`UseCases/`). Use cases return `ModOperationResultDto` (a record).
- `ModManagementFacade` wraps use cases and appends update-check results post-install.
- Request inputs are **record types** in `Application/UseCases/Requests/`.

## Startup Flow
`App.axaml.cs` → shows `StartupWindow` → calls `StartupWindowViewModel.InitializeAsync()` → `RefreshLibraryUseCase.ExecuteAsync()` runs platform scans + version/wiki fetches in parallel → fires `InitializationCompleted` event → `MainWindow` is swapped in.

## Key External Sources (parsed at runtime, no local DB)
- ReShade versions: scraped from `reshade.me` and `github.com/crosire/reshade/tags`
- RenoDX mod list: `raw.githubusercontent.com/wiki/clshortfuse/renodx/Mods.md` (HtmlAgilityPack)
- RenoDX releases: `github.com/clshortfuse/renodx/releases/tag/snapshot|nightly-*`
- SteamGridDB artwork: `craftersmine.SteamGridDB.Net` SDK; API key in **user secrets** (`UserSecretsId: restall-steamgriddb`)

## Mod Detection
`ModDetectionService` uses **PeNet** to read PE version headers from disk:
- ReShade: scans `*.dll` / `*.asi` (max 10 MB), matches `ProductName == "ReShade"`
- RenoDX: scans `*.addon64` / `*.addon32`, matches `OriginalFilename.StartsWith("renodx-")`

## Caching
All cache/download paths are relative to `AppDomain.CurrentDomain.BaseDirectory`:
- `Cache/ReShade/<branch>/<version>/` – extracted ReShade DLL
- `DownloadCache/ReShade/<branch>/` – raw installer `.exe`
- `DownloadCache/RenoDX/<branch>/` – raw addon files
- `Cache/SGDB/<steamGridDbId>/` – artwork PNGs + `index.json` lookup

## Game Name Matching
`GameNameHelper` (Application layer) provides `NormalizeName`, `StripEditionSuffix`, and `FuzzyNameMatch` used when matching detected games against the RenoDX wiki mod list. Use these helpers—don't roll custom string comparison.

## Build & Run
```powershell
# Build entire solution
dotnet build Restall.slnx

# Run the UI project (entry point)
dotnet run --project Restall.UI/Restall.UI.csproj
```
Target framework: **net10.0**. The only executable project is `Restall.UI` (`OutputType=WinExe`).

## Key Files to Know
| File | Purpose |
|------|---------|
| `Restall.Infrastructure/Extensions/InfrastructureServiceCollectionExtensions.cs` | All service registrations |
| `Restall.UI/App.axaml.cs` | App startup, DI container bootstrap |
| `Restall.Application/Facades/ModManagementFacade.cs` | Unified mod install/uninstall API |
| `Restall.Application/UseCases/RefreshLibraryUseCase.cs` | Full library scan orchestration |
| `Restall.Infrastructure/Services/ModDetectionService.cs` | PE-based mod detection |
| `Restall.UI/ViewModels/GameModViewModel.cs` | Per-game VM wrapping domain `Game` + lazy `Bitmap` loading |
| `Restall.UI/Messages/SelectedGameChangedMessage.cs` | The only inter-VM message |

