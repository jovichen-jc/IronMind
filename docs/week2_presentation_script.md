# IronMind — Week 2 Presentation Script
**Presenter:** Kevin (Phoenix)
**Audience:** 3 teammates
**Duration:** ~15 minutes

---

## OPENING (1 min)

"Welcome back. Week 1 is done.

Everyone has a working local environment — PostgreSQL is running, migrations applied, and you got a JWT back from Swagger. That was the gate and you cleared it.

This week we split up. Each of you owns a module and delivers it solo. Today I'm going to tell you exactly what your job is, what files to touch, and what done looks like."

---

## PART 1 — What Got Built in Week 1 (2 min)

"Here's the full picture of what's already in the repo:

**Auth** — register and login are fully working. JWT is generated, BCrypt hashes the password, user data is saved to the DB. That's done.

**Nutrition** — log a meal, view daily calorie summary, delete a meal — all working. The one gap is food search, which throws a `NotImplementedException`. That's Week 2.

**Exercise** — log cardio sessions and strength workouts, paginated history, delete — all working. The gap is calorie estimation, which currently just multiplies duration by 8. A flat number is wrong. That's Week 2.

**Hydration** — log water, view daily totals, set a reminder schedule, toggle it on or off — all working. The gap is that reminders do nothing — there's no push notification wired up. That's Week 2.

**The DB schema is locked.** One exception: Dev 4 needs to add a `LastNotifiedAt` column to `ReminderSchedules` — that requires a new migration, which I'll explain in their guide.

If any other module needs a schema change this week, you come to me first. We do not change the DB schema independently."

---

## PART 2 — The Four Tracks (4 min)

"Here's the split:

---

**Dev 1 — Auth + Unit Conversion**

Your job is two things.

First: profile endpoints. Right now there's no way for a user to view or update their profile after registering. You're building `GET /auth/profile` and `PUT /auth/profile`.

Second, and more important: the unit conversion layer. We committed to storing everything in metric — kilograms, kilometres, millilitres. But users might want imperial — pounds, miles, ounces. You're building the `UnitConverter` class that all other modules will use to convert values before sending them back in API responses.

You don't need to wire it into every module this week — that happens in Week 3. You just need to build it and verify it works on the profile endpoint.

---

**Dev 2 — Nutrition + Open Food Facts**

Your job is to implement food search.

There's already a `SearchFoodAsync` method in `NutritionService` — it currently throws a `NotImplementedException`. You're replacing that with a real HTTP call to the Open Food Facts API. No API key required. Free. 2.8 million products.

The route is already registered at `GET /nutrition/food/search?q=banana`. You just need the service to return real data instead of crashing.

---

**Dev 3 — Exercise + Calorie Estimation**

Your job is to fix how we estimate calories burned during cardio.

Right now it's `duration × 8`. That's wrong — a 30-minute swim and a 30-minute walk don't burn the same calories.

The real formula is MET-based: `Calories = MET × user_weight_kg × (duration_minutes / 60)`. MET is a number that represents how hard an activity works the body. Running has a higher MET than walking.

You're building a MET lookup table per activity type and wiring it into `LogCardioAsync`. You'll also need to fetch the user's weight from the DB since the formula needs it.

---

**Dev 4 — Hydration + FCM Push Notifications**

Your job is to make reminders actually fire.

The reminder schedule is already saved in the DB — start time, end time, interval. The user's device token field already exists on the User model. What doesn't exist is anything that reads those schedules and sends a notification.

You're building two things: an endpoint to register a device token (`PATCH /auth/device-token`), and a background service that runs on a timer, checks active reminder schedules, and fires Firebase push notifications to the right users.

This is the most involved track. Your guide has the full breakdown."

---

## PART 3 — The Two Rules (2 min)

"Two rules that make parallel work not a disaster:

**Rule 1 — Never call another module's code directly.**
If Dev 3 needs the user's weight, they go through the DB context, not through `AuthService`. If Dev 2 needs to know if a food item was already logged, they look in the `MealLogs` table directly. You do not import another module's service class into yours.

**Rule 2 — If you need a schema change, tell me before you branch.**
The only planned schema change this week is Dev 4's `LastNotifiedAt` column. If something unexpected comes up, we discuss it as a team. Schema changes affect everyone.

That's it. Two rules."

---

## PART 4 — How to Work This Week (2 min)

"Each of you has an individual guide in the `docs/` folder of the repo. Read it before you write a single line of code.

The guide tells you:
- Which files to open
- What to add or change
- How to test it in Swagger
- What done looks like

Work on your own branch:
```bash
git checkout -b dev1/auth-units      # or dev2/nutrition-food, dev3/exercise-calories, dev4/hydration-fcm
```

Push your branch during the week so I can see progress. Do not merge to main until Week 3.

If you're stuck for more than 30 minutes on something, message the group chat. Don't sit on a blocker."

---

## PART 5 — What Done Looks Like (1 min)

"At the next meeting, each track needs to demo their feature live in Swagger.

**Dev 1:** Show profile endpoint returning data. Show a metric user getting kg/km, an imperial user getting lbs/miles.

**Dev 2:** Search for a real food by name and get back real results from Open Food Facts.

**Dev 3:** Log a cardio session and show a calorie estimate that's based on activity type and user weight — not a flat multiplier.

**Dev 4:** Show a device token being saved. Show that the background service is running and that a notification fires for an active reminder schedule.

If it works in Swagger, it's done. If it doesn't run, it's not done."

---

## CLOSING (1 min)

"Pull the latest from main before you start — there are new docs in the `docs/` folder including your individual guide.

```bash
git pull origin main
git checkout -b your-branch-name
```

Any questions on your specific track before we break?"

---

## Q&A Prep

**Q: What if my feature depends on something another dev hasn't built yet?**
A: Build your feature against the existing DB and models. Don't wait on another dev's code. If you need data they're producing, query the DB directly.

**Q: Can I add new DTOs or interfaces?**
A: Yes — `IronMind.Core` is fair game for additions. Don't modify existing DTOs or interfaces without telling the group, since other people's code depends on them.

**Q: What if Open Food Facts API is slow or down?**
A: Dev 2, add a reasonable timeout (5 seconds) and return an empty list on failure — don't let a flaky external API crash the whole endpoint.

**Q: Do we need unit tests this week?**
A: No. Week 3. Focus on getting the feature working in Swagger first.
