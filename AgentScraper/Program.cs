using AgentScraper.Components;
using MudBlazor.Services;
using AgentScraper.Services.Scraper;
using AgentScraper.Services.AI;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Domain Services
builder.Services.AddScoped<PlaywrightService>();
builder.Services.AddScoped<DomParserService>();
builder.Services.AddScoped<ScraperOrchestrator>();

// AI Service Configuration
var azureOpenAI = builder.Configuration.GetSection("AzureOpenAI");
builder.Services.AddKernel().AddAzureOpenAIChatCompletion(
    deploymentName: azureOpenAI["DeploymentName"]!,
    endpoint: azureOpenAI["Endpoint"]!,
    apiKey: azureOpenAI["ApiKey"]!
);
builder.Services.AddScoped<IAiService, AiService>();
// builder.Services.AddScoped<IAiService, MockAiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
