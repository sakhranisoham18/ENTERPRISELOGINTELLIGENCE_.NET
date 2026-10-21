# Marmik — App setup, controllers and API (push FIRST)

Enterprise Log Intelligence is split across three team members. Every zip uses the
same root folder (`EnterpriseLogIntelligence/`), and no file appears in more than
one zip, so the three parts fit together in one GitHub repo with no conflicts.

| Member | Part | Folders |
|---|---|---|
| Marmik | App setup, controllers, API | Program.cs, .csproj, appsettings, Properties, Controllers, Hubs, README, Dockerfile |
| Soham | Backend | Models, Data, Repositories, Services |
| Sahil | Frontend | Views, wwwroot, ViewModels |

The project only builds once all three parts are in the repo.

## Push order

1. **Marmik** creates the GitHub repo, adds Soham and Sahil as collaborators
   (Settings → Collaborators), and pushes his part:
   ```
   cd EnterpriseLogIntelligence
   git init
   git add .
   git commit -m "Add app setup, controllers and API"
   git branch -M main
   git remote add origin https://github.com/<owner>/EnterpriseLogIntelligence.git
   git push -u origin main
   ```
2. **Soham** and then **Sahil** each do:
   ```
   git clone https://github.com/<owner>/EnterpriseLogIntelligence.git
   # copy the contents of your zip's EnterpriseLogIntelligence folder into the cloned folder
   cd EnterpriseLogIntelligence
   git add .
   git commit -m "Add <backend | frontend>"
   git pull --rebase
   git push
   ```

After all three pushes, anyone can clone the repo and run `dotnet run`
(local login: admin / admin123).

## Files in this part
- .dockerignore
- .gitignore
- Controllers/AccountController.cs
- Controllers/AdminController.cs
- Controllers/AlertsController.cs
- Controllers/DashboardController.cs
- Controllers/GeneratorController.cs
- Controllers/HomeController.cs
- Controllers/IngestionController.cs
- Controllers/LogsController.cs
- Controllers/PatternsController.cs
- Controllers/ReportsController.cs
- Dockerfile
- EnterpriseLogIntelligence.csproj
- Hubs/DashboardHub.cs
- PHASE_1_CHECKLIST.md
- PHASE_2_CHECKLIST.md
- Program.cs
- Properties/launchSettings.json
- README.md
- RENDER_DEPLOY.md
- appsettings.Development.json
- appsettings.json

To host the app online after all three parts are pushed, see RENDER_DEPLOY.md.
