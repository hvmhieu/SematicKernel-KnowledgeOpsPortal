namespace KnowledgeOps.Models.Documents;

public sealed class DocumentDetailsViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly LastReviewedOn { get; init; }
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}