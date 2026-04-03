using Cadence.API.Data;
using Cadence.API.Services.CreateHabitsService;
using Cadence.API.Services.GetHabitStatusService;
using Cadence.API.Services.GetHabitByIdService;
using Cadence.API.Services.GetHabitsService;
using Cadence.API.Services.UpdateHabitCompletionService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddScoped<IGetHabitsService, GetHabitsService>();
builder.Services.AddScoped<IGetHabitByIdService, GetHabitByIdService>();
builder.Services.AddScoped<ICreateHabitService, CreateHabitService>();
builder.Services.AddScoped<IUpdateHabitCompletionService, UpdateHabitCompletionService>();
builder.Services.AddScoped<IGetHabitStatusService, GetHabitStatusService>();

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var dbConnected = await db.Database.CanConnectAsync();
    
    if(!dbConnected)
        throw new Exception("Could not connect to database");
    
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();