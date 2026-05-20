# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick Start Commands

**Build:**
```bash
dotnet build
```

**Run API (watch mode for development):**
```bash
dotnet watch run --project IronMind.API
```

**Run API (normal):**
```bash
dotnet run --project IronMind.API
```

**Run all tests:**
```bash
dotnet test
```

**Run specific test class:**
```bash
dotnet test --filter "ClassName"
```

**Run tests with coverage:**
```bash
dotnet test /p:CollectCoverage=true
```

**Apply migrations:**
```bash
dotnet ef database update --project IronMind.Data --startup-project IronMind.API
```

**Create new migration:**
```bash
dotnet ef migrations add MigrationName --project IronMind.Data --startup-project IronMind.API
```

## Project Architecture

IronMind is a .NET 10 ASP.NET Core fitness tracking API with **4-layer clean architecture**:

1. **API Layer** (`IronMind.API`) — Minimal API with route groups, no controllers
   - Routes organized by domain: `AuthRoutes.cs`, `NutritionRoutes.cs`, `ExerciseRoutes.cs`, `HydrationRoutes.cs`
   - Entry point: `Program.cs` (configures DbContext, JWT auth, services, routes)
   - Swagger/OpenAPI enabled on `/swagger`

2. **Services Layer** (`IronMind.Services`) — Business logic
   - `AuthService` — JWT generation, BCrypt password hashing, user registration/login
   - `NutritionService`, `ExerciseService`, `HydrationService` — domain-specific logic
   - All implement interfaces from `IronMind.Core/Interfaces/`

3. **Core Layer** (`IronMind.Core`) — Domain models, DTOs, interfaces
   - Domain models: `User`, `MealLog`, `WorkoutLog`, `CardioDetails`, `StrengthSets`, `WaterLog`, `ReminderSchedule`
   - DTOs: `AuthDtos.cs`, `ExerciseDtos.cs`, `NutritionDtos.cs`, `HydrationDtos.cs`
   - Utility: `UnitConverter.cs` (static conversions between metric/imperial)
   - Service interfaces: `IAuthService`, `INutritionService`, `IExerciseService`, `IHydrationService`

4. **Data Layer** (`IronMind.Data`) — EF Core + PostgreSQL
   - `AppDbContext` — DbContext with all entity DbSets and fluent API configurations
   - Entity configurations in `Config/` folder
   - Migrations in `Migrations/` folder (EF Core Code-First)

## Authentication & Authorization

- **JWT Bearer** — configured in `Program.cs` with token validation
- **Password hashing** — BCrypt.Net-Next (v4.1.0), configured in `AuthService`
- **Token claims** — `NameIdentifier` (user ID), `Email`
- **Token expiry** — 7 days (configurable via `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience` in appsettings)
- **Protected routes** — use `.RequireAuthorization()` middleware
- **Connection string** — stored in AWS Secrets Manager (for production) or `appsettings.Development.json` (local)

## Database Configuration

- **Provider** — PostgreSQL via Npgsql driver
- **Connection string** — key: `ConnectionStrings:Default`
- **Async patterns** — all queries use `async/await`
- **Entity relationships** — defined in `Config/` via fluent API
- **Migrations** — managed by EF Core, apply via `dotnet ef database update`

## Testing

- **Framework** — xUnit
- **Code coverage** — Coverlet (https://github.com/coverlet-coverage/coverlet)
- **Test project** — `IronMind.Tests/`
- **Project references** — IronMind.Services, IronMind.Core (no API layer dependency)

## API Domains

- **Auth** — `/auth/register`, `/auth/login`, `/auth/profile` (GET/PUT), JWT-protected
- **Nutrition** — `/nutrition/meals` (POST), `/nutrition/meals/summary` (GET), `/nutrition/food/search` (public), unit conversion support
- **Exercise** — `/exercise/cardio` (POST), `/exercise/strength` (POST), `/exercise/history` (GET), `/exercise/{id}` (DELETE)
- **Hydration** — `/hydration/water` (POST), `/hydration/water/summary` (GET), `/hydration/reminders` (POST/PATCH), FCM push notifications

## AWS Deployment

- **Region** — ca-central-1
- **Compute** — ECS Fargate (0.25 vCPU, 512 MB RAM)
- **Container registry** — ECR (`ironmind/api`, private repository)
- **Database** — RDS PostgreSQL (`db.t4g.micro`, 20 GB gp3)
- **CI/CD** — GitHub Actions (pushes to ECR on merge to main)
- **Authentication** — GitHub OIDC with IAM role assumption (not hardcoded keys)
- **Docker** — Multi-stage build, Dockerfile in repository root

## Development Workflow

1. **Branch naming** — `dev{N}/feature-name` for feature branches (see `/docs/git_branch_guide.md`)
2. **Migrations** — Always create a migration when modifying entity models
3. **Entity changes** — Update model in `IronMind.Core`, then fluent config in `IronMind.Data/Config/`, then create migration
4. **DTO updates** — Keep DTOs in sync with routes; use record types for immutability
5. **Service implementation** — Add interface to `IronMind.Core/Interfaces/`, implement in `IronMind.Services/`, register in `Program.cs`

## Key Files

- `Program.cs` — Full app configuration, DI setup, route mapping
- `AppDbContext.cs` — All DbSets, migration entry point
- `UnitConverter.cs` — Metric ↔ Imperial conversions (used by all domains)
- `AuthService.cs` — JWT generation logic, password hashing
- Development guides — `/docs/dev{1-4}_guide_*.md` (module-specific patterns)

## Notes for Claude

- **Connection string location** — For local development, ensure `appsettings.Development.json` exists with `ConnectionStrings:Default`; for AWS, pull from Secrets Manager
- **Port** — API runs on 8080 (hardcoded in Dockerfile and ECS task definition)
- **Nullable context** — `<Nullable>enable</Nullable>` enforced across all projects; use `!` for null-forgiving operator when safe
- **Implicit usings** — `<ImplicitUsings>enable</ImplicitUsings>` enabled; standard .NET namespaces auto-imported
