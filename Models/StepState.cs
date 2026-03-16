namespace LeadPipeline.Models;

public class StepState
{
    public int Step { get; set; }
    public StepStatus Status { get; set; }
    public string? Result { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
