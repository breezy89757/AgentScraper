using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using AgentScraper.Models;

namespace AgentScraper.Services.Scraper;

public class PlaywrightService : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task<string> FetchHtmlAsync(string url)
    {
        var data = await FetchPageDataAsync(url, captureScreenshot: false);
        return data.Html;
    }

    public async Task<PageData> FetchPageDataAsync(string url, bool captureScreenshot = false)
    {
        if (_playwright == null)
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        var page = await _browser.NewPageAsync(new BrowserNewPageOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        });

        try
        {
            // Wait for network idle
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000 });
            
            // Add a small fixed delay (3s) to catch delayed scripts (e.g., setTimeout(..., 1000)) that run AFTER load
            // This is crucial for simple universal scraping of "delayed start" SPAs
            await page.WaitForTimeoutAsync(3000);
            
            var content = await page.ContentAsync();
            string? screenshotBase64 = null;

            if (captureScreenshot)
            {
                var bytes = await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true, Type = ScreenshotType.Jpeg, Quality = 50 });
                screenshotBase64 = Convert.ToBase64String(bytes);
            }

            return new PageData { Html = content, ScreenshotBase64 = screenshotBase64 };
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }
}
