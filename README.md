# RaceDay Part 2 — ASP.NET Core Web API

This is a Visual Studio 2022 solution targeting .NET 8. It includes the API, EF Core SQL Server connection, session-based login/logout, password hashing, role checks, controllers for users/events/categories/enrolments/results, Swagger, basic API smoke tests, and a GitHub Actions workflow.

## Run in Visual Studio 2022
1. Extract the ZIP to a normal writable folder.
2. Open `RaceDay.sln`.
3. In Solution Explorer, right-click **RaceDay.Api** and choose **Set as Startup Project** (do not choose RaceDay.Tests).
4. Open `appsettings.json`. The default connection uses SQL Server LocalDB:
   `Server=(localdb)\\MSSQLLocalDB;Database=RaceDay;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`
   If you use SQL Server Express or another instance, change `Server=...` to your server name.
5. Make sure the `ASPNETCORE_ENVIRONMENT` launch setting is `Development`, then press **F5** or **Ctrl+F5**.
6. Swagger should open at `https://localhost:7184/swagger` (or the HTTP profile shown in `Properties/launchSettings.json`). If it does not open automatically, copy the URL into your browser.
7. On first run, the API uses EF Core `EnsureCreated()` to create the database and seeds the `Organiser` and `Participant` roles. This is intended for local coursework development; use EF migrations for a production deployment.

## First API test
1. In Swagger, run `POST /api/auth/register` with a request like:
   ```json
   {
     "firstName": "Taylor",
     "lastName": "Tester",
     "email": "taylor@example.com",
     "password": "StrongPass123!",
     "phone": "0712345678",
     "roleName": "Participant"
   }
   ```
2. Run `POST /api/auth/login` with the same email and password. The API establishes a session cookie.
3. Use `GET /api/users/me` and other protected endpoints in the same browser/session. Swagger's browser-based requests should retain the session cookie when using the same origin.
4. Register an `Organiser` separately to test organiser-only operations.

## Database script
`Database/RaceDay.sql` is included for review/execution in SSMS. The API currently creates its EF Core database from the model using `EnsureCreated()`; do not run the SQL script against the same database first unless you understand that `EnsureCreated()` does not migrate an existing schema. Back up any existing database before experimenting.

## Build and test
- Visual Studio: **Build → Build Solution**, then **Test → Run All Tests**.
- CLI from this folder (with .NET 8 SDK installed):
  ```bash
  dotnet restore RaceDay.sln
  dotnet build RaceDay.sln
  dotnet test RaceDay.sln
  ```

## Important notes
- I cannot certify a successful compile/run from this environment because the .NET SDK is not installed here. Run Build Solution locally and fix any environment-specific connection/package issues that appear.
- This is a coursework starter implementation, not a production security review. Session state is in-memory and will be lost when the API restarts; configure a persistent distributed cache for multi-instance production hosting.
- Review the schema and endpoint implementation against your Part 1 SQL/ERD before submission. The API model currently includes `DistanceKm` and `EventType` fields on Events, which are not shown in the six-table SQL schema described in the Part 1 document. Remove those properties and their DTO/controller references if your final Part 1 SQL script does not include them, or update the approved schema consistently with your lecturer's guidance.
