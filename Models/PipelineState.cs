namespace LeadPipeline.Models;

public class PipelineState
{
    public int TotalSteps { get; set; }
    public int MaxConcurrency { get; set; }
    public PipelineStatus Status { get; set; }
    public List<StepState> Steps { get; set; } = [];
}
