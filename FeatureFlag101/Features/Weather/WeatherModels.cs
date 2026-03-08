namespace FeatureFlag101.Features.Weather;

/// <summary>
/// V1 weather forecast response — the stable, public contract.
/// All existing consumers rely on this shape; it must never have breaking changes.
/// </summary>
/// <param name="Date">Forecast date.</param>
/// <param name="TemperatureC">Temperature in Celsius.</param>
/// <param name="TemperatureF">Temperature in Fahrenheit (computed).</param>
/// <param name="Summary">Human-readable condition summary.</param>
public record WeatherForecastV1(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string? Summary);

/// <summary>
/// V2 weather forecast response — an additive, non-breaking extension of V1.
/// Consumers that only deserialize V1 fields continue to work without modification.
/// </summary>
/// <param name="Date">Forecast date.</param>
/// <param name="TemperatureC">Temperature in Celsius.</param>
/// <param name="TemperatureF">Temperature in Fahrenheit (computed).</param>
/// <param name="Summary">Human-readable condition summary.</param>
/// <param name="Humidity">Relative humidity in percent (0–100).</param>
/// <param name="WindSpeedKmh">Wind speed in km/h.</param>
/// <param name="UvIndex">UV index on the WHO scale (0–11+).</param>
public sealed record WeatherForecastV2(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string? Summary,
    int Humidity,
    double WindSpeedKmh,
    int UvIndex) : WeatherForecastV1(Date, TemperatureC, TemperatureF, Summary);
