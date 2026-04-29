using System.Text.Json.Serialization;
using Cadence.API.Data;
using Cadence.API.Features.Analytics;
using Cadence.API.Features.Habits;
using Cadence.API.Infrastructure;
using Cadence.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    bool dbConnected = await db.Database.CanConnectAsync();

    if (!dbConnected)
        throw new Exception("Could not connect to database");

    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapHabitEndpoints();
app.MapAnalyticsEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
