using FeatureFlag101.Infrastructure.FeatureFlags;
using Microsoft.FeatureManagement;

namespace FeatureFlag101.Features.Weather;

/// <summary>
/// Pure static handler methods for weather forecast endpoints.
/// Static + injected dependencies make these trivially unit-testable without
/// spinning up a full WebApplication.
/// </summary>
internal static class WeatherHandlers
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Returns a 5-day weather forecast.
    /// When the <c>EnhancedWeatherForecast</c> flag is enabled the response
    /// includes V2 extra fields but remains backward-compatible with V1 consumers.
    /// </summary>
    /// <param name="featureManager">Feature manager resolved via DI.</param>
    /// <param name="days">
    /// Number of forecast days (1–14). Validated against
    /// <c>StrictTemperatureValidation</c> flag when enabled.
    /// </param>
    internal static async Task<IResult> GetForecastAsync(
        IFeatureManager featureManager,
        int days = 5)
    {
        if (await featureManager.IsEnabledAsync(FeatureNames.StrictTemperatureValidation) && days > 14)
            return TypedResults.BadRequest(new { error = "Query parameter 'days' must not exceed 14 when strict validation is active." });

        days = Math.Clamp(days, 1, 30);

        return await featureManager.IsEnabledAsync(FeatureNames.EnhancedWeatherForecast)
            ? TypedResults.Ok(BuildV2Forecast(days))
            : TypedResults.Ok(BuildV1Forecast(days));
    }

    /// <summary>
    /// Returns an enhanced V2 weather forecast.
    /// This endpoint is only reachable when <c>EnhancedWeatherForecastEndpoint</c>
    /// is enabled; otherwise the endpoint filter returns 404 before this handler runs.
    /// </summary>
    /// <param name="featureManager">Feature manager resolved via DI.</param>
    /// <param name="days">Number of forecast days (1–14).</param>
    internal static async Task<IResult> GetEnhancedForecastAsync(
        IFeatureManager featureManager,
        int days = 5)
    {
        if (await featureManager.IsEnabledAsync(FeatureNames.StrictTemperatureValidation) && days > 14)
            return TypedResults.BadRequest(new { error = "Query parameter 'days' must not exceed 14 when strict validation is active." });

        days = Math.Clamp(days, 1, 30);

        return TypedResults.Ok(BuildV2Forecast(days));
    }

    // -------------------------------------------------------------------------
    // Private builders
    // -------------------------------------------------------------------------

    private static WeatherForecastV1[] BuildV1Forecast(int days) =>
        Enumerable.Range(1, days)
            .Select(i => new WeatherForecastV1(
                Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                TemperatureC: Random.Shared.Next(-20, 55),
                TemperatureF: 32 + (int)(Random.Shared.Next(-20, 55) / 0.5556),
                Summary: Summaries[Random.Shared.Next(Summaries.Length)]))
            .ToArray();

    private static WeatherForecastV2[] BuildV2Forecast(int days) =>
        Enumerable.Range(1, days)
            .Select(i =>
            {
                int tempC = Random.Shared.Next(-20, 55);
                return new WeatherForecastV2(
                    Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                    TemperatureC: tempC,
                    TemperatureF: 32 + (int)(tempC / 0.5556),
                    Summary: Summaries[Random.Shared.Next(Summaries.Length)],
                    Humidity: Random.Shared.Next(20, 100),
                    WindSpeedKmh: Math.Round(Random.Shared.NextDouble() * 80, 1),
                    UvIndex: Random.Shared.Next(0, 12));
            })
            .ToArray();
}
