# FeatureFlag101

A production-ready **.NET 10 Minimal API** demonstrating **backward-compatible feature flags** using [Microsoft.FeatureManagement](https://github.com/microsoft/FeatureManagement-Dotnet).

The same `GET /weatherforecast` endpoint silently returns an enriched V2 response when a flag is enabled — without breaking any existing V1 consumer.

---

## Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Feature Flags](#feature-flags)
- [API Reference](#api-reference)
- [Project Structure](#project-structure)
- [How to Add a New Feature Flag](#how-to-add-a-new-feature-flag)
- [Upgrading to Azure App Configuration](#upgrading-to-azure-app-configuration)
- [License](#license)

---

## Architecture

```
GET /weatherforecast
        │
        ▼
 WeatherHandlers.GetForecastAsync
        │
        ├─── EnhancedWeatherForecast = false  ──► WeatherForecastV1[]  (stable contract)
        │         { date, temperatureC, temperatureF, summary }
        │
        └─── EnhancedWeatherForecast = true   ──► WeatherForecastV2[]  (additive, non-breaking)
                  { date, temperatureC, temperatureF, summary,
                    humidity, windSpeedKmh, uvIndex }

GET /weatherforecast/enhanced
        │
        ├─── EnhancedWeatherForecastEndpoint = false  ──► 404 (endpoint hidden)
        │
        └─── EnhancedWeatherForecastEndpoint = true   ──► WeatherForecastV2[]
```

### Backward-compatibility guarantee

`WeatherForecastV2` is a record that **inherits** `WeatherForecastV1`.  
Any client that only reads V1 fields will continue to work after V2 fields are added to the payload — no version negotiation, no routing change.

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| IDE | Visual Studio 2022 17.13+, VS Code, or Rider |

---

## Getting Started

```bash
# Clone the repository
git clone https://github.com/<your-org>/FeatureFlag101.git
cd FeatureFlag101

# Restore and build
dotnet build

# Run (Development environment: enhanced flags ON by default)
dotnet run --project FeatureFlag101
```

The API starts on `http://localhost:5151` (or the port in `launchSettings.json`).  
OpenAPI document is served at `http://localhost:5151/openapi/v1.json` in Development.

---

## Feature Flags

Flags live under the `"FeatureManagement"` key in `appsettings.json`.  
Override them per-environment in `appsettings.{Environment}.json` — no code change, no redeployment.

| Flag | Prod default | Dev default | Effect |
|------|-------------|-------------|--------|
| `EnhancedWeatherForecast` | `false` | `true` | `GET /weatherforecast` returns V2 payload (backward-compatible) |
| `EnhancedWeatherForecastEndpoint` | `false` | `true` | `GET /weatherforecast/enhanced` is accessible; returns 404 when disabled |
| `StrictTemperatureValidation` | `false` | `false` | Rejects `?days > 14` with 400 Bad Request |

### Toggling a flag locally

Edit `FeatureFlag101/appsettings.Development.json`:

```json
"FeatureManagement": {
  "EnhancedWeatherForecast": true,
  "EnhancedWeatherForecastEndpoint": true,
  "StrictTemperatureValidation": false
}
```

Restart the app — no rebuild needed.

---

## API Reference

### `GET /weatherforecast`

Returns a multi-day weather forecast.  
Response shape depends on the `EnhancedWeatherForecast` flag.

**Query parameters**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `days` | `int` | `5` | Number of forecast days (clamped to 1–30; max 14 when `StrictTemperatureValidation` is on) |

**V1 response** (default — flag disabled)

```json
[
  {
    "date": "2026-03-09",
    "temperatureC": 22,
    "temperatureF": 71,
    "summary": "Warm"
  }
]
```

**V2 response** (`EnhancedWeatherForecast = true`)

```json
[
  {
    "date": "2026-03-09",
    "temperatureC": 22,
    "temperatureF": 71,
    "summary": "Warm",
    "humidity": 65,
    "windSpeedKmh": 14.3,
    "uvIndex": 5
  }
]
```

---

### `GET /weatherforecast/enhanced`

Always returns the full V2 payload.  
Returns `404` when `EnhancedWeatherForecastEndpoint` is disabled.

**Query parameters** — same as above.

---

## Project Structure

```
FeatureFlag101/
├── Extensions/
│   └── ServiceCollectionExtensions.cs   ← AddAppServices() (OpenAPI + FeatureManagement)
├── Features/
│   └── Weather/
│       ├── WeatherEndpoints.cs           ← MapWeatherEndpoints() route registration
│       ├── WeatherHandlers.cs            ← Static TypedResults handler methods
│       └── WeatherModels.cs              ← WeatherForecastV1 / WeatherForecastV2 records
├── Infrastructure/
│   └── FeatureFlags/
│       └── FeatureNames.cs              ← Const string flag names (no magic strings)
├── Properties/
│   └── launchSettings.json
├── appsettings.json                      ← Production defaults (all flags false)
├── appsettings.Development.json          ← Dev overrides (enhanced flags true)
├── FeatureFlag101.csproj
├── FeatureFlag101.http                   ← HTTP test requests for VS / VS Code
└── Program.cs                           ← Composition root (≤16 lines)
```

---

## How to Add a New Feature Flag

1. **Declare the constant** in `Infrastructure/FeatureFlags/FeatureNames.cs`:

   ```csharp
   /// <summary>Enables the new pricing algorithm.</summary>
   public const string NewPricingAlgorithm = nameof(NewPricingAlgorithm);
   ```

2. **Add the flag to config** (both `appsettings.json` and `appsettings.Development.json`):

   ```json
   "FeatureManagement": {
     "NewPricingAlgorithm": false
   }
   ```

3. **Use it in a handler** via injected `IFeatureManager`:

   ```csharp
   if (await featureManager.IsEnabledAsync(FeatureNames.NewPricingAlgorithm))
   {
       // new code path
   }
   ```

4. **Gate an entire endpoint** with an inline filter (see `WeatherEndpoints.cs` `/enhanced` for the pattern).

---

## Upgrading to Azure App Configuration

The current setup reads flags from `appsettings.json` via `IConfiguration`.  
To switch to real-time, portal-controlled flags with **zero code changes**:

1. Install the Azure App Configuration provider:

   ```bash
   dotnet add package Microsoft.Azure.AppConfiguration.AspNetCore
   ```

2. In `Extensions/ServiceCollectionExtensions.cs`, before `AddFeatureManagement()`:

   ```csharp
   builder.Configuration.AddAzureAppConfiguration(options =>
   {
       options.Connect(new Uri(endpoint), new DefaultAzureCredential());
       options.UseFeatureFlags();
   });
   builder.Services.AddAzureAppConfiguration();
   ```

3. Add `app.UseAzureAppConfiguration();` before `MapWeatherEndpoints()` in `Program.cs`.

All flag names, handler logic, and endpoint code remain unchanged.

---

## License

[MIT](LICENSE)
