using System.Text.Json;
using AgentScraper.Models;
using System.Text.RegularExpressions;
using System.Text;

namespace AgentScraper.Services.Scraper;

public class RecipeService
{
    private readonly string _recipeFolder;
    private List<Recipe> _cachedRecipes = new();

    public RecipeService(IWebHostEnvironment env)
    {
        _recipeFolder = Path.Combine(env.ContentRootPath, "Recipes");
        if (!Directory.Exists(_recipeFolder))
        {
            Directory.CreateDirectory(_recipeFolder);
        }
    }

    public async Task LoadRecipesAsync()
    {
        _cachedRecipes.Clear();
        // Look for SKILL.md in subdirectories
        // Pattern: Recipes/AgentScraper_*/SKILL.md
        if (!Directory.Exists(_recipeFolder)) return;

        var skillFiles = Directory.GetFiles(_recipeFolder, "SKILL.md", SearchOption.AllDirectories);

        foreach (var file in skillFiles)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(file);
                if (lines.Length == 0) continue;

                var recipe = new Recipe();
                bool inFrontmatter = false;
                var sb = new System.Text.StringBuilder();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.Trim() == "---")
                    {
                        if (i == 0) 
                        {
                            inFrontmatter = true; 
                            continue;
                        }
                        if (inFrontmatter) 
                        {
                            inFrontmatter = false; 
                            continue;
                        }
                    }

                    if (inFrontmatter)
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().ToLower();
                            var value = parts[1].Trim();
                            switch (key)
                            {
                                case "id": 
                                    if (Guid.TryParse(value, out var guid)) recipe.Id = guid; 
                                    break;
                                case "name": recipe.Name = value; break;
                                case "url_pattern": recipe.UrlPattern = value; break;
                                case "discovery_url": recipe.DiscoveryUrl = value; break;
                                case "wait_selector": recipe.WaitSelector = value; break;
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
                
                recipe.SkillDefinition = sb.ToString().Trim();
                if (!string.IsNullOrEmpty(recipe.UrlPattern) && !string.IsNullOrEmpty(recipe.SkillDefinition))
                {
                    _cachedRecipes.Add(recipe);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recipe {file}: {ex.Message}");
            }
        }
    }

    public async Task SaveRecipeAsync(Recipe recipe)
    {
        // Sanitize filename and create folder structure
        var safeName = string.Join("", recipe.Name.Split(Path.GetInvalidFileNameChars()));
        var folderName = $"AgentScraper_{safeName}"; // User requested prefix
        var folderPath = Path.Combine(_recipeFolder, folderName);
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(folderPath, "SKILL.md");

        var waitLine = string.IsNullOrEmpty(recipe.WaitSelector) ? "" : $"\r\nwait_selector: {recipe.WaitSelector}";

        var content = $@"---
id: {recipe.Id}
name: {recipe.Name}
url_pattern: {recipe.UrlPattern}
discovery_url: {recipe.DiscoveryUrl}{waitLine}
created_at: {recipe.CreatedAt:O}
---

{recipe.SkillDefinition}";

        await File.WriteAllTextAsync(filePath, content);
        
        // Update cache locally without waiting for reload
        var existing = _cachedRecipes.FirstOrDefault(r => r.Id == recipe.Id);
        if (existing != null)
        {
            _cachedRecipes.Remove(existing);
        }
        _cachedRecipes.Add(recipe);
    }

    public async Task<Recipe?> GetMatchingRecipeAsync(string url)
    {
        if (_cachedRecipes.Count == 0) 
        {
            await LoadRecipesAsync(); 
        }

        return _cachedRecipes.FirstOrDefault(r => 
        {
            try {
                return Regex.IsMatch(url, r.UrlPattern);
            } catch {
                return false; 
            }
        });
    }

    public List<Recipe> GetAllRecipes() => _cachedRecipes;
}
