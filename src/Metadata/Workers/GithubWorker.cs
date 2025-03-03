using Github.Client.Interface;
using Github.Client.Models;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Metadata.Data;
using Metadata.Models.Entity;
namespace Metadata.Workers;

public class GithubWorker : BackgroundService
{
    private readonly IGitHubClient _githubClient;
    private readonly ILogger<GithubWorker> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IDbContextFactory<TokenMetaDataDbContext> _dbContextFactory;
    public GithubWorker(
        IGitHubClient githubClient,
        ILogger<GithubWorker> logger,
        HttpClient httpClient,
        IDbContextFactory<TokenMetaDataDbContext> dbContextFactory,
        IConfiguration config)
    {
        _githubClient = githubClient;
        _logger = logger;
        _httpClient = httpClient;
        _config = config;
        _dbContextFactory = dbContextFactory;
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using TokenMetaDataDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(stoppingToken);
        SyncState? syncState = await dbContext.SyncStates.OrderByDescending(ss => ss.Date).FirstOrDefaultAsync(cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (true)
            {
                GitCommit latestCommit = await _githubClient.GetLatestCommit(stoppingToken);
                SaveFile(latestCommit, "latest_commit");
                GitTreeResponse treeResponse = await _githubClient.GetTree(latestCommit.Sha!, stoppingToken);
                SaveFile(treeResponse, "tree_response");
                foreach (GitTreeItem item in treeResponse.Tree!)
                {
                    if (item.Path is null || !item.Path.StartsWith("mappings/") || !item.Path.EndsWith(".json")) continue;
                    string subject = GetSubjectFromPath(item.Path);

                    GitCommitFile mappingFile = await _githubClient.GetMappingFromPath(latestCommit.Sha!, item.Path, stoppingToken);
                    SaveFile(mappingFile, "mapping_file", item.Path);

                    // await dbContext.TokenMetadatas.AddAsync(new()
                    // {
                    //     Subject = subject,
                    //     Data = JsonSerializer.SerializeToUtf8Bytes(mappingFile)
                    // }, stoppingToken);
                }
                // await dbContext.SyncStates.AddAsync(new()
                // {
                //     // @TODO handle null from API???
                //     Sha = latestCommit.Sha ?? string.Empty,
                //     Date = latestCommit.Commit?.Author?.Date ?? DateTime.UtcNow
                // }, stoppingToken);
                // await dbContext.SaveChangesAsync(stoppingToken);
            }
            else
            {
                // filter to unique commits
                List<GitCommit> latestCommitsSince = await _githubClient.GetCommitsSince(DateTime.UtcNow.AddDays(-5));
                SaveFile(latestCommitsSince, "latest_commits");
                foreach (GitCommit commit in latestCommitsSince)
                {
                    GitCommit resolvedCommit = await _githubClient.GetResolvedCommit(commit.Url!, stoppingToken);
                    SaveFile(resolvedCommit, "resolved_commit");

                    foreach (GitCommitFile file in resolvedCommit.Files!)
                    {
                        if (string.IsNullOrEmpty(file.Filename) || !file.Filename.StartsWith("mappings/") || !file.Filename.EndsWith(".json"))
                        {
                            _logger.LogError("File name is null or not a mapping file for commit: {commit.Sha}", commit.Sha);
                            continue;
                        }
                        ;
                        string subject = GetSubjectFromPath(file.Filename!);

                        GitCommitFile mappingFile = await _githubClient.GetMappingFromFileName(resolvedCommit.Sha!, file.Filename, stoppingToken);
                        SaveFile(mappingFile, "mapping_file", file.Filename);
                    }
                }
            }
            await Task.Delay(10000, stoppingToken);
        }
    }

    // Private helper function to save the commit
    private void SaveFile(object commit, string commitVariableName, string filename = "latest.json")
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string dataDirectory = Path.Combine(currentDirectory, "data", commitVariableName);

        Directory.CreateDirectory(dataDirectory);
        string path = Path.GetFileNameWithoutExtension(filename);

        string latestCommitPath = Path.Combine(dataDirectory, $"{path}.json");

        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true }; // Pretty-print the JSON
            string jsonString = JsonSerializer.Serialize(commit, options);
            File.WriteAllText(latestCommitPath, jsonString);
            _logger.LogInformation("Latest commit saved to: {FilePath}", latestCommitPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the latest commit to {FilePath}", latestCommitPath);
            throw; // Re-throw after logging
        }
    }

    private string GetSubjectFromPath(string path)
    {
        return path
            .Replace("mappings/", string.Empty)
            .Replace(".json", string.Empty);
    }
}