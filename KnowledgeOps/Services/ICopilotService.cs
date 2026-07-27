using KnowledgeOps.AI;
using KnowledgeOps.Models.Copilot;
using Microsoft.SemanticKernel.ChatCompletion;

namespace KnowledgeOps.Services;

public interface ICopilotService
{
    Task<CopilotResponse> GetResponseAsync(CopilotRequest request, CancellationToken cancellationToken = default);
}

public sealed class CopilotService(IKnowledgeOpsChatClient chatClient, ILogger<CopilotService> logger) : ICopilotService
{
    public async Task<CopilotResponse> GetResponseAsync(CopilotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException(
                "A message is required before the copilot can respond", nameof(request));
        }

        var history = new ChatHistory();
        history.AddSystemMessage(BuildSystemMessage(request.Context));
        if (request.Context is not null)
        {
            history.AddUserMessage(BuildContextMessage(request.Context));
        }
        
        history.AddUserMessage(request.Message.Trim());
        
        logger.LogInformation(
            "Sending copilot request. Area: {Area}, EntityType: {EntityType}, EntityId: {EntityId}",
            request.Context?.Area,
            request.Context?.EntityType,
            request.Context?.EntityId
            );
        var response = await chatClient.ReplyAsync(history, cancellationToken);

        return new CopilotResponse
        {
            Message = string.IsNullOrWhiteSpace(response)
                ? "I could not generate a response for that request"
                : response,
            ContextSummary = BuildContextSummary(request.Context),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private string? BuildContextSummary(CopilotPageContext? requestContext)
    {
        if (requestContext is null) return null;

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(requestContext.PageTitle))
            parts.Add($"Page: {requestContext.PageTitle.Trim()}");

        if (!string.IsNullOrWhiteSpace(requestContext.EntityType))
        {
            var id = string.IsNullOrWhiteSpace(requestContext.EntityId) ? "(no id)" : requestContext.EntityId.Trim();
            parts.Add($"{requestContext.EntityType.Trim()} ({id})");
        }

        if (!string.IsNullOrWhiteSpace(requestContext.Summary))
            parts.Add(requestContext.Summary.Trim());

        if (requestContext.Metadata is { Count: > 0 })
        {
            var briefMeta = string.Join(", ", requestContext.Metadata
                .Take(3)
                .Select(kv => $"{kv.Key}={kv.Value}"));
            parts.Add($"Meta: {briefMeta}");
        }

        var combined = string.Join(" — ", parts).Trim();
        if (string.IsNullOrWhiteSpace(combined)) return null;

        // Simple redaction for common PII patterns
        combined = System.Text.RegularExpressions.Regex.Replace(
            combined,
            @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b",
            "[REDACTED_EMAIL]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        combined = System.Text.RegularExpressions.Regex.Replace(
            combined,
            @"\b(?:\+?\d[\d\-\s]{7,}\d)\b",
            "[REDACTED_PHONE]");

        const int max = 400;
        if (combined.Length > max) combined = combined.Substring(0, max - 1).TrimEnd() + "…";

        return combined;
    }

    private static string BuildContextMessage(CopilotPageContext requestContext)
    {
        var area = string.IsNullOrWhiteSpace(requestContext?.Area) ? "(unknown)" : requestContext.Area;
        var pageTitle = string.IsNullOrWhiteSpace(requestContext?.PageTitle) ? "(none)" : requestContext.PageTitle;
        var entityType = string.IsNullOrWhiteSpace(requestContext?.EntityType) ? "(none)" : requestContext.EntityType;
        var entityId = string.IsNullOrWhiteSpace(requestContext?.EntityId) ? "(none)" : requestContext.EntityId;
        var summary = string.IsNullOrWhiteSpace(requestContext?.Summary) ? "(none)" : requestContext.Summary;

        var metadata = requestContext?.Metadata != null && requestContext.Metadata.Any()
            ? string.Join("\n", requestContext.Metadata.Select(kv => $"- {kv.Key}: {kv.Value}"))
            : "- (none)";

        return $"""
            Page context for {area}:

            - Page title: {pageTitle}
            - Entity: {entityType} ({entityId})
            - Summary: {summary}

            Metadata:
            {metadata}

            Use this context to inform your response. If required information is missing, explicitly ask for it.
            """;
    }

    private static string BuildSystemMessage(CopilotPageContext? requestContext)
    {
        var area = string.IsNullOrWhiteSpace(requestContext?.Area)
            ? "The KnowledgeOps portal"
            : $"the {requestContext.Area} area of the knowledgeOps portal";
        
        return $"""
                You are the embedded copilot for {area}.

                Your job is to help users understand information, summarize business context,
                clarify next steps, and reason over the current portal workflow.

                Follow these rules:
                - Stay focused on the user's current page and task.
                - Use the supplied page context when it is relevant.
                - Do not claim that you accessed documents, databases, or systems unless that information was supplied.
                - Do not invent missing business facts.
                - If the available context is not enough, say what additional information would be needed.
                - Keep responses professional, practical, and concise.
                """;
    }
}