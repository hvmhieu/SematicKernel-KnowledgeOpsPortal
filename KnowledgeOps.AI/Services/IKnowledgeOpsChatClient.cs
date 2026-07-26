using KnowledgeOps.AI.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace KnowledgeOps.AI;

public interface IKnowledgeOpsChatClient
{
    Task<string> ReplyAsync(ChatHistory history, CancellationToken cancellationToken = default);
    Task<string> GetCurrentDateAsync(CancellationToken cancellationToken = default);
    Task<string> AskWithPromptAsync(string prompt, CancellationToken cancellationToken = default);

    Task<string> SummarizeRequestAsync(string requestTitle,
        string requestDetails,
        string audience,
        CancellationToken cancellationToken = default);
    
    Task<string> CreateOperationsBriefAysnc (string requestDetails, CancellationToken cancellationToken = default);
    Task<string> GetBusinessRequestAsync(string requestId, CancellationToken cancellationToken = default);
    Task<string> GetOpenBusinessRequestAsync(CancellationToken cancellationToken = default);
    Task<string> CreateRequestBriefFromPluginAsync(string requestId, CancellationToken cancellationToken = default);
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

    public async Task<string> AskWithPromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
        return result.ToString();
    }

    public async Task<string> SummarizeRequestAsync(string requestTitle, string requestDetails,
                                             string audience, CancellationToken cancellationToken = default)
    {
        var arguments = new KernelArguments
        {
            ["requestTitle"] = requestTitle,
            ["requestDetails"] = requestDetails,
            ["audience"] = audience
                
        };

        var result = await kernel.InvokePromptAsync(
            KnowledgeOpsPromptTemplates.RequestSummary,
            arguments,
            cancellationToken: cancellationToken);

        return result.ToString();
    }

    public async Task<string> CreateOperationsBriefAysnc(string requestDetails, CancellationToken cancellationToken = default)
    {
        var arguments = new KernelArguments
        {
            ["requestDetails"] = requestDetails
        };

        var result = await kernel.InvokeAsync(
            "RequestOperations",
            "CreateOperationsBrief",
            arguments,
            cancellationToken
        );
        return result.ToString();
    }

    public async Task<string> GetBusinessRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        var arguments = new KernelArguments
        {
            ["requestId"] = requestId
        };
        var result = await kernel.InvokeAsync(
            pluginName: "BusinessRequests",
            functionName: "get_request_by_id",
            arguments: arguments,
            cancellationToken: cancellationToken
        );
        return result.ToString();
    }

    public async Task<string> GetOpenBusinessRequestAsync(CancellationToken cancellationToken = default)
    {
        var result = await kernel.InvokeAsync(
            pluginName: "BusinessRequests",
            functionName: "get_open_requests",
            cancellationToken: cancellationToken);
        
        return result.ToString();
    }

    public async Task<string> CreateRequestBriefFromPluginAsync(string requestId, CancellationToken cancellationToken = default)
    {
        var arguments = new KernelArguments
        {
            ["requestId"] = requestId
        };
        
        var result = await kernel.InvokePromptAsync(
            promptTemplate: KnowledgeOpsPromptTemplates.RequestBriefFromPlugin,
            arguments: arguments,
            cancellationToken: cancellationToken);
        
        return result.ToString();
    }
}