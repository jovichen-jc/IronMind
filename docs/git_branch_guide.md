# IronMind — Git Branch & Merge Guide

This guide covers the full workflow for Week 2: creating your branch, working on it, and merging it back to main in Week 3.

---

## The Big Picture

```
main ──────────────────────────────────────────── merge week ──▶
        │           │           │           │
        dev1/...    dev2/...    dev3/...    dev4/...
```

Everyone branches off the same point. You work independently. Week 3 we merge everything back.

---

## Step 1 — Get the Latest Code

Before you create your branch, make sure you're on main and up to date:

```bash
git checkout main
git pull origin main
```

Never branch off old code.

---

## Step 2 — Create Your Branch

```bash
git checkout -b dev1/auth-units
```

Replace `dev1/auth-units` with your track name:

| Track | Branch name |
|-------|-------------|
| Dev 1 | `dev1/auth-units` |
| Dev 2 | `dev2/nutrition-food` |
| Dev 3 | `dev3/exercise-calories` |
| Dev 4 | `dev4/hydration-fcm` |

You are now on your own branch. Changes here do not affect main or anyone else's branch.

---

## Step 3 — Do Your Work

Write code, test in Swagger, repeat. As you finish pieces, commit your progress:

```bash
git add IronMind.Services/ExerciseService.cs
git commit -m "Add MET lookup table and update calorie estimate formula"
```

Commit often — small commits are easier to review and easier to undo if something breaks. You do not need to push after every commit, but push at least once per day so your progress is visible.

---

## Step 4 — Push Your Branch to GitHub

The first time you push a new branch:

```bash
git push -u origin dev1/auth-units
```

After that, just:

```bash
git push
```

---

## Step 5 — Keep Your Branch Up to Date

If main gets new commits while you're working (for example, Dev 4 pushes their migration), pull those changes into your branch so you stay in sync:

```bash
git fetch origin
git merge origin/main
```

Do this at least once before you start the merge in Week 3.

If there are conflicts (two people edited the same file), Git will mark them like this:

```
<<<<<<< HEAD
your version of the code
=======
their version of the code
>>>>>>> origin/main
```

Open the file, decide which version is correct (or combine them), delete the conflict markers, then:

```bash
git add <the-file>
git commit -m "Resolve merge conflict in ExerciseService"
```

---

## Step 6 — Merging Into Main (Week 3)

When your feature is done and tested, you merge it into main. Do this together as a team so conflicts are resolved with everyone present.

**On your machine:**

```bash
# Make sure main is current
git checkout main
git pull origin main

# Merge your branch in
git merge dev1/auth-units
```

If there are conflicts, resolve them the same way as Step 5.

Once clean:

```bash
git push origin main
```

Then the next person does the same with their branch.

**Order for merging (Week 3):**
1. Dev 4 first — their migration affects the DB schema
2. Dev 1, Dev 2, Dev 3 in any order after that

---

## Everyday Commands

| What you want to do | Command |
|---------------------|---------|
| See what branch you're on | `git branch` |
| See what files changed | `git status` |
| See what changed inside files | `git diff` |
| Stage a specific file | `git add path/to/file.cs` |
| Stage all changed files | `git add .` |
| Commit staged changes | `git commit -m "your message"` |
| Push to GitHub | `git push` |
| Pull latest from GitHub | `git pull` |
| Switch to a different branch | `git checkout branch-name` |
| See recent commits | `git log --oneline -10` |

---

## Rules for This Project

1. **Never commit directly to main.** Always work on your branch.
2. **Never force push** (`git push --force`). If Git rejects your push, ask first.
3. **One migration per person.** Only Dev 4 creates a migration this week. If you think you need a schema change, talk to Kevin first.
4. **Don't edit another dev's service file.** If you need something from another module, read from the DB directly.
5. **Commit messages should say what changed**, not just "fix" or "update". Example: `"Replace flat calorie estimate with MET-based formula"`.

---

## If Something Goes Wrong

**Accidentally committed to main:**
```bash
git checkout -b your-branch-name   # save your work to a new branch
git checkout main
git reset --hard origin/main       # put main back to where it was on GitHub
```

**Want to undo your last commit (but keep the changes):**
```bash
git reset --soft HEAD~1
```

**Want to throw away all uncommitted changes:**
```bash
git checkout .
```

When in doubt, ask before running any command with `reset`, `rebase`, or `--force` in it.
