# IronMind — Local Database Setup Guide

Follow these steps after cloning the repo. Do this once on your machine before Week 2.

---

## Prerequisites

Make sure you have both of these installed before starting:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) (version 14 or higher)

Verify with:
```bash
dotnet --version   # should show 10.x.x
psql --version     # should show 14 or higher
```

---

## Step 1 — Install the EF Core CLI Tool

This tool lets you create and apply database migrations from the terminal.

```bash
dotnet tool install --global dotnet-ef
```

Then add it to your PATH so it works in every terminal session.

**Mac:**
```bash
echo 'export PATH="$PATH:/Users/$USER/.dotnet/tools"' >> ~/.zprofile
source ~/.zprofile
```

**Windows (PowerShell — run once):**
```powershell
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";$env:USERPROFILE\.dotnet\tools", "User")
```
Then close and reopen your terminal.

Verify it worked:
```bash
dotnet ef --version
```

---

## Step 2 — Create the Database

Open a terminal and run:

**Mac:**
```bash
createdb ironmind_dev
```

**Windows (in psql as the postgres user):**
```sql
psql -U postgres
CREATE DATABASE ironmind_dev;
\q
```

---

## Step 3 — Create Your Local Settings File

`appsettings.Development.json` is gitignored — you must create it yourself. This keeps your credentials off GitHub.

Navigate to `IronMind.API/` and create the file:

**Mac:**
```bash
cd IronMind.API
touch appsettings.Development.json
```

**Windows:** Create the file in File Explorer or your IDE inside `IronMind.API/`.

Paste this content into the file, then fill in your own values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=ironmind_dev;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Secret": "DEV_ONLY_CHANGE_IN_PROD_super_secret_key_32chars!!",
    "Issuer": "IronMind",
    "Audience": "IronMind"
  }
}
```

### Finding your username and password

**Mac (Homebrew PostgreSQL):**
Your username is your macOS system username. Run `whoami` in the terminal to see it. Homebrew installs PostgreSQL with no password by default, so leave `Password` empty:
```
"Default": "Host=localhost;Database=ironmind_dev;Username=your_mac_username;Password="
```

**Windows (PostgreSQL installer):**
Your username is `postgres` and your password is whatever you set when you installed PostgreSQL:
```
"Default": "Host=localhost;Database=ironmind_dev;Username=postgres;Password=your_password_here"
```

---

## Step 4 — Run the Migration

From the root of the repo (`IronMind/`), run:

```bash
dotnet ef database update --project IronMind.Data --startup-project IronMind.API
```

You should see output ending with `Done.`

This creates all the tables in your `ironmind_dev` database. You only need to run this once (and again whenever a new migration is added).

---

## Step 5 — Start the API

```bash
cd IronMind.API
dotnet run
```

Then open your browser and go to:
```
http://localhost:5235/swagger
```

You should see four route groups: **Auth**, **Nutrition**, **Exercise**, **Hydration**.

---

## Step 6 — Test Your Setup

In Swagger, expand **Auth → POST /auth/register** and click **Try it out**.

Send this body:
```json
{
  "email": "test@example.com",
  "password": "Test1234!",
  "name": "Test User",
  "dateOfBirth": "2000-01-01",
  "weightKg": 70,
  "heightCm": 175,
  "units": 0,
  "dailyCalorieGoal": 2000,
  "dailyWaterGoalMl": 2500
}
```

If you get a `200 OK` response with a `token` field, your setup is complete.

**Bring a working environment to the next meeting — this is the gate to Week 2.**

---

## Troubleshooting

**`role "postgres" does not exist` (Mac)**
Your PostgreSQL was installed via Homebrew. Your username is your Mac username, not `postgres`. Run `whoami` and use that in the connection string.

**`connection refused` or `could not connect to server`**
PostgreSQL isn't running. Start it with:
- Mac: `brew services start postgresql@18` (or whatever version you installed)
- Windows: Open **Services** and start **PostgreSQL**

**`dotnet ef not found` after install**
Your PATH doesn't include the dotnet tools directory. Follow Step 1 again and make sure you restarted your terminal.

**`Build failed` when running migrations**
Run `dotnet build` from the repo root first to see the error. Usually a missing package — run `dotnet restore` and try again.
