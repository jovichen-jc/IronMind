# IronMind Database Schema

## Database Summary

Database engine:

```text
PostgreSQL
```

ORM:

```text
Entity Framework Core
```

Production database:

```text
Amazon RDS PostgreSQL
```

Local database:

```text
PostgreSQL through Docker Compose or local PostgreSQL
```

The application stores metric values internally. User-facing values can be converted to imperial units in API responses based on user preference.

## Entity Relationship Overview

```text
Users
  ├── MealLogs
  ├── WorkoutLogs
  │     ├── CardioDetails
  │     └── StrengthSets
  ├── WaterLogs
  └── ReminderSchedules
```

## Tables

### Users

Stores account, profile, goal, unit preference, and notification information.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| Email | text / varchar(255) | Required, unique |
| PasswordHash | text | BCrypt password hash |
| Name | text / varchar(100) | Required |
| DateOfBirth | date | Required |
| WeightKg | real | Stored in kilograms |
| HeightCm | real | Stored in centimeters |
| Units | text | `Metric` or `Imperial` |
| DailyCalorieGoal | real | Daily calorie target |
| DailyWaterGoalMl | real | Stored in milliliters |
| DeviceToken | text, nullable | Optional notification token |
| CreatedAt | timestamp | UTC creation time |

Relationships:

- One user has many meal logs.
- One user has many workout logs.
- One user has many water logs.
- One user has many reminder schedules.

Indexes:

- Unique index on `Email`

### MealLogs

Stores nutrition entries.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| UserId | integer | Foreign key to `Users.Id` |
| FoodName | text / varchar(200) | Required |
| Calories | real | Required |
| ProteinG | real, nullable | Protein in grams |
| CarbsG | real, nullable | Carbohydrates in grams |
| FatG | real, nullable | Fat in grams |
| OpenFoodFactsId | text, nullable | Optional external food ID |
| LoggedAt | timestamp | UTC log time |

Indexes:

- Composite index on `UserId`, `LoggedAt`

### WorkoutLogs

Stores one workout session.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| UserId | integer | Foreign key to `Users.Id` |
| Type | text | `Cardio` or `Strength` |
| LoggedAt | timestamp | UTC log time |
| Notes | text, nullable | Optional workout notes |

Indexes:

- Composite index on `UserId`, `LoggedAt`

### CardioDetails

Stores cardio-specific details for a workout.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| WorkoutLogId | integer | Foreign key to `WorkoutLogs.Id` |
| ActivityType | text | Running, cycling, swimming, walking, etc. |
| DurationMinutes | integer | Workout duration |
| DistanceKm | real, nullable | Distance in kilometers |
| CaloriesBurned | real, nullable | Estimated by MET formula |

Relationship:

- One cardio workout has one cardio details row.

### StrengthSets

Stores strength-training sets for a workout.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| WorkoutLogId | integer | Foreign key to `WorkoutLogs.Id` |
| ExerciseName | text | Exercise name |
| SetNumber | integer | Set order |
| Reps | integer | Repetitions |
| WeightKg | real | Stored in kilograms |

Relationship:

- One strength workout has many strength set rows.

### WaterLogs

Stores hydration entries.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| UserId | integer | Foreign key to `Users.Id` |
| AmountMl | real | Water amount in milliliters |
| LoggedAt | timestamp | UTC log time |

### ReminderSchedules

Stores user hydration reminder preferences.

| Column | Type | Notes |
|---|---|---|
| Id | integer | Primary key |
| UserId | integer | Foreign key to `Users.Id` |
| IntervalMinutes | integer | Reminder interval |
| StartTime | time | Daily start time |
| EndTime | time | Daily end time |
| IsActive | boolean | Whether reminders are enabled |

## Migration

Current migration:

```text
IronMind.Data/Migrations/20260423232342_InitialSchema.cs
```

Apply migrations locally:

```bash
dotnet ef database update --project IronMind.Data --startup-project IronMind.API
```

Production behavior:

The API runs EF Core migrations at startup. This lets the ECS task initialize or update the RDS schema automatically when the container starts.

