using System.Threading.Tasks;
using AgentScraper.Models;

namespace AgentScraper.Services.AI;

public class MockAiService : IAiService
{
    public Task<AiResponse> ChatAsync(string systemPrompt, string userPrompt, string? imageBase64 = null)
    {
        // Simulate network delay
        System.Threading.Thread.Sleep(500);

        if (systemPrompt.Contains("SmartScraper"))
        {
            return Task.FromResult(new AiResponse 
            {
                Content = @"```json
{
    ""target_description"": ""Article Title"",
    ""suggested_selectors"": [""h1"", "".title"", ""#main-heading""],
    ""data_structure"": {""title"": ""string""},
    ""page_type"": ""single""
}
```",
                TotalTokens = 150 
            });
        }
        
        if (systemPrompt.Contains("DataExtractor"))
        {
            return Task.FromResult(new AiResponse
            {
                Content = "Based on the page content, the domain is example.com and the main heading is 'Example Domain'.",
                TotalTokens = 230
            });
        }

        return Task.FromResult(new AiResponse { Content = "Mock AI Response: I received your request but I am in mock mode.", TotalTokens = 50 });
    }
}
