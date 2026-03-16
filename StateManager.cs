using System.Text.Json;
using System.Text.Json.Serialization;
using LeadPipeline.Models;

namespace LeadPipeline;

/// <summary>
/// Handles loading, creating, and persisting pipeline state to disk.
/// All writes are serialized through a lock to prevent concurrent corruption.
/// </summary>
public class StateManager(string stateFilePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Loads an existing state file if one exists and the run is incomplete,
    /// otherwise creates fresh state for the given parameters.
    /// </summary>
    public PipelineState LoadOrCreate(int totalSteps, int maxConcurrency)
    {
        if (File.Exists(stateFilePath))
        {
            var json = File.ReadAllText(stateFilePath);
            var existing = JsonSerializer.Deserialize<PipelineState>(json, JsonOptions);

            if (existing is not null && existing.Status == PipelineStatus.InProgress)
            {
                var completed = existing.Steps.Count(s => s.Status == StepStatus.Completed);
                Console.WriteLine($"Resuming existing run — {completed}/{existing.TotalSteps} steps already completed.");
                return existing;
            }

            if (existing?.Status == PipelineStatus.Completed)
            {
                Console.WriteLine("Previous run is already complete. Starting fresh.");
            }
        }

        return CreateFresh(totalSteps, maxConcurrency);
    }

    /// <summary>Persists the current state to disk, thread-safely.</summary>
    public async Task SaveAsync(PipelineState state)
    {
        await _writeLock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            await File.WriteAllTextAsync(stateFilePath, json);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static PipelineState CreateFresh(int totalSteps, int maxConcurrency)
    {
        var steps = Enumerable.Range(1, totalSteps)
            .Select(n => new StepState { Step = n, Status = StepStatus.Pending })
            .ToList();

        return new PipelineState
        {
            TotalSteps = totalSteps,
            MaxConcurrency = maxConcurrency,
            Status = PipelineStatus.InProgress,
            Steps = steps
        };
    }
}
