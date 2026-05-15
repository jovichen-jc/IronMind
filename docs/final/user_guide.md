# IronMind User Guide

## Overview

IronMind is a fitness tracking backend API. It supports account registration, JWT login, profile management, nutrition tracking, exercise tracking, hydration tracking, and reminder schedules.

The project is API-first. Users or testers interact with it through HTTP requests, Swagger locally, or an API client such as Postman.

## Public AWS API

Base URL:

```text
http://15.223.186.37:8080
```

This is the current direct ECS Fargate task endpoint. It can change after task restarts. For a permanent production URL, place an Application Load Balancer in front of the ECS service.

Health check:

```text
GET /health
```

Database connectivity check:

```text
GET /db-test
```

## Local Setup

Prerequisites:

- .NET 10 SDK
- Docker Desktop
- PostgreSQL, if running without Docker

Start the local database and API with Docker Compose:

```bash
docker compose up --build
```

Local API URL:

```text
http://localhost:5235
```

Swagger is available when running in development:

```text
http://localhost:5235/swagger
```

## Register a User

Endpoint:

```text
POST /auth/register
```

Example body:

```json
{
  "email": "student@example.com",
  "password": "Password123!",
  "name": "Student User",
  "dateOfBirth": "2000-01-01",
  "weightKg": 70,
  "heightCm": 175,
  "units": 0,
  "dailyCalorieGoal": 2200,
  "dailyWaterGoalMl": 2500
}
```

Successful response:

```json
{
  "success": true,
  "token": "<jwt-token>",
  "error": null
}
```

Save the token. Authenticated endpoints require this header:

```text
Authorization: Bearer <jwt-token>
```

## Log In

Endpoint:

```text
POST /auth/login
```

Example body:

```json
{
  "email": "student@example.com",
  "password": "Password123!"
}
```

## Profile

Get current profile:

```text
GET /auth/profile
```

Update current profile:

```text
PUT /auth/profile
```

All profile routes require a JWT bearer token.

## Nutrition Tracking

Log a meal:

```text
POST /nutrition/meals
```

Example body:

```json
{
  "foodName": "Chicken bowl",
  "calories": 650,
  "proteinG": 42,
  "carbsG": 58,
  "fatG": 18,
  "openFoodFactsId": null
}
```

Get daily nutrition summary:

```text
GET /nutrition/meals/summary
```

Search food data:

```text
GET /nutrition/food/search?q=banana
```

Update or delete a meal:

```text
PUT /nutrition/meals/{id}
DELETE /nutrition/meals/{id}
```

## Exercise Tracking

Log cardio:

```text
POST /exercise/cardio
```

Example body:

```json
{
  "activityType": "running",
  "durationMinutes": 30,
  "distanceKm": 5,
  "notes": "Easy pace"
}
```

Log strength:

```text
POST /exercise/strength
```

Example body:

```json
{
  "exerciseName": "Bench press",
  "sets": [
    { "setNumber": 1, "reps": 10, "weightKg": 60 },
    { "setNumber": 2, "reps": 8, "weightKg": 65 }
  ],
  "notes": "Good form"
}
```

View workout history:

```text
GET /exercise/history?page=1&pageSize=20
```

Update or delete workouts:

```text
PUT /exercise/cardio/{id}
PUT /exercise/strength/{id}
DELETE /exercise/{id}
```

## Hydration Tracking

Log water:

```text
POST /hydration/water
```

Example body:

```json
{
  "amountMl": 500
}
```

Get daily water summary:

```text
GET /hydration/water/summary
```

Create a reminder schedule:

```text
POST /hydration/reminders
```

Toggle reminder:

```text
PATCH /hydration/reminders/{id}?active=false
```

## Common Troubleshooting

If protected routes return `401 Unauthorized`, log in again and include the bearer token.

If the AWS URL does not respond, verify the URL uses `http`, not `https`, and includes port `8080`.

If `/db-test` fails, check RDS status, ECS task logs, database security groups, and the `ConnectionStrings__Default` secret.
