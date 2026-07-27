namespace KnowledgeOps.Models.Copilot;

public class CopilotResponse
{
    public string Message { get; init; } = string.Empty;
    public string? ContextSummary { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}