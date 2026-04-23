# FitTrack — Project Planning Document
**Stack:** C# / ASP.NET Core / PostgreSQL  
**Date:** 2026-04-23

---

## 1. Mind Map

```
FitTrack App
│
├── User
│   ├── Register / Login
│   ├── Profile (name, age, weight, height, sex)
│   └── Goals (target weight, daily calories, daily water)
│
├── Nutrition
│   ├── Calorie Tracking
│   │   ├── Log a meal (name, calories, macros)
│   │   ├── Search food database (built-in or API)
│   │   ├── Daily calorie summary
│   │   └── Calorie goal progress bar
│   └── (future: macro breakdown — carbs / protein / fat)
│
├── Exercise
│   ├── Cardio
│   │   ├── Type (run, cycle, swim, walk, etc.)
│   │   ├── Duration (minutes)
│   │   ├── Distance (km/miles)
│   │   └── Estimated calories burned
│   └── Strength Training
│       ├── Exercise name (bench press, squat, etc.)
│       ├── Number of sets
│       ├── Reps per set
│       ├── Weight used (kg/lbs)
│       └── Notes (form cues, feel, etc.)
│
├── Hydration
│   ├── Log water intake (ml or oz per entry)
│   ├── Daily water goal
│   ├── Progress toward goal
│   └── Reminders (timed notifications)
│
└── Dashboard
    ├── Today's summary (calories in/out, water, workouts)
    ├── Weekly history
    └── Streak / progress metrics
```

---

## 2. Functional Requirements

### 2.1 User Authentication & Profile
| ID | Requirement |
|----|-------------|
| F-01 | User can register with email and password |
| F-02 | User can log in and receive a JWT token |
| F-03 | User can create/edit a profile (name, DOB, weight, height) |
| F-04 | User can set personal goals (calorie target, water target, weight goal) |

### 2.2 Calorie Tracking
| ID | Requirement |
|----|-------------|
| F-10 | User can log a meal with a name, calorie count, and optional macros |
| F-11 | System provides a basic food database to search from |
| F-12 | User can view total calories consumed for the current day |
| F-13 | User can edit or delete a logged meal |
| F-14 | System shows remaining calories vs daily goal |

### 2.3 Exercise Tracking
| ID | Requirement |
|----|-------------|
| F-20 | User can log a cardio session (type, duration, distance) |
| F-21 | System estimates calories burned based on cardio session |
| F-22 | User can log a strength session (exercise, sets × reps × weight) |
| F-23 | User can view their full workout history |
| F-24 | User can edit or delete a workout log entry |

### 2.4 Water Intake & Reminders
| ID | Requirement |
|----|-------------|
| F-30 | User can log a water intake entry (amount in ml) |
| F-31 | User can view daily water total vs goal |
| F-32 | System sends reminders at user-configured intervals (e.g. every 2 hours) |
| F-33 | User can enable or disable reminders |

---

## 3. Non-Functional Requirements

| Category | Requirement |
|----------|-------------|
| **Security** | Passwords hashed with BCrypt (no plaintext ever stored) |
| **Security** | All endpoints authenticated via JWT Bearer tokens |
| **Security** | HTTPS enforced in production |
| **Security** | User data is isolated — users can only access their own records |
| **Performance** | API endpoints respond in < 300ms under normal load |
| **Performance** | Database queries use indexes on user_id and date fields |
| **Reliability** | Core flows (log meal, log workout) must not fail silently — return clear error messages |
| **Scalability** | Stateless API design (JWT, no server-side sessions) so it can scale horizontally |
| **Maintainability** | Code follows clean layered architecture (API → Service → Repository) |
| **Maintainability** | All endpoints documented with Swagger/OpenAPI |
| **Usability** | API returns consistent error shapes `{ error: string, code: int }` |
| **Data Integrity** | All writes use DB transactions where multiple tables are affected |

---

## 4. Use Case Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         FitTrack System                         │
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  Authentication                                          │   │
│   │   (UC-01) Register                                      │   │
│   │   (UC-02) Login                                         │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                ▲                                │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  Nutrition Module                                        │   │
│   │   (UC-10) Log Meal                                      │   │
│   │   (UC-11) Search Food Database                         │   │
│   │   (UC-12) View Daily Calorie Summary                   │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                ▲                                │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  Exercise Module                                         │   │
│   │   (UC-20) Log Cardio Session                           │   │
│   │   (UC-21) Log Strength Session                         │   │
│   │   (UC-22) View Workout History                         │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                ▲                                │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  Hydration Module                                        │   │
│   │   (UC-30) Log Water Intake                             │   │
│   │   (UC-31) Set Reminder Schedule                        │   │
│   │   (UC-32) View Daily Water Progress                    │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                ▲                                │
└─────────────────────────────────────────────────────────────────┘
         ▲                                        ▲
         │                                        │
  ┌─────────────┐                        ┌─────────────────┐
  │ Registered  │                        │     System      │
  │    User     │                        │  (Scheduler /   │
  └─────────────┘                        │   Reminders)    │
                                         └─────────────────┘

Actors:
  - Guest         → UC-01, UC-02 only
  - Registered User → all UC-10 through UC-32
  - System        → UC-32 (triggers reminders automatically)
```

---

## 5. Proposed Architecture (Leg 1 scope)

```
IronMind/
├── IronMind.API/          ← ASP.NET Core Minimal API
│   ├── Program.cs
│   ├── Routes/
│   │   ├── AuthRoutes.cs
│   │   ├── NutritionRoutes.cs
│   │   ├── ExerciseRoutes.cs
│   │   └── HydrationRoutes.cs
│   └── appsettings.json
│
├── IronMind.Core/         ← Domain models + interfaces
│   ├── Models/
│   └── Interfaces/
│
├── IronMind.Services/     ← Business logic
│
├── IronMind.Data/         ← EF Core, DbContext, Migrations
│
└── IronMind.Tests/        ← xUnit tests
```

---

## Team Decisions (Locked In — 2026-04-23)

| # | Question | Decision |
|---|----------|----------|
| 1 | Food database | **Open Food Facts API** — free, no key required, 2.8M+ products |
| 2 | Reminders | **Push notifications** — via a notification service (e.g. Firebase FCM) |
| 3 | Units | **User-selectable** — metric (kg/km/ml) or imperial (lbs/miles/oz) stored on user profile |
| 4 | Frontend | **API-first** — no UI this leg, Swagger for testing |
| 5 | Deployment | **Local dev now**, architecture designed to be cloud-portable (Azure-ready) |

### Impact on Architecture

- `UserProfile` model gains a `PreferredUnit` enum field (`Metric` / `Imperial`)
- All display values convert at the API response layer, not in the DB (store metric internally always)
- Push notifications require a `DeviceToken` stored per user + a background notification service
- No hard Azure dependencies — use `IConfiguration` / environment variables so secrets swap cleanly
- Open Food Facts calls go through a dedicated `FoodService` (no direct HTTP calls from routes)
