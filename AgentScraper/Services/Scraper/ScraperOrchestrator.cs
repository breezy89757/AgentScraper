using System.Threading.Tasks;
using AgentScraper.Services.AI;
using AgentScraper.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AgentScraper.Services.Scraper;

public class ScraperOrchestrator
{
    private readonly PlaywrightService _playwright;
    private readonly DomParserService _domParser;
    private readonly IAiService _ai;

    public ScraperOrchestrator(PlaywrightService playwright, DomParserService domParser, IAiService ai)
    {
        _playwright = playwright;
        _domParser = domParser;
        _ai = ai;
    }

    public async Task<AnalysisResult> AnalyzePageAsync(string url, string userGoal)
    {
        // 1. Fetch
        var html = await _playwright.FetchHtmlAsync(url);
        
        // 2. Clean
        var simplifiedHtml = _domParser.GetSimplifiedHtml(html);
        // Truncate if too long (simple protection)
        if (simplifiedHtml.Length > 50000) simplifiedHtml = simplifiedHtml.Substring(0, 50000);

        // 3. AI Analyze
        var systemPrompt = Prompts.SmartScraperSystem;
        var userPrompt = $"Page Title: {url}\n\nUser Goal: {userGoal}\n\nHTML Structure:\n{simplifiedHtml}\n\nPlease analyze and return JSON.";

        var aiResponse = await _ai.ChatAsync(systemPrompt, userPrompt);

        var result = new AnalysisResult { Url = url, Analysis = aiResponse.Content };
        
        // Try cleaning json markdown
        var jsonMatch = Regex.Match(aiResponse.Content, @"```json\s*(\{.*?\})\s*```", RegexOptions.Singleline);
        if (jsonMatch.Success)
        {
            result.Analysis = jsonMatch.Groups[1].Value;
        }

        return result;
    }

    public async Task<ScrapeResult> ExtractDataAsync(string url, string query, bool useVisualMode = false)
    {
        // 1. Fetch
        var pageData = await _playwright.FetchPageDataAsync(url, captureScreenshot: useVisualMode);
        var html = pageData.Html;

        // 2. Clean HTML (preserve structure)
        var simplifiedHtml = _domParser.GetSimplifiedHtml(html);
        if (simplifiedHtml.Length > 50000) simplifiedHtml = simplifiedHtml.Substring(0, 50000);

        // 3. AI Extract
        var systemPrompt = Prompts.DataExtractorSystem;
        // Use simplifiedHtml to preserve table structure (table, tr, td tags)
        var userPrompt = $"User Query: {query}\n\nPage Content:\n{simplifiedHtml}";

        // If visual mode, AI will use both HTML and Image
        var aiResponse = await _ai.ChatAsync(systemPrompt, userPrompt, pageData.ScreenshotBase64);

        return new ScrapeResult 
        { 
            Data = aiResponse.Content, 
            TokenUsage = aiResponse.TotalTokens, 
            DebugHtml = simplifiedHtml,
            DebugScreenshotBase64 = pageData.ScreenshotBase64 // Pass back for UI debug view
        };
    }
}
