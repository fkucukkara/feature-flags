# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                          # Build the solution
dotnet run --project FeatureFlag101   # Run the API (http://localhost:5151)
dotnet build --configuration Release  # Release build
```

Manual API testing is done via `FeatureFlag101/FeatureFlag101.http` (VS Code REST Client or Visual Studio).

## Architecture

Single ASP.NET Core Minimal API project (**.NET 10**) demonstrating feature flag patterns using `Microsoft.FeatureManagement.AspNetCore`.

**Composition root** (`Program.cs`) is intentionally minimal — it delegates to:
- `builder.AddAppServices()` — registers OpenAPI and FeatureManagement
- `app.MapWeatherEndpoints()` — registers all routes

**Feature organization**: each feature lives in `Features/<FeatureName>/` with three files:
- `*Endpoints.cs` — route registration and endpoint filters
- `*Handlers.cs` — pure static handler methods (injected via delegates)
- `*Models.cs` — request/response types

**Feature flag constants** are centralized in `Infrastructure/FeatureFlags/FeatureNames.cs` — never use magic strings.

## Feature Flag Patterns

Two distinct approaches are used:

1. **In-handler branching** — inject `IFeatureManager` into the handler and call `IsEnabledAsync()` to return different response shapes (e.g., V1 vs V2 payload).

2. **Endpoint-level filter** — attach an `AddEndpointFilter` at route registration time to return 404 when a flag is off, making the endpoint invisible without touching handler logic.

**Configuration**: flags are toggled in `appsettings.json` (all `false` in prod) and `appsettings.Development.json` (all `true` in dev). No code changes needed — can be swapped for Azure App Configuration by changing the configuration provider only.

**Backward-compatible models**: `WeatherForecastV2 : WeatherForecastV1` uses record inheritance so V1 consumers are unaffected when V2 fields are added.

## Adding a New Feature Flag

1. Add a `const string` to `FeatureNames.cs`
2. Add the flag (default `false`) to both `appsettings.json` and `appsettings.Development.json`
3. Inject `IFeatureManager` in the handler or add an endpoint filter
4. Use `await featureManager.IsEnabledAsync(FeatureNames.YourFlag)`
