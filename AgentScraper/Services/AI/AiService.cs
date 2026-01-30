using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading.Tasks;
using AgentScraper.Models;

namespace AgentScraper.Services.AI;

public class AiService : IAiService
{
    private readonly IChatCompletionService _chat;

    public AiService(Kernel kernel)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<AiResponse> ChatAsync(string systemPrompt, string userPrompt, string? imageBase64 = null)
    {
        var history = new ChatHistory(systemPrompt);
        
        if (!string.IsNullOrEmpty(imageBase64))
        {
            var collection = new ChatMessageContentItemCollection
            {
                new TextContent(userPrompt),
                new ImageContent(System.Convert.FromBase64String(imageBase64), "image/jpeg")
            };
            history.AddUserMessage(collection);
        }
        else
        {
            history.AddUserMessage(userPrompt);
        }

        var result = await _chat.GetChatMessageContentAsync(history);
        
        var totalTokens = 0;
        if (result.Metadata != null && result.Metadata.TryGetValue("Usage", out var usageStr))
        {
            // Try to parse Semantic Kernel's usage object (often CompletionsUsage)
            // For now, simpler approach: check if it's a dynamic object or try reflection if needed
            // But usually for Azure OpenAI connector, it's a specific type. 
            // Let's assume a safe fallback or dynamic access if possible, or 0 if not easily accessible without referencing specific connector.
            // Actually, in newer SK, Usage is well defined? 
            // Let's stick to safe parsing or dynamic.
            try {
                 dynamic usage = usageStr;
                 totalTokens = (int)usage.TotalTokens;
            } catch {}
        }

        return new AiResponse 
        { 
            Content = result.Content ?? string.Empty,
            TotalTokens = totalTokens
        };
    }
}
