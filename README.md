# IronMind — Fitness Tracking API

A RESTful fitness tracking API built with **ASP.NET Core (.NET 10)** and **PostgreSQL**. Tracks nutrition, exercise, and hydration for individual users with JWT authentication and Firebase push notifications.

---

## Tech Stack

| Layer | Technology |
| --- | --- |
| API | ASP.NET Core Minimal API (.NET 10) |
| Language | C# |
| Database | PostgreSQL (EF Core + Npgsql) |
| Authentication | JWT Bearer tokens |
| Password hashing | BCrypt.Net-Next |
| Food data | Open Food Facts API |
| Push notifications | Firebase Cloud Messaging (FCM) |
| Deployment | AWS ECS Fargate + RDS PostgreSQL |
| CI/CD | GitHub Actions (OIDC → ECR → ECS) |

---

## Architecture

Clean 4-layer architecture:

```
IronMind.API        → Minimal API routes, middleware, DI configuration
IronMind.Services   → Business logic (Auth, Nutrition, Exercise, Hydration)
IronMind.Core       → Domain models, DTOs, service interfaces, UnitConverter
IronMind.Data       → EF Core DbContext, entity configs, migrations
IronMind.Tests      → xUnit test suite
```

---

## API Endpoints

### Auth
| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/auth/register` | No | Register a new user |
| POST | `/auth/login` | No | Login and receive JWT |
| GET | `/auth/profile` | Yes | Get current user profile |
| PUT | `/auth/profile` | Yes | Update profile (name, weight, height, goals) |

### Nutrition
| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/nutrition/meals` | Yes | Log a meal |
| GET | `/nutrition/meals/summary` | Yes | Daily calorie summary |
| GET | `/nutrition/food/search` | No | Search Open Food Facts database |

### Exercise
| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/exercise/cardio` | Yes | Log a cardio session (MET-based calorie estimation) |
| POST | `/exercise/strength` | Yes | Log a strength training session |
| GET | `/exercise/history` | Yes | View full workout history |
| DELETE | `/exercise/{id}` | Yes | Delete a workout entry |

### Hydration
| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/hydration/water` | Yes | Log water intake |
| GET | `/hydration/water/summary` | Yes | Daily water total vs goal |
| POST | `/hydration/reminders` | Yes | Set reminder schedule |
| PATCH | `/hydration/reminders` | Yes | Enable or disable reminders |

### System
| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/health` | No | Health check |
| GET | `/swagger` | No | Swagger UI (API docs) |

---

## Local Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL running locally (or a connection string to a remote instance)

### 1. Clone the repo

```bash
git clone https://github.com/jovichen-jc/IronMind.git
cd IronMind/IronMind
```

### 2. Configure environment

Create `IronMind.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=ironmind;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters",
    "Issuer": "IronMind",
    "Audience": "IronMindUsers"
  }
}
```

### 3. Apply database migrations

```bash
dotnet ef database update --project IronMind.Data --startup-project IronMind.API
```

### 4. Run the API

```bash
dotnet run --project IronMind.API
```

API available at `http://localhost:5235` — Swagger UI at `http://localhost:5235/swagger`

---

## Running Tests

```bash
dotnet test
```

---

## AWS Deployment

The API is deployed to **AWS ECS Fargate** in `ca-central-1`:

- **Container registry:** ECR (`ironmind/api`)
- **Database:** RDS PostgreSQL `db.t4g.micro`, 20 GB gp3
- **Secrets:** Connection string and JWT secret stored in AWS Secrets Manager
- **CI/CD:** GitHub Actions triggers on push to `main` — builds image, pushes to ECR, updates ECS service
- **Port:** 8080

---

## Project Structure

```
IronMind/
├── IronMind.API/
│   ├── Program.cs
│   └── Routes/
│       ├── AuthRoutes.cs
│       ├── NutritionRoutes.cs
│       ├── ExerciseRoutes.cs
│       └── HydrationRoutes.cs
├── IronMind.Core/
│   ├── Models/
│   ├── DTOs/
│   └── Interfaces/
├── IronMind.Services/
│   ├── AuthService.cs
│   ├── NutritionService.cs
│   ├── ExerciseService.cs
│   ├── HydrationService.cs
│   └── FcmNotificationService.cs
├── IronMind.Data/
│   ├── AppDbContext.cs
│   ├── Config/
│   └── Migrations/
├── IronMind.Tests/
└── docs/
    ├── planning.md
    ├── ports.md
    └── deployment_report.md
```

---

## Security

- Passwords hashed with BCrypt — never stored in plaintext
- All user data isolated by JWT claims — users can only access their own records
- JWT tokens expire after 7 days
- Production secrets stored in AWS Secrets Manager, not in code
- HTTPS enforced in production
