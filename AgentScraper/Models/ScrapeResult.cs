namespace AgentScraper.Models;

public class ScrapeResult
{
    public string Data { get; set; } = string.Empty;
    public int TokenUsage { get; set; }
    public string? DebugHtml { get; set; }
    public string? DebugScreenshotBase64 { get; set; }
}
