# AgentScraper (v1.0)

<div align="center">
  <img src="AgentScraper/wwwroot/images/logo.png" height="200" alt="Scrappy" />
</div>



**AgentScraper** is a next-generation AI-powered web scraping assistant built with **.NET 10**, **Blazor**, and **Microsoft Playwright**. It leverages **Generative AI (gpt-5.2-chat)** to turn unstructured web content into structured data, bypassing the fragility of traditional CSS selector-based scraping.

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Status](https://img.shields.io/badge/status-Active-success.svg) ![AI](https://img.shields.io/badge/AI-Powered-purple.svg)

## 🚀 Key Features

### 1. Dual-Mode Extraction
The core innovation of AgentScraper is its ability to "see" the web in two dimensions:
*   **Structure Mode (HTML)**: Efficiently parses simplified HTML DOM to extract text, tables, and lists. Fast and cost-effective.
*   **Visual Mode (Multimodal)**: **[NEW!]** Uses **gpt-5.2-chat Vision** to analyze screenshots along with code. Perfect for extracting data from charts, canvas elements, complex dashboards, or highly obfuscated sites.

### 2. Intelligent Parsing
*   **Smart Analysis**: Automatically identifies the best scraping strategy based on your natural language goal.
*   **Token-Optimized**: A custom `DomParserService` cleans noise (scripts, styles, ads) from HTML before sending it to the AI, saving ~60% of tokens per request.

### 3. Crystallized Skills **[NEW!]**
*   **Knowledge Persistence**: After a successful AI extraction, "crystallized" the strategy into a permanent **Skill**.
*   **Zero-Shot Execution**: Reuse skills for similar URLs to bypass AI reasoning costs and achieve near-instant results.
*   **Skill Dashboard**: Browse, search, and manage your library of scraping skills.

### 4. Developer Experience
*   **Debug View**: Inspect exactly what the AI sees—both the simplified HTML code and the visual snapshot.
*   **Token Tracking**: Real-time monitoring of AI token consumption to help you manage costs.
*   **Stealth Mode**: Built on Playwright with custom headers to mimic real user behavior.

## ⚡ Quick Start

1. **Setup**: Clone the repo and update `appsettings.json` with your OpenAI/Azure keys.
2. **Install Browsers**: Run `pwsh bin/Debug/net10.0/playwright.ps1 install` (if Playwright complains).
3. **Run**: `dotnet run` and visit `http://localhost:5250`.

## 📖 Usage Guide

1. **Enter Target URL**: The site you want to scrape.
2. **Define Goal**: Tell the AI what you want in natural language.
3. **Select Mode & Auto-Match**: 
   *   Toggle **Visual Mode** ON for visual-heavy sites.
   *   Leave **Auto-Match Skills** ON (Default) to automatically reuse your saved skills.
4. **Scrape & Crystallize**: If the result is perfect, click **"Crystalize Skill"** to save the logic.
5. **Manage**: Visit **"My Skills"** to view details or manually trigger a skill for any URL.

## 📝 Release Notes

### v1.1 (2026-01-31) - Skill Transformation
*   **Feature**: Introduced **"Crystallized Skills"** architecture.
*   **UI**: Added **"My Skills"** Dashboard with dialog-based skill inspection.
*   **Logic**: Added `RecipeService` for markdown-based skill persistence.
*   **Fix**: Resolved form validation deadlocks and improved URL pre-fill from skills.



---
*Created by breezy89757*
