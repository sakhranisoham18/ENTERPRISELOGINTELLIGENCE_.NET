# Soham — Backend: data models, database, repositories and services (push SECOND)

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
- Data/MongoDbContext.cs
- Data/MongoDbSettings.cs
- Models/Alert.cs
- Models/AlertEvent.cs
- Models/AuditLog.cs
- Models/LogEntry.cs
- Models/LogPattern.cs
- Models/User.cs
- Repositories/AlertRepositories.cs
- Repositories/AuditRepositories.cs
- Repositories/ILogRepository.cs
- Repositories/InMemoryLogRepository.cs
- Repositories/MongoLogRepository.cs
- Repositories/PatternRepositories.cs
- Repositories/UserRepositories.cs
- Services/AlertService.cs
- Services/AnalyticsService.cs
- Services/AuditService.cs
- Services/AuthService.cs
- Services/LogGeneratorService.cs
- Services/LogIngestionService.cs
- Services/LogParserService.cs
- Services/MongoHealthCheck.cs
- Services/PasswordHashService.cs
- Services/PatternService.cs
- Services/StartupDataService.cs
