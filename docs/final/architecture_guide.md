# IronMind Architecture Guide

## System Purpose

IronMind is a cloud-hosted ASP.NET Core Minimal API for fitness tracking. It stores user profiles, nutrition logs, exercise logs, hydration logs, and reminder schedules.

## High-Level Architecture

```text
User / API Client
  | HTTP port 8080
  v
ECS Fargate Task
  |
  | Npgsql / PostgreSQL connection
  v
Amazon RDS PostgreSQL
```

Supporting AWS services:

- Amazon ECR stores the Docker image.
- AWS Secrets Manager stores the database connection string and JWT values.
- CloudWatch Logs stores container logs.
- An Application Load Balancer can be added for a stable production URL.
- IAM controls permissions for ECS, ECR, Secrets Manager, RDS, and deployment automation.

## Repository Architecture

```text
IronMind/
├── IronMind.API/          ASP.NET Core Minimal API and route definitions
├── IronMind.Core/         Domain models, DTOs, interfaces, unit conversion helpers
├── IronMind.Data/         EF Core DbContext, entity config, migrations, repositories
├── IronMind.Services/     Business logic for auth, nutrition, exercise, hydration
├── IronMind.Tests/        xUnit tests and in-memory repositories
├── .github/workflows/     CI/CD workflows
├── aws/                   ECS task definition
├── docker-compose.yml     Local API + PostgreSQL environment
└── docs/                  Project documentation
```

## Application Layers

### API Layer

Project: `IronMind.API`

Responsibilities:

- Configures dependency injection.
- Configures JWT authentication.
- Configures Swagger.
- Maps route groups.
- Runs startup EF Core migrations.
- Exposes health and database test endpoints.

Important files:

- `Program.cs`
- `Routes/AuthRoutes.cs`
- `Routes/NutritionRoutes.cs`
- `Routes/ExerciseRoutes.cs`
- `Routes/HydrationRoutes.cs`

### Core Layer

Project: `IronMind.Core`

Responsibilities:

- Defines domain models.
- Defines DTO records.
- Defines service and repository interfaces.
- Stores shared conversion logic.

Important folders:

- `Models/`
- `DTOs/`
- `Interfaces/`

### Data Layer

Project: `IronMind.Data`

Responsibilities:

- Defines `AppDbContext`.
- Configures EF Core entities.
- Stores migrations.
- Implements repository interfaces.

Database provider:

- PostgreSQL through `Npgsql.EntityFrameworkCore.PostgreSQL`

### Service Layer

Project: `IronMind.Services`

Responsibilities:

- Auth registration, login, JWT creation, profile updates.
- Nutrition logging, daily summaries, food search through Open Food Facts.
- Exercise logging, history, updates, calorie estimates.
- Hydration logging, daily summaries, reminder schedules.

### Test Layer

Project: `IronMind.Tests`

Responsibilities:

- Tests auth registration, duplicate email handling, and unit conversion.
- Tests exercise calorie estimation.
- Uses in-memory repositories for service tests.

## AWS Deployment Architecture

### Container Registry

Amazon ECR repository:

```text
481088927864.dkr.ecr.ca-central-1.amazonaws.com/ironmind/api
```

### Runtime

ECS cluster:

```text
ironmind-cluster
```

ECS service:

```text
ironmind-api-service
```

Task definition:

```text
ironmind-api:2
```

Runtime platform:

```text
Linux ARM64
```

The ARM64 setting is required because the image was built from Apple Silicon and pushed as an ARM64 image.

### Public Access

Current endpoint type:

```text
Direct public ECS Fargate task IP
```

Current verified endpoint:

```text
http://15.223.186.37:8080
```

Recommended production improvement:

```text
Application Load Balancer on port 80 forwarding to ECS task port 8080
```

### Database

RDS instance:

```text
ironmind-postgres
```

Engine:

```text
PostgreSQL
```

Database:

```text
ironmind
```

### Secrets

Secrets Manager entries:

```text
ironmind/prod/connection-string
ironmind/prod/jwt-secret
ironmind/prod/jwt-issuer
ironmind/prod/jwt-audience
```

The ECS task definition injects these as environment variables.

## Security Model

- Passwords are hashed with BCrypt.
- JWT bearer authentication protects user routes.
- Production secrets are not committed to GitHub.
- RDS is not publicly accessible.
- RDS accepts PostgreSQL traffic only from the ECS API security group.
- The ECS task currently accepts public HTTP traffic on port 8080.
- RDS accepts PostgreSQL traffic only from the ECS API security group.

## CI/CD Architecture

GitHub Actions workflow:

```text
.github/workflows/deploy.yml
```

Workflow behavior:

1. Restore, build, and test the .NET solution.
2. Authenticate to AWS using OIDC.
3. Build the Docker image.
4. Push the image to ECR.
5. Render the ECS task definition.
6. Deploy the ECS service.

Validation workflow:

```text
.github/workflows/pr-validation.yml
```

This workflow builds, tests, and validates Docker image creation on pull requests.
