using Github.Client.Models;
using System.Text.Json;
namespace Github.Client.Interface;

public interface IGitHubClient
{
    Task<List<GitCommit>> GetCommitsSince(DateTime since);
    Task<List<JsonElement>> GetCommitsSinceJson(DateTime since);
    Task<IEnumerable<GitCommit>> GetAllCommits(CancellationToken? stoppingToken = null);
    Task<int> GetTotalCommits(DateTime since);
    Task<GitTreeResponse> GetTree(string sha, CancellationToken? stoppingToken = null);
    Task<JsonElement> GetTreeJson(string sha, CancellationToken? stoppingToken = null);
    Task<GitCommit> GetLatestCommit(CancellationToken? stoppingToken = null);
    Task<JsonElement> GetLatestCommitJson(CancellationToken? stoppingToken = null);
    Task<GitCommitFile> GetMappingFromPath(string sha, string path, CancellationToken? stoppingToken = null);
    Task<GitCommit> GetResolvedCommit(string commitUrl, CancellationToken? stoppingToken = null);
    Task<JsonElement> GetResolvedCommitJson(string commitUrl, CancellationToken? stoppingToken = null);
    Task<GitCommitFile> GetMappingFromFileName(string sha, string filename, CancellationToken? stoppingToken = null);
    Task<JsonElement> GetMappingFromFileNameJson(string sha, string filename, CancellationToken? stoppingToken = null);
    Task<JsonElement> GetMappingFromPathJson(string sha, string path, CancellationToken? stoppingToken = null);
    
}