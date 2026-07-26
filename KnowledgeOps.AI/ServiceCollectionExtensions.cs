using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Net.Http;
using DefaultNamespace;
using Microsoft.SemanticKernel.Plugins.Core;

namespace KnowledgeOps.AI;
/// <summary>
/// Formula to create extension method -> AddXxx(...) = bind options + register dependencies + register service
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKnowledgeOpsAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<OpenAIOptions>()
            .Bind(configuration.GetSection(OpenAIOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            // get object OpenAIOptions which is registered via DI
            var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            var httpClientHandler = new HttpClientHandler
            {
                CheckCertificateRevocationList = !options.DisableCertificateRevocationCheck
            };
            var httpClient = new HttpClient(httpClientHandler, disposeHandler: true);

            var builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
            builder.AddOpenAIChatCompletion(
                modelId: options.ModelId,
                endpoint: new Uri(options.Endpoint),
                apiKey: options.ApiKey,
                orgId: null,
                serviceId: null,
                httpClient: httpClient);
            builder.Plugins.AddFromType<TimePlugin>("Time");
            builder.Plugins.AddFromType<ConversationSummaryPlugin>("ConversationSummary");
            builder.Plugins.AddFromType<BusinessRequestPlugin>("BusinessRequests");
            
            builder.Services.AddSingleton<IBusinessRequestRepository, InMemoryBusinessRequestRepository>();
            var kernel = builder.Build();
            
            var requestOperationsPluginPath = Path.Combine(
                AppContext.BaseDirectory,
                "Plugins",
                "RequestOperationsPlugin");
            kernel.ImportPluginFromPromptDirectory(requestOperationsPluginPath, "RequestOperations");
            
            return kernel;
        });
        
        // get IChatCompletionService from Kernel
        services.AddSingleton(sp => sp.GetRequiredService<Kernel>()
                                      .GetRequiredService<IChatCompletionService>());
        
        services.AddSingleton<IKnowledgeOpsChatClient, KnowledgeOpsChatClient>();
        return services;
    }
}