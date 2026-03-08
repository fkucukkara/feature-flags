using FeatureFlag101.Extensions;
using FeatureFlag101.Features.Weather;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapWeatherEndpoints();

app.Run();
