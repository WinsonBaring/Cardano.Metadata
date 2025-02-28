using Github.Client.Models;

namespace Github.Client.Interface;

public interface IGitHubClient
{
    Task<List<GitCommit>> GetCommitsSince(DateTime since);
    // Task<string> GetCommitsSince(DateTime since);
}