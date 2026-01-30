using System.Threading.Tasks;
using AgentScraper.Models;

namespace AgentScraper.Services.AI;

public interface IAiService
{
    Task<AiResponse> ChatAsync(string systemPrompt, string userPrompt, string? imageBase64 = null);
}
