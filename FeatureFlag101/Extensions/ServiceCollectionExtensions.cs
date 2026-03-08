using Microsoft.FeatureManagement;

namespace FeatureFlag101.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/> that group related
/// service registrations. Keeps <c>Program.cs</c> minimal and each concern composable.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services required by the API:
    /// OpenAPI document generation and Microsoft Feature Management.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>The same builder for chaining.</returns>
    public static WebApplicationBuilder AddAppServices(this WebApplicationBuilder builder)
    {
        // OpenAPI document generation (Scalar / Swagger UI in Development)
        builder.Services.AddOpenApi();

        // Feature Management reads from IConfiguration "FeatureManagement" section.
        // No Azure dependency by default; swap in AddAzureAppConfiguration() to go cloud-native.
        builder.Services.AddFeatureManagement();

        return builder;
    }
}
