namespace FeatureFlag101.Infrastructure.FeatureFlags;

/// <summary>
/// Centralised feature flag names. Use these constants throughout the codebase
/// to avoid magic strings and enable compile-time safety.
/// </summary>
/// <remarks>
/// Each constant must match the corresponding key under "FeatureManagement"
/// in appsettings.json / appsettings.Development.json.
/// </remarks>
public static class FeatureNames
{
    /// <summary>
    /// When enabled, <c>GET /weatherforecast</c> returns the enhanced V2 response
    /// that includes <c>humidity</c>, <c>windSpeedKmh</c>, and <c>uvIndex</c>.
    /// Consumers that only read V1 fields are unaffected (backward compatible).
    /// </summary>
    public const string EnhancedWeatherForecast = nameof(EnhancedWeatherForecast);

    /// <summary>
    /// When enabled, the dedicated <c>GET /weatherforecast/enhanced</c> endpoint
    /// is accessible. With the flag disabled the endpoint returns 404, making
    /// it invisible to clients during the rollout phase.
    /// </summary>
    public const string EnhancedWeatherForecastEndpoint = nameof(EnhancedWeatherForecastEndpoint);

    /// <summary>
    /// When enabled, requests that specify a <c>days</c> query parameter greater
    /// than 14 are rejected with 400 Bad Request.
    /// </summary>
    public const string StrictTemperatureValidation = nameof(StrictTemperatureValidation);
}
