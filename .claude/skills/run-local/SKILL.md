---
name: run-local
description: >-
  Starts MyLeague locally for manual testing: Postgres + Seq in Docker, the API
  with Kestrel on the host, and the React UI with Vite on the host. Use when the
  user asks to run, start, launch, or try the app, open Scalar, seed the database,
  or when "localhost:5173" / "localhost:8080" is not responding.
---

# Run MyLeague locally

The UI always runs on the host with Vite. Do not start the Compose `frontend` or `webapi` services. `webapi` also listens on 8080, and running the API on the host means the current working tree is what runs.

## Steps

1. Run `docker info`. If Docker Desktop is not running, start it and poll until `docker info` succeeds.
2. Start only PostgreSQL and Seq:

   ```bash
   docker compose up -d postgres seq
   ```

3. Start the API on the host in the background (it is long-running). `appsettings.Development.json` already points at `localhost:5432`.

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = 'Development'
   $env:Serilog__WriteTo__2__Name = 'Seq'
   $env:Serilog__WriteTo__2__Args__serverUrl = 'http://localhost:5341'
   dotnet run --project src/backend/WebAPI/WebAPI.csproj --urls http://localhost:8080
   ```

   The two `Serilog__` variables add a Seq sink. The appsettings files only log to Console and File, so without these variables Seq stays empty. The login code is printed to the console (`LOGIN CODE for ...`). Startup applies all four contexts' migrations. Wait until the log shows `Now listening on: http://localhost:8080`.

4. Start the UI in the background from `src/frontend`. If port 5173 is already served by this repo's Vite process, reuse it.

   ```bash
   pnpm install
   pnpm dev
   ```

5. If the database is empty, seed it while the API is running (the seeder calls the HTTP API):

   ```bash
   dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all
   ```

   Narrower scopes: `hockey`, `clubs`, `teams`, `seasons`, `seasonmatches`, `tournaments`, and others (see `src/tools/Seeder/Program.cs`).

| Service | URL |
|---------|-----|
| UI | http://localhost:5173 |
| API | http://localhost:8080 |
| Scalar | http://localhost:8080/scalar/v1 |
| Seq | http://localhost:5341 |

Leave `src/frontend/.env.development` at `VITE_API_URL=http://localhost:8080/api`. Do not switch it to the launchSettings ports (`65532` / `65533`).

Seeded dev logins: `test@myleague.local` (site admin) and `clubadmin@myleague.local` (club admin).

## Port 5432 already taken

If `localhost:5432` rejects password `postgres`, a local PostgreSQL service owns that port. Leave that service running. Recreate the Compose container on host port 5433 with the existing volume:

```bash
docker stop myleague-postgres
docker rm myleague-postgres
docker run -d --name myleague-postgres --network myleague-app-xamkfi_myleague-network -e POSTGRES_DB=myleague -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5433:5432 -v myleague-app-xamkfi_postgres_data:/var/lib/postgresql/data postgres:16-alpine
```

Then start the API with `ConnectionStrings__DefaultConnection=Host=localhost;Database=myleague;Username=postgres;Password=postgres;Port=5433`.

## Never

- Set `LoginCode:AutoFillLoginCode` (or `LoginCode__AutoFillLoginCode`) to true anywhere except local development
