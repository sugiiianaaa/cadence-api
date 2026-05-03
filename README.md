# Cadence

Personal habit tracker. REST API + CLI tool to log and review daily habits.

## Run with Docker

**Prerequisites:** Docker, Docker Compose

```bash
cp .env.example .env
```

Fill in `.env`:

```env
ConnectionStrings__Default=Host=cadence-db;Database=cadence;Username=cadence;Password=yourpassword
ApiKey=your-secret-key
POSTGRES_DB=cadence
POSTGRES_USER=cadence
POSTGRES_PASSWORD=yourpassword
```

If you're not using Caddy as a reverse proxy, add this to `docker-compose.yml` under `cadence-api` → `ports`:

```yaml
ports:
  - "8080:8080"
```

Then:

```bash
docker compose up -d
```

API is at `http://localhost:8080`. Health check: `GET /health`.

## API

All endpoints require two headers:

```
x-tkey: <your ApiKey>
X-Timezone: Asia/Jakarta   # any IANA timezone
```

```
GET    /api/habits                              list unarchived habits
GET    /api/habits/today                        today's habits with completion state
POST   /api/habits                              create habit
DELETE /api/habits/{id}                         archive habit
PUT    /api/habits/{id}/completions/{date}      set or unset a completion (yyyy-MM-dd)
```

## CLI

```bash
export CADENCE_API_URL=http://localhost:8080
export CADENCE_API_KEY=your-secret-key
export CADENCE_TIMEZONE=Asia/Jakarta

dotnet run --project src/Cadence.CLI
```

Commands:

```
cx                      show today's habits (default)
cx today                same, interactive toggle
cx done <name>          mark a habit done
cx done <name> --undo   unmark
cx week <name>          heatmap for the last 8 weeks
cx habit add <name>     create a habit
cx habit list           list all habits
cx habit archive        archive a habit
cx habit colors         show available color names
```

## Migrations

```bash
dotnet ef migrations add <Name> --project src/Cadence.API
```

Migrations apply automatically on startup.
