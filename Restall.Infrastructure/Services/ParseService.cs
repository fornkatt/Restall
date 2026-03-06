using HtmlAgilityPack;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class ParseService : IParseService
{
    private readonly ILogService _logService;
    private readonly HttpClient _httpClient;
    
    private const string RenoDxUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    private const string RenoDxTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd
    
    private readonly Dictionary<ReShade.Branch, List<string>> _availableReShadeVersions = [];
    private readonly Dictionary<RenoDX.Branch, List<RenoDXTagInfoDto>> _availableRenoDxTags = [];
    // Store by mod name
    private readonly Dictionary<string, List<RenoDXModInfoDto>> _availableWikiModsByName = [];
    private readonly Dictionary<string, RenoDXModPreferenceDto> _modPreferences = [];

    public ParseService(
        ILogService logService,
        HttpClient httpClient
        )
    {
        _logService = logService;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Restall");
    }
    
    public async Task FetchAvailableModVersionsAsync()
    {
        await Task.WhenAll(
            FetchStableReShadeVersionsAsync(),
            FetchLatestRenoDxSnapshotAsync(),
            FetchRenoDxNightlyVersionsAsync(),
            FetchRenoDxWikiModsAsync()
        );
    }

    private async Task FetchStableReShadeVersionsAsync()
    {
        throw new System.NotImplementedException();
    }

    private async Task FetchRenoDxWikiModsAsync()
    {
        throw new System.NotImplementedException();
    }
    
    private async Task FetchLatestRenoDxSnapshotAsync()
    {
        try
        {
            var html = await _httpClient.GetStringAsync(RenoDxTagUrl + "snapshot");
            
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var timeNode = document.DocumentNode.SelectSingleNode("//relative-time");
            DateOnly? date = null;
            if (timeNode is not null)
            {
                var datetime = timeNode.GetAttributeValue("datetime", null);
                if (DateTime.TryParse(datetime, out var parsed))
                    date = DateOnly.FromDateTime(parsed.ToUniversalTime());
            }

            if (date is null)
            {
                await _logService.LogWarningAsync("Failed to parse snapshot release date");
                return;
            }

            var bodyNode = document.DocumentNode.SelectSingleNode(
                "//div[contains(@class, 'markdown-body')]");
            var commitNotes = new List<string>();

            if (bodyNode is not null)
            {
                string? currentSection = null;
                foreach (var node in bodyNode.ChildNodes)
                {
                    if (node.Name == "h2")
                    {
                        currentSection = node.InnerText.Trim();
                        continue;
                    }

                    if (node.Name == "ul")
                    {
                        foreach (var li in node.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
                        {
                            var text = li.InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(text))
                                commitNotes.Add(currentSection is not null ? $"[{currentSection}] {text}" : text);
                        }
                    }
                }
            }
            
            await _logService.LogInfoAsync($"Successfully parsed snapshot: {date.Value}\n{string.Join(Environment.NewLine, commitNotes)}");
            _availableRenoDxTags[RenoDX.Branch.Snapshot] = [new RenoDXTagInfoDto(date.Value, RenoDX.Branch.Snapshot, commitNotes)];
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch latest RenoDX snapshot", ex);
        }
    }

    private async Task FetchRenoDxNightlyVersionsAsync()
    {
        throw new System.NotImplementedException();
    }
    
    private async Task PersistPreferencesAsync()
    {
        throw new System.NotImplementedException();
    }
    
    private async Task LoadPreferencesAsync()
    {
        throw new System.NotImplementedException();
    }
}