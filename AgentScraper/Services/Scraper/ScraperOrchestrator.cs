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
    private readonly RecipeService _recipeService;

    public ScraperOrchestrator(
        PlaywrightService playwright, 
        DomParserService domParser, 
        IAiService ai,
        RecipeService recipeService)
    {
        _playwright = playwright;
        _domParser = domParser;
        _ai = ai;
        _recipeService = recipeService;
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


    public async Task<ScrapeResult> ExtractDataAsync(string url, string query, bool useVisualMode = false, Guid? selectedRecipeId = null, bool disableAutoMatch = false)
    {
        // Try to find the recipe
        Recipe? recipe = null;
        
        if (selectedRecipeId.HasValue)
        {
            recipe = _recipeService.GetAllRecipes().FirstOrDefault(r => r.Id == selectedRecipeId.Value);
        }
        else if (!disableAutoMatch)
        {
            recipe = await _recipeService.GetMatchingRecipeAsync(url);
        }

        // 1. Fetch
        PageData pageData;
        if (recipe != null && !string.IsNullOrEmpty(recipe.WaitSelector))
        {
            // RECIPE MODE with WAIT: Use specific wait selector
            pageData = await _playwright.FetchPageDataAsync(url, captureScreenshot: useVisualMode, waitSelector: recipe.WaitSelector);
        }
        else
        {
            pageData = await _playwright.FetchPageDataAsync(url, captureScreenshot: useVisualMode);
        }
        
        var html = pageData.Html;

        // 2. Clean HTML (preserve structure)
        var simplifiedHtml = _domParser.GetSimplifiedHtml(html);
        if (simplifiedHtml.Length > 50000) simplifiedHtml = simplifiedHtml.Substring(0, 50000);

        // 3. Prompt Selection (Recipe vs Default)
        string systemPrompt;
        string userPrompt;

        if (recipe != null)
        {
            // RECIPE MODE: Open Book
            systemPrompt = recipe.SkillDefinition;
            userPrompt = $"User Query: {query}\n\nTarget URL: {url}\n\nPage Content:\n{simplifiedHtml}";
        }
        else
        {
            // EXPLORE MODE: Default
            systemPrompt = Prompts.DataExtractorSystem;
            userPrompt = $"User Query: {query}\n\nPage Content:\n{simplifiedHtml}";
        }

        // If visual mode, AI will use both HTML and Image
        var aiResponse = await _ai.ChatAsync(systemPrompt, userPrompt, pageData.ScreenshotBase64);

        return new ScrapeResult 
        { 
            Data = aiResponse.Content, 
            TokenUsage = aiResponse.TotalTokens, 
            DebugHtml = simplifiedHtml,
            DebugScreenshotBase64 = pageData.ScreenshotBase64, // Pass back for UI debug view
            UsedRecipeName = recipe?.Name
        };
    }

    public async Task<Recipe> GenerateRecipeAsync(string url, string userGoal, string simplifiedHtml, string successfulJson)
    {
        var userPrompt = $@"User Goal: {userGoal}
Target URL: {url}

HTML Content Snippet:
{simplifiedHtml.Substring(0, Math.Min(simplifiedHtml.Length, 10000))}...

Successfully Extracted Data:
{successfulJson}

Please write the Skill System Prompt now.";

        var response = await _ai.ChatAsync(Prompts.RecipeGeneratorSystem, userPrompt);
        var content = response.Content;
        
        // Parse Metadata
        string name = $"{new Uri(url).Host} Extractor";
        string urlPattern = Regex.Escape(new Uri(url).Host).Replace("/", "\\/");
        string? waitSelector = null;

        var nameMatch = Regex.Match(content, @"NAME:\s*(.+)");
        if (nameMatch.Success) 
        {
            name = nameMatch.Groups[1].Value.Trim();
            content = content.Replace(nameMatch.Value, "").Trim();
        }

        var patternMatch = Regex.Match(content, @"URL_PATTERN:\s*(.+)");
        if (patternMatch.Success)
        {
            urlPattern = patternMatch.Groups[1].Value.Trim();
            content = content.Replace(patternMatch.Value, "").Trim();
        }

        var waitMatch = Regex.Match(content, @"WAIT_SELECTOR:\s*(.+)");
        if (waitMatch.Success)
        {
            waitSelector = waitMatch.Groups[1].Value.Trim();
            content = content.Replace(waitMatch.Value, "").Trim();
        }

        // Clean markdown code blocks if present
        if (content.Contains("```markdown"))
        {
             content = Regex.Replace(content, @"^```markdown\r?\n|```$", "", RegexOptions.Multiline).Trim();
        }
        else if (content.Contains("```"))
        {
            content = Regex.Replace(content, @"^```[a-zA-Z]*\r?\n|```$", "", RegexOptions.Multiline).Trim();
        }

        return new Recipe
        {
            Name = name,
            UrlPattern = urlPattern,
            DiscoveryUrl = url, // Store the clean original URL
            SkillDefinition = content,
            WaitSelector = waitSelector
        };
    }
}
