# Lead Pipeline

This repo was created using Claude Code, for the purpose of creating a sample application for a Lead role.

The following prompt was used to create the code, and then slightly modified to my liking.

## Prompt

> Create a C# console application that demonstrates a step-based job pipeline with checkpoint/resume capability.
>
> The program takes two arguments: total number of steps, and max concurrent steps.
>
> **Behavior:**
>
> - On start, check for a state file (e.g. `snapshot-state.json`). If an incomplete run exists, resume from the last completed step.
> - Each step simulates work by awaiting a random delay (e.g. 1-3 seconds) and produces a fixed result (e.g. `"Step N result: {random number}"`).
> - Steps are executed respecting the concurrency limit. With concurrency 1, they run sequentially. With concurrency > 1, independent steps run in parallel.
> - After each step completes, persist the updated state to the state file (step number, result, status).
> - Print progress to the console in real-time: `"Step 3/10 completed (result: 42) — elapsed: 4.2s"`
> - When all steps finish, mark the run as complete in the state file.
> - If the process is interrupted (Ctrl+C) and restarted, it should resume from where it left off.
>
> State file should be human-readable JSON, something like:
>
> ```json
> {
>   "totalSteps": 10,
>   "maxConcurrency": 3,
>   "status": "in_progress",
>   "steps": [
>     { "step": 1, "status": "completed", "result": "42", "completedAt": "..." },
>     { "step": 2, "status": "completed", "result": "17", "completedAt": "..." },
>     { "step": 3, "status": "pending" }
>   ]
> }
> ```
>
> Keep it clean and simple. Focus on good naming, clear separation of concerns, and idiomatic async C#. No external dependencies beyond what's in the .NET SDK.
