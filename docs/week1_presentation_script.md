# IronMind — Week 1 Presentation Script
**Presenter:** Kevin (Phoenix)  
**Audience:** 3 teammates  
**Duration:** ~15 minutes

---

## OPENING (1 min)

"Hey everyone, welcome to the first official IronMind team meeting.

IronMind is our fitness tracking app — and by the end of today you'll know exactly what we're building, why we made the decisions we made, and what your job is for this first week.

Let's get into it."

---

## PART 1 — What Are We Building? (2 min)

"IronMind is a C# REST API that helps users track three things:

- **What they eat** — calories and macros
- **How they train** — cardio sessions and strength workouts
- **How much they drink** — water intake, with reminders

Users log in, track their day, and the app keeps them on target.

No frontend yet — we're building the API first and testing everything through Swagger. This keeps us focused and moving fast."

---

## PART 2 — The Mind Map (2 min)

*[Show the mind map from the planning doc or draw it on a whiteboard]*

"Here's how the whole system breaks down:

At the top we have the **User** — they register, log in, set their profile and goals.

Under that we have four modules:
- **Nutrition** — log meals, search a food database, see daily calorie summaries
- **Exercise** — log cardio with duration and distance, or strength training with sets, reps, and weight
- **Hydration** — log water intake and set reminder intervals
- **Dashboard** — today's summary, weekly history, streaks

Every single feature traces back to one of these four modules. If someone asks 'should we build X?' — the answer is yes only if it fits in this map."

---

## PART 3 — Key Decisions We Locked In (3 min)

"Before writing a single line of code, we made five decisions as a team. These are not up for debate anymore — they're locked.

**1. Food Database → Open Food Facts API**
Free, no API key needed, 2.8 million products from 150 countries. We call it through a dedicated FoodService class so we can swap it later if needed.

**2. Reminders → Push Notifications via Firebase FCM**
We store a device token per user and fire notifications from a background service in .NET.

**3. Units → User-selectable metric or imperial**
Critical rule: we ALWAYS store metric in the database — kilograms, kilometres, millilitres. We only convert to imperial when we send the response back to the user. This means no unit bugs hiding in our data.

**4. Frontend → API-first**
Swagger is our UI for now. Every endpoint gets documented. Adding a frontend later will be clean because the contract is already solid.

**5. Deployment → Local dev now, Azure-ready later**
No hard-coded connection strings. Everything uses IConfiguration and environment variables. Moving to Azure later is just a config change."

---

## PART 4 — The Architecture (3 min)

*[Show the folder structure on screen or draw on whiteboard]*

"Here's the solution structure. Five projects, each with a clear job:

```
IronMind.Core       → The contract. Models + interfaces + DTOs.
                      Everyone reads this. Nobody changes it alone.

IronMind.Data       → The database layer. EF Core, DbContext, migrations.

IronMind.Services   → Business logic. One service per module.

IronMind.API        → The entry point. Routes, JWT auth, Swagger.

IronMind.Tests      → xUnit tests. Empty for now, we fill it in Week 3.
```

The dependency flow goes one way only:
API → Services → Data → Core

**Core knows nothing about anyone. Data knows only Core. Services know Core and Data. API knows everything.**

Why does this matter? Because in Week 2, you'll each be working on a different module at the same time. If you follow this structure, your code won't break anyone else's."

---

## PART 5 — The 3-Week Plan (2 min)

"We cut the original 6-week plan in half by working in parallel. Here's how:

**Week 1 — All 4 of us together:**
Set up PostgreSQL locally, run the first EF Core migration, test register and login through Swagger. Everyone starts from the same working baseline.

**Week 2 — We split into 4 tracks:**
- Dev 1 → Auth + unit conversion layer
- Dev 2 → Calorie endpoints + Open Food Facts integration  
- Dev 3 → Exercise endpoints + calorie burn estimation
- Dev 4 → Water intake endpoints + FCM push notification service

**Week 3 — Integration + polish:**
Each dev finishes edge cases, then last 2 days are cross-team testing through Swagger.

The two rules that make parallel work actually work:
1. Agree on the DB schema before anyone branches at the end of Week 1
2. Never call another module's code directly — always go through the service interface"

---

## PART 6 — Your Week 1 Tasks (2 min)

"Here's what every one of you needs to do before our next meeting:

**Step 1 — Clone the repo:**
```bash
git clone https://github.com/jovichen-jc/IronMind.git
cd IronMind
```

**Step 2 — Set up your local database:**
Create a PostgreSQL database called `ironmind_dev` and update `appsettings.Development.json` with your own credentials. This file is gitignored — it never gets committed.

**Step 3 — Run the migration:**
```bash
dotnet ef migrations add InitialSchema --project IronMind.Data --startup-project IronMind.API
dotnet ef database update --project IronMind.Data --startup-project IronMind.API
```

**Step 4 — Start the API and test it:**
```bash
cd IronMind.API
dotnet run
```
Open `http://localhost:5235/swagger` — you should see all four route groups: Auth, Nutrition, Exercise, Hydration.

Try registering a user and logging in. If you get a JWT token back, your setup is working.

**Come to the next meeting with a working local environment. That's the gate to Week 2.**"

---

## CLOSING (1 min)

"That's it. We have a clear plan, a clean codebase, and a 3-week timeline.

The repo is at: **github.com/jovichen-jc/IronMind**

Any questions before we get to work?"

---

## Q&A Prep — Common Questions

**Q: Why PostgreSQL and not SQL Server?**
A: PostgreSQL is free, open-source, runs on every platform, and is industry standard for new projects. Azure supports it natively when we deploy.

**Q: Why C# and not Node or Python?**
A: This is a Cloud Database Management program. C# with ASP.NET Core is Microsoft's stack — it maps directly to what we're learning and what Azure is built around.

**Q: What if I've never used EF Core?**
A: You don't need to understand it deeply yet. Follow the migration steps and it handles the database for you. We'll walk through it together in Week 1.

**Q: What is Swagger exactly?**
A: It's an auto-generated web UI that reads our API and lets you test every endpoint in the browser — like Postman but built right in. Go to `/swagger` when the app is running.
