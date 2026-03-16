namespace LeadPipeline;
public class Program
{
    private static readonly string StateFile = "snapshot-state.json";
    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 2
            || !int.TryParse(args[0], out int totalSteps)
            || !int.TryParse(args[1], out int maxConcurrency)
            || totalSteps < 1
            || maxConcurrency < 1)
        {
            Console.WriteLine("Usage: lead-pipeline <totalSteps> <maxConcurrency>");
            Console.WriteLine("  totalSteps    — total number of steps to execute (>= 1)");
            Console.WriteLine("  maxConcurrency — maximum steps running in parallel (>= 1)");
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine("Valid parameters not found, using deafults totalSteps 10, maxConcurrency 1");
            totalSteps = 10;
            maxConcurrency = 1;
        }

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent immediate process termination
            Console.WriteLine("\nInterrupt received — saving state and exiting...");
            cts.Cancel();
        };

        var stateManager = new StateManager(StateFile);
        var state = stateManager.LoadOrCreate(totalSteps, maxConcurrency);

        var runner = new PipelineRunner(state, stateManager);

        await runner.RunAsync(cts.Token);

        return 0;
    }
}