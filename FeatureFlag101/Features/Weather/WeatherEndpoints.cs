using FeatureFlag101.Infrastructure.FeatureFlags;
using Microsoft.FeatureManagement;

namespace FeatureFlag101.Features.Weather;

/// <summary>
/// Registers all weather-related endpoints as a grouped route under
/// <c>/weatherforecast</c>. Kept separate from <c>Program.cs</c> so each
/// feature area owns its routing, OpenAPI tags, and filters.
/// </summary>
public static class WeatherEndpoints
{
    /// <summary>
    /// Maps weather forecast routes onto the application.
    /// Call this from <c>Program.cs</c> after <c>app.Build()</c>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication MapWeatherEndpoints(this WebApplication app)
    {
        var group = app
            .MapGroup("/weatherforecast")
            .WithTags("Weather");

        // ── V1 / adaptive endpoint ──────────────────────────────────────────
        // Returns V1 or V2 depending on the EnhancedWeatherForecast flag.
        // Backward-compatible: existing consumers see no breaking change.
        group.MapGet("/", WeatherHandlers.GetForecastAsync)
            .WithName("GetWeatherForecast")
            .WithSummary("Get weather forecast")
            .WithDescription(
                "Returns a multi-day weather forecast. " +
                "When the `EnhancedWeatherForecast` feature flag is enabled the response " +
                "includes additional fields (humidity, windSpeedKmh, uvIndex) while " +
                "remaining backward-compatible with V1 consumers.");

        // ── V2 dedicated endpoint ───────────────────────────────────────────
        // Always returns the V2 payload. The endpoint is hidden (404) until
        // the EnhancedWeatherForecastEndpoint feature flag is enabled.
        group.MapGet("/enhanced", WeatherHandlers.GetEnhancedForecastAsync)
            .WithName("GetEnhancedWeatherForecast")
            .WithSummary("Get enhanced weather forecast (V2)")
            .WithDescription(
                "Returns the V2 weather forecast including humidity, wind speed, and UV index. " +
                "Requires the `EnhancedWeatherForecastEndpoint` feature flag to be enabled. " +
                "Returns 404 when the flag is disabled.")
            .AddEndpointFilter(async (context, next) =>
            {
                var featureManager = context.HttpContext.RequestServices
                    .GetRequiredService<IFeatureManager>();

                return await featureManager.IsEnabledAsync(FeatureNames.EnhancedWeatherForecastEndpoint)
                    ? await next(context)
                    : Results.NotFound(new
                    {
                        error = "This endpoint is not available. Enable the 'EnhancedWeatherForecastEndpoint' feature flag to activate it."
                    });
            });

        return app;
    }
}
