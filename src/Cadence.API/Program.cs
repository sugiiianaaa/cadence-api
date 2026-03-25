using Cadence.API.Data;
using Cadence.API.Services.CreateHabitsService;
using Cadence.API.Services.GetHabitsService;
using Cadence.API.Services.UpdateCompletionService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.AddScoped<IGetHabitsService, GetHabitsService>();
builder.Services.AddScoped<ICreateHabitService, CreateHabitService>();
builder.Services.AddScoped<IUpdateCompletionService, UpdateCompletionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();

app.Run();
