using System.Text.Json.Serialization;

namespace LeadPipeline.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PipelineStatus
{
    InProgress,
    Completed
}
