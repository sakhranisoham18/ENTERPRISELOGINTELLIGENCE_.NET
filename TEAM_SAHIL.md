# Sahil — Frontend: Razor views, CSS/JS and view models (push THIRD)

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
- ViewModels/AdminViewModels.cs
- ViewModels/AlertsViewModel.cs
- ViewModels/AnalyticsViewModel.cs
- ViewModels/DashboardViewModel.cs
- ViewModels/LogFilterViewModel.cs
- ViewModels/LoginViewModel.cs
- Views/Account/AccessDenied.cshtml
- Views/Account/Login.cshtml
- Views/Admin/Index.cshtml
- Views/Alerts/Index.cshtml
- Views/Dashboard/Index.cshtml
- Views/Generator/Index.cshtml
- Views/Home/Error.cshtml
- Views/Ingestion/Index.cshtml
- Views/Logs/Details.cshtml
- Views/Logs/Index.cshtml
- Views/Patterns/Index.cshtml
- Views/Reports/Index.cshtml
- Views/Shared/_Layout.cshtml
- Views/_ViewImports.cshtml
- Views/_ViewStart.cshtml
- wwwroot/css/site.css
- wwwroot/js/dashboard.js
- wwwroot/js/site.js
