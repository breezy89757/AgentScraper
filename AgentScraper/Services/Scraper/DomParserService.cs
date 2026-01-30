using HtmlAgilityPack;
using System.Linq;

namespace AgentScraper.Services.Scraper;

public class DomParserService
{
    public string GetSimplifiedHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//svg|//noscript|//iframe|//comment()|//link|//meta");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        // Optional: Remove all attributes to save tokens, only keep meaningful structure
        try 
        {
            var allNodes = doc.DocumentNode.Descendants().ToList();
            foreach (var node in allNodes)
            {
                if (node.Name == "a" || node.Name == "img") 
                {
                     // Keep essential attributes for these tags, remove others if needed
                     // For simplicity, just keep them as is or clean specific ones
                     node.Attributes.Remove("style");
                     node.Attributes.Remove("class");
                     node.Attributes.Remove("onclick");
                     continue;
                }
                
                // For other tags, remove attributes that bloat tokens (style, class, etc.)
                // Instead of RemoveAll(), let's remove common noise to be safer/faster
                node.Attributes.Remove("style");
                node.Attributes.Remove("class");
                node.Attributes.Remove("id");
                node.Attributes.Remove("width");
                node.Attributes.Remove("height");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning attributes: {ex.Message}");
            // Continue with partially cleaned doc
        }
        
        return doc.DocumentNode.OuterHtml;
    }

    public string ExtractText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        // Basic text extraction, can be improved
        return System.Web.HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);
    }
}
