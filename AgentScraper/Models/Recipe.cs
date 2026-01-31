using System;

namespace AgentScraper.Models;

public class Recipe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Friendly name for the recipe (e.g., "PChome Product Details")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Regex pattern to match the target URL
    /// </summary>
    public string UrlPattern { get; set; } = string.Empty;

    /// <summary>
    /// The actual original URL used during discovery (for pre-filling UI)
    /// </summary>
    public string DiscoveryUrl { get; set; } = string.Empty;

    /// <summary>
    /// The "Open Book" instruction (System Prompt) for the AI
    /// </summary>
    public string SkillDefinition { get; set; } = string.Empty;

    /// <summary>
    /// Optional: CSS selector to wait for before scraping (async content)
    /// </summary>
    public string? WaitSelector { get; set; }

    /// <summary>
    /// When this recipe was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
