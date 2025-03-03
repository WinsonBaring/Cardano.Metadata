using Github.Client.Interface;
using Github.Client.Models;
using System.Text.Json;
using System.Runtime.CompilerServices;  
namespace Metadata.Workers;

public class NewGithubWorker : BackgroundService
{
    private readonly IGitHubClient _githubClient;
    private readonly ILogger<NewGithubWorker> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    // private readonly IDbContextFactory<TokenMetaDataDbContext> _dbContextFactory;
    public NewGithubWorker(
        IGitHubClient githubClient, 
        ILogger<NewGithubWorker> logger, 
        HttpClient httpClient, 
        // IDbContextFactory<TokenMetaDataDbContext> dbContextFactory, 
        IConfiguration config)
    {
        _githubClient = githubClient;
        _logger = logger;
        _httpClient = httpClient;
        _config = config;
        // _dbContextFactory = dbContextFactory;
    }
    


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if(false){
                JsonElement latestCommitJson = await _githubClient.GetLatestCommitJson(stoppingToken);
                string sha = latestCommitJson.GetProperty("sha").GetString()!;
                SaveLatestCommit(latestCommitJson,"latest_commit_json");

                JsonElement treeResponseJson = await _githubClient.GetTreeJson(sha, stoppingToken);
                SaveLatestCommit(treeResponseJson,"tree_response_json");

                // treeResponseJson is an object like this { {} {} }
                if (treeResponseJson.TryGetProperty("tree", out JsonElement treeArray)){
                    SaveLatestCommit(treeArray,"tree_array");

                    foreach (JsonElement treeItem in treeArray.EnumerateArray()){
                        JsonElement path = treeItem.GetProperty("path");
                        if (path.GetString() is null || !path.GetString()!.StartsWith("mappings/") || !path.GetString()!.EndsWith(".json")) continue;
                        string subject = GetSubjectFromPath(path.GetString()!);

                        JsonElement mappingFile = await _githubClient.GetMappingFromPathJson(sha, treeItem.GetProperty("path").GetString()!, stoppingToken);
                        SaveLatestCommit(mappingFile,"mapping_file_json",subject);
                        
                    }
                }

            }else{
                // filter to unique commits
                List<JsonElement> latestCommitsSinceJson = await _githubClient.GetCommitsSinceJson(DateTime.UtcNow.AddDays(-5));
                SaveLatestCommit(latestCommitsSinceJson,"latest_commits_since_json");

                foreach (JsonElement commit in latestCommitsSinceJson)
                {
                    string url = commit.GetProperty("url").GetString()!;
                    string sha = commit.GetProperty("sha").GetString()!;
                    JsonElement resolvedCommitJson = await _githubClient.GetResolvedCommitJson(url, stoppingToken);
                    string files = resolvedCommitJson.GetProperty("files").ToString() ?? throw new Exception("Files is null");
                    SaveLatestCommit(resolvedCommitJson,"resolved_commit_json");
                    if (resolvedCommitJson.TryGetProperty("files", out JsonElement filesArray)){
                        foreach (JsonElement file in filesArray.EnumerateArray())
                        {
                            string filename = file.GetProperty("filename").GetString()!;
                            if(string.IsNullOrEmpty(filename) || !filename.StartsWith("mappings/") || !filename.EndsWith(".json")) {
                                _logger.LogError("File name is null or not a mapping file for commit: {commit.Sha}", sha);
                                continue;
                            };
                            string subject = GetSubjectFromPath(filename);

                            JsonElement mappingFileJson = await _githubClient.GetMappingFromFileNameJson(sha, filename, stoppingToken);
                            SaveLatestCommit(mappingFileJson,"mapping_file_json",subject);
                        }
                        
                    }
                }
                // _logger.LogInformation("Hey dude, Total commits: {totalCommits}", totalCommits);
                
            }
            await Task.Delay(10000, stoppingToken);
        }
    }
    // Private helper function to save the commit
    private void SaveLatestCommit(object commit,string commitVariableName,string filename = "latest.json" )
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string dataDirectory = Path.Combine(currentDirectory, "data", commitVariableName);
        
        Directory.CreateDirectory(dataDirectory);

        string latestCommitPath = Path.Combine(dataDirectory, $"{filename}.json");

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
    
    private string GetSubjectFromPath(string path){
        return path
            .Replace("mappings/", string.Empty)
            .Replace(".json", string.Empty);
    }
}