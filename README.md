# Enterprise Log Intelligence Platform — Phase 2

ASP.NET Core MVC + C# + .NET 10 LTS log intelligence platform. Phase 2 connects the previously isolated modules through shared services and adds persistent MongoDB support, stronger ingestion, alert evaluation, pattern persistence, user management, audit logging, SignalR refresh, health checks and Atlas-ready configuration.

## Technology
- ASP.NET Core MVC / .NET 10
- C#
- Razor Views + HTML/CSS/JavaScript
- MongoDB .NET/C# driver
- MongoDB Atlas-ready
- ASP.NET Core Cookie Authentication
- ASP.NET Core SignalR
- Built-in health checks

## Local run
1. Open `EnterpriseLogIntelligence.csproj` in Visual Studio 2026.
2. Restore NuGet packages and rebuild.
3. Keep `Storage.Provider=InMemory` for a zero-configuration local run.
4. Run with HTTPS.
5. Login with `admin / admin123` in Development.

## Phase 2 — MongoDB Atlas
1. In MongoDB Atlas, create a database user and add the IP address of the machine that will run the application to the Atlas IP access list. Atlas supplies an SRV connection string for driver connections. See MongoDB's driver/Atlas connection documentation.
2. Do not commit the connection string or passwords to Git.
3. Set these environment variables (or use ASP.NET Core User Secrets):
   - `Storage__Provider=MongoDB`
   - `MongoDb__ConnectionString=mongodb+srv://...`
   - `MongoDb__DatabaseName=EnterpriseLogIntelligence`
   - `Authentication__SeedAdminPassword=<strong-password>`
   - `Ingestion__ApiKey=<strong-api-key>`
4. Start the application. On startup it pings MongoDB and creates the application indexes when the connection is available.
5. Open **Administration** and verify MongoDB status = CONNECTED.
6. Open **Log Explorer**, **Alerts**, **Patterns**, and **Reports** to verify they read from the same persistent store.

## New Phase 2 connections
- Common `ILogRepository` abstraction shared by dashboard, explorer, generator and ingestion.
- MongoDB persistence for logs, patterns, alert rules/events, users and audit logs.
- JSON / JSONL / CSV / text ingestion parser.
- API ingestion with optional `X-ELI-API-Key` protection.
- Alert rule evaluation after ingestion, with thresholds and cooldowns.
- Persistent recurring pattern records.
- PBKDF2 password hashing and Mongo-backed users.
- Administrator user management and audit trail.
- SignalR dashboard refresh after new ingestion.
- `/health` endpoint for basic application/database health.
- Dashboard chart label/undefined visualization fix.

## Important security note
The Development configuration uses `admin / admin123` for local testing. Before any cloud deployment, replace it with a strong secret and move all credentials/API keys to environment variables or a managed secret store.
