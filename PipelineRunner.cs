using System.Diagnostics;
using LeadPipeline.Models;

namespace LeadPipeline;

/// <summary>
/// Executes pending pipeline steps with a configurable concurrency limit.
/// Persists state after every completed step so progress survives interruption.
/// </summary>
public class PipelineRunner(PipelineState state, StateManager stateManager)
{
    private readonly Random _random = new();

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var pending = state.Steps
            .Where(s => s.Status == StepStatus.Pending)
            .ToList();

        if (pending.Count == 0)
        {
            Console.WriteLine("All steps already completed — nothing to do.");
            return;
        }

        Console.WriteLine($"Running {pending.Count} pending steps with max concurrency {state.MaxConcurrency}.");

        var stopwatch = Stopwatch.StartNew();
        using var semaphore = new SemaphoreSlim(state.MaxConcurrency, state.MaxConcurrency);

        var tasks = pending.Select(step => ExecuteStepAsync(step, semaphore, stopwatch, cancellationToken));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is expected on Ctrl+C — state was already saved per-step.
            return;
        }

        state.Status = PipelineStatus.Completed;
        await stateManager.SaveAsync(state);
        Console.WriteLine($"\nAll {state.TotalSteps} steps completed in {stopwatch.Elapsed.TotalSeconds:F1}s.");
    }

    private async Task ExecuteStepAsync(
        StepState step,
        SemaphoreSlim semaphore,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var delayMs = _random.Next(1000, 3001);
            await Task.Delay(delayMs, cancellationToken);

            var result = _random.Next(1, 1000).ToString();
            var now = DateTimeOffset.UtcNow;

            step.Status = StepStatus.Completed;
            step.Result = result;
            step.CompletedAt = now;

            await stateManager.SaveAsync(state);

            Console.WriteLine(
                $"Step {step.Step}/{state.TotalSteps} completed (result: {result}) — elapsed: {stopwatch.Elapsed.TotalSeconds:F1}s");
        }
        finally
        {
            semaphore.Release();
        }
    }
}
