# Deploying to Render

Deploy only after all three parts (Marmik, Soham, Sahil) are pushed to the same repo.
The app won't build with only one or two parts in it.

## 1. Database (recommended): MongoDB Atlas free tier
Render's free instances restart and sleep after about 15 minutes idle. In the default
in-memory mode, every log, user and alert is lost each time that happens.

1. Create a free M0 cluster at mongodb.com/atlas.
2. Database Access: add a database user and password.
3. Network Access: add `0.0.0.0/0`, because Render free instances have no fixed IP.
4. Copy the connection string (`mongodb+srv://...`).

## 2. Render web service
1. Sign in to Render with the GitHub account that owns the repo, or make the repo public.
2. Click **New > Web Service** and pick the repo. Render detects the `Dockerfile`, so set Language to **Docker**.
3. Instance type: Free.
4. Health check path: `/health`.
5. Add these environment variables (double underscore `__` means a nested setting):

| Key | Value |
|---|---|
| `Storage__Provider` | `MongoDB` |
| `MongoDb__ConnectionString` | your Atlas connection string |
| `MongoDb__DatabaseName` | `EnterpriseLogIntelligence` |
| `Authentication__SeedAdminPassword` | a strong password for `admin` |
| `Ingestion__ApiKey` | a long random key (protects `POST /api/logs`) |

6. Click **Create Web Service**. The first build takes a few minutes.
   The site will be at `https://<name>.onrender.com`.

## Notes
- If you skip Atlas, leave out the Mongo variables. The app runs in demo mode, but data resets on every restart.
- Without `SeedAdminPassword`, the admin password falls back to `admin123`, which anyone could guess on a public site.
- The admin password is only set the first time the admin account is created in the database. Changing the variable later has no effect.
- After a restart or redeploy, signed-in users have to sign in again. That's expected.
- Each push to `main` triggers a new deploy automatically.
