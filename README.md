# Cadence

Personal habit tracker backend. Built to eventually power a real app — tracks habits, scheduled days, and daily completions.

## What it is

REST API built with ASP.NET Core (.NET 10) backed by PostgreSQL via EF Core. No auth yet — this is the foundation.

**Core domain:**
- `Habit` — has a name, description, color, icon, scheduled days of the week, and an archive flag
- `Completion` — a record that a habit was done on a specific date (toggle: creates if missing, deletes if exists)

## Stack

| Thing | Choice |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| ORM | EF Core + Npgsql |
| Database | PostgreSQL |

## Endpoints

```
GET    /api/habits                    → all habits
GET    /api/habits/{id}               → single habit
POST   /api/habits                    → create habit
POST   /api/habits/{id}/completion    → toggle today's completion
```

## Architecture

Each operation is its own service class (`IGetHabitsService`, `ICreateHabitService`, etc.) injected into a single `HabitController`. Thin controller, logic in service.

```
Controllers/
  HabitController.cs
Services/
  GetHabitsService/
  GetHabitByIdService/
  CreateHabitsService/
  UpdateCompletionService/
Data/
  AppDbContext.cs
  Entities/
    Habit.cs
    Completion.cs
```

## Running locally

**Prerequisites:** PostgreSQL running, .NET 10 SDK

1. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "Default": "Host=localhost;Database=cadence;Username=youruser;Password=yourpass;"
}
```

2. Apply migrations:
```bash
dotnet ef database update --project src/Cadence.API
```

3. Run:
```bash
dotnet run --project src/Cadence.API
```

App fails fast on startup if DB is unreachable — by design.

## Migrations

```bash
# add new migration
dotnet ef migrations add MigrationName --project src/Cadence.API

# apply
dotnet ef database update --project src/Cadence.API
```
