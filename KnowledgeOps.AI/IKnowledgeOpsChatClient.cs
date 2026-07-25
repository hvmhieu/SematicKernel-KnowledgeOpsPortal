using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace KnowledgeOps.AI;

public interface IKnowledgeOpsChatClient
{
    Task<string> ReplyAsync(ChatHistory history, CancellationToken cancellationToken = default);
    Task<string> GetCurrentDateAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 
/// </summary>
/// <param name="chatCompletionService"></param>
/// <param name="kernel"></param>
internal sealed class KnowledgeOpsChatClient(
    IChatCompletionService chatCompletionService, Kernel kernel) : IKnowledgeOpsChatClient
{
    public async Task<string> ReplyAsync(ChatHistory history, CancellationToken cancellationToken = default)
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var result = await chatCompletionService.GetChatMessageContentsAsync(
            history,
            settings,
            kernel: kernel,
            cancellationToken: cancellationToken);

        return string.Join("\n", result.Select(x => x.Content));
    }

    public async Task<string> GetCurrentDateAsync(CancellationToken cancellationToken = default)
    {
        var result = await kernel.InvokeAsync("Time", "Today", cancellationToken: cancellationToken);
        return result.ToString();
    }
}