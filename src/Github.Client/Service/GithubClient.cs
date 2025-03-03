using Github.Client.Interface;
using Github.Client.Models;
using System.Net.Http.Json;
using Github.Client.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Github.Client.Service;
public class GithubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;
    private readonly string _owner;
    private readonly string _repo;
    private readonly ILogger<GithubClient> _logger;
    private readonly bool _saveFile;

    public GithubClient(HttpClient httpClient, string owner, string repo, ILogger<GithubClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _logger = logger;
    }


    // get all commits since a given date
    public async Task<List<GitCommit>> GetCommitsSince(DateTime sinceDate)
    {

        List<GitCommit> commits = [];
        int page = 1;
        while (true)
        {
            try
            {
                string url = GithubUrl.GetCommitsSince(_owner, _repo, sinceDate, page);
                IEnumerable<GitCommit>? response = await _httpClient.GetFromJsonAsync<IEnumerable<GitCommit>>(url);
                _logger?.LogInformation("Fetching commits since {sinceDate} - Current Count: {count} - Page: {page}", sinceDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), commits.Count, page);
                if (response == null || !response.Any())
                {
                    _logger?.LogInformation("No more commits to fetch - Page: {page}", page);
                    break;
                }
                ;
                commits.AddRange(response);
                commits = commits.DistinctBy(c => c.Sha).ToList();
                page++;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error fetching commits since {sinceDate}: {ex.Message}", sinceDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), ex.Message);
                break;
            }
        }
        return commits;
    }
    public async Task<List<JsonElement>> GetCommitsSinceJson(DateTime sinceDate)
    {

        List<JsonElement> commits = [];
        int page = 1;
        while (true)
        {
            try
            {
                string url = GithubUrl.GetCommitsSince(_owner, _repo, sinceDate, page);
                IEnumerable<JsonElement>? response = await _httpClient.GetFromJsonAsync<IEnumerable<JsonElement>>(url);
                _logger?.LogInformation("Fetching commits since {sinceDate} - Current Count: {count} - Page: {page}", sinceDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), commits.Count, page);
                if (response == null || !response.Any())
                {
                    _logger?.LogInformation("No more commits to fetch - Page: {page}", page);
                    break;
                }
                ;
                commits.AddRange(response);
                page++;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error fetching commits since {sinceDate}: {ex.Message}", sinceDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), ex.Message);
                break;
            }
        }
        return commits;
    }
    // get all mappings and json of cardano token metadata
    public async Task<IEnumerable<GitCommit>> GetAllCommits(CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        IEnumerable<GitCommit>? commits = [];

        string url = GithubUrl.GetAllCommits(_owner, _repo);
        _logger?.LogInformation("Fetching all commits from {url}", url);
        try
        {
            commits = await _httpClient
            .GetFromJsonAsync<IEnumerable<GitCommit>>(
                url,
                token
            );
            if (commits is null || !commits.Any())
            {
                _logger?.LogError("Repo: {repo} Owner: {owner} has no commits!", _owner, _repo);
                throw new Exception("No commits found");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching all commits: {ex.Message}", ex.Message);
        }
        return commits!;
    }
    public async Task<GitCommit> GetResolvedCommit(string commitUrl, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        string url = GithubUrl.GetResolvedCommit(commitUrl);
        try
        {
            GitCommit? commit = await _httpClient.GetFromJsonAsync<GitCommit>(url, token);
            if (commit is null || commit.Files is null || !commit.Files.Any())
            {
                _logger?.LogError("Resolved commit is null for url: {commitUrl}", commitUrl);
                throw new Exception("Resolved commit is null");
            }
            return commit;
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching resolved commit: {ex.Message}", ex.Message);
            throw new Exception("Error fetching resolved commit", ex);
        }
    }
    public async Task<JsonElement> GetResolvedCommitJson(string commitUrl, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        string url = GithubUrl.GetResolvedCommit(commitUrl);
        try
        {
            JsonElement? commit = await _httpClient.GetFromJsonAsync<JsonElement>(url, token);
            if (commit is null)
            {
                _logger?.LogError("Resolved commit is null for url: {commitUrl}", commitUrl);
                throw new Exception("Resolved commit is null");
            }
            return commit.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching resolved commit: {ex.Message}", ex.Message);
            throw new Exception("Error fetching resolved commit", ex);
        }
    }
    public async Task<GitCommit> GetLatestCommit(CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        IEnumerable<GitCommit>? commits = [];
        GitCommit latestCommit = new GitCommit();

        string url = GithubUrl.GetAllCommits(_owner, _repo);
        _logger?.LogInformation("Fetching all commits from {url}", url);
        try
        {
            commits = await _httpClient
            .GetFromJsonAsync<IEnumerable<GitCommit>>(
                url,
                token
            );
            if (commits is null || !commits.Any())
            {
                _logger?.LogError("Repo: {repo} Owner: {owner} has no commits!", _owner, _repo);
                throw new Exception("No commits found");
            }

            latestCommit = commits.First();
            if (latestCommit is null || latestCommit.Sha is null)
            {
                _logger?.LogError("Latest commit is null");
                throw new Exception("Latest commit is null");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching all commits: {ex.Message}", ex.Message);
            throw new Exception("Error getting latest commit", ex);
        }
        return latestCommit;

    }
    public async Task<JsonElement> GetLatestCommitJson(CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        IEnumerable<JsonElement>? commits = [];
        JsonElement latestCommit = new JsonElement();

        string url = GithubUrl.GetAllCommits(_owner, _repo);
        _logger?.LogInformation("Fetching all commits from {url}", url);
        try
        {
            commits = await _httpClient
            .GetFromJsonAsync<IEnumerable<JsonElement>>(
                url,
                token
            );
            if (commits is null || !commits.Any())
            {
                _logger?.LogError("Repo: {repo} Owner: {owner} has no commits!", _owner, _repo);
                throw new Exception("No commits found");
            }

            latestCommit = commits.First();
            if (latestCommit.GetProperty("sha").GetString() is null)
            {
                _logger?.LogError("Latest commit is null");
                throw new Exception("Latest commit is null");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching all commits: {ex.Message}", ex.Message);
            throw new Exception("Error getting latest commit", ex);
        }
        return latestCommit;

    }
    public async Task<GitTreeResponse> GetTree(string sha, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        string url = GithubUrl.GetTree(_owner, _repo, sha);
        GitTreeResponse? treeResponse = null;
        try
        {
            treeResponse = await _httpClient.GetFromJsonAsync<GitTreeResponse>(url, token);
            if (treeResponse is null || treeResponse.Tree is null || !treeResponse.Tree.Any())
            {
                _logger?.LogError("Tree response is null for url: {url} sha: {sha}", url, sha);
                throw new Exception("Tree response is null");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching tree: {ex.Message}", ex.Message);
            throw new Exception("Error fetching tree", ex);
        }
        return treeResponse;

    }
    public async Task<JsonElement> GetTreeJson(string sha, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        string url = GithubUrl.GetTree(_owner, _repo, sha);
        JsonElement? treeResponse = null;
        try
        {
            treeResponse = await _httpClient.GetFromJsonAsync<JsonElement>(url, token);
            if (treeResponse is null)
            {
                _logger?.LogError("Tree response is null for url: {url} sha: {sha}", url, sha);
                throw new Exception("Tree response is null");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error fetching tree: {ex.Message}", ex.Message);
            throw new Exception("Error fetching tree", ex);
        }
        return treeResponse.Value;

    }
    public async Task<GitCommitFile> GetMappingFromFileName(string sha, string filename, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        string url = GithubUrl.GetJsonMappingFileName(_owner, _repo, sha, filename);
        try
        {
            GitCommitFile? mappingFile = await _httpClient.GetFromJsonAsync<GitCommitFile>(url, token);
            if (mappingFile is null)
            {
                _logger?.LogError("GetMappingFromFileName Mapping file is null for url: {url} sha: {sha} filename: {filename}", url, sha, filename);
                throw new Exception("Mapping file is null");
            }
            return mappingFile;
        }
        catch (Exception ex)
        {
            _logger?.LogError("GetMappingFromFileName Error fetching mapping file: {ex.Message}", ex.Message);
            throw new Exception("GetMappingFromFileName Error fetching mapping file", ex);
        }
    }
        public async Task<JsonElement> GetMappingFromFileNameJson(string sha, string filename, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        string url = GithubUrl.GetJsonMappingFileName(_owner, _repo, sha, filename);
        try
        {
            JsonElement? mappingFile = await _httpClient.GetFromJsonAsync<JsonElement>(url, token);
            if (mappingFile is null)
            {
                _logger?.LogError("GetMappingFromFileNameJson Mapping file is null for url: {url} sha: {sha} filename: {filename}", url, sha, filename);
                throw new Exception("Mapping file is null");
            }
            return mappingFile.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogError("GetMappingFromFileNameJson Error fetching mapping file: {ex.Message}", ex.Message);
            throw new Exception("GetMappingFromFileNameJson Error fetching mapping file", ex);
        }
    }
    public async Task<GitCommitFile> GetMappingFromPath(string sha, string path, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        try
        {
            string url = GithubUrl.GetJsonMappingPath(_owner, _repo, sha, path);
            GitCommitFile? mappingFile = await _httpClient.GetFromJsonAsync<GitCommitFile>(url, token);
            if (mappingFile is null)
            {
                _logger?.LogError("GetMappingFromPath Mapping file is null for url: {url} sha: {sha} path: {path}", url, sha, path);
                throw new Exception("Mapping file is null");
            }
            return mappingFile;
        }
        catch (Exception ex)
        {
            _logger?.LogError("GetMappingFromPath Error fetching mapping file: {ex.Message}", ex.Message);
            throw new Exception("GetMappingFromPath Error fetching mapping file", ex);
        }
    }
        public async Task<JsonElement> GetMappingFromPathJson(string sha, string path, CancellationToken? stoppingToken)
    {
        CancellationToken token = stoppingToken ?? CancellationToken.None;
        try
        {
            string url = GithubUrl.GetJsonMappingPath(_owner, _repo, sha, path);
            JsonElement? mappingFile = await _httpClient.GetFromJsonAsync<JsonElement>(url, token);
            if (mappingFile is null)
            {
                _logger?.LogError("GetMappingFromPathJson Mapping file is null for url: {url} sha: {sha} path: {path}", url, sha, path);
                throw new Exception("Mapping file is null");
            }
            return mappingFile.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogError("GetMappingFromPathJson Error fetching mapping file: {ex.Message}", ex.Message);
            throw new Exception("GetMappingFromPathJson Error fetching mapping file", ex);
        }
    }


    // Get count of commits since a given date
    public async Task<int> GetTotalCommits(DateTime sinceDate)
    {
        List<GitCommit> commits = [];
        int page = 1;
        while (true)
        {
            _logger?.LogInformation("current time: {time}", DateTime.Now.ToString("HH:mm:ss"));
            string url = GithubUrl.GetCommitsSince(_owner, _repo, sinceDate, page);
            try
            {
                IEnumerable<GitCommit>? response = await _httpClient.GetFromJsonAsync<IEnumerable<GitCommit>>(url);
                if (response == null || !response.Any()) break;
                _logger?.LogInformation("Fetching commits since {sinceDate} - Current Count: {count} - Page: {page}", sinceDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), commits.Count, page);

                commits.AddRange(response);
                page++;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error fetching commits: {ex.Message}", ex.Message);
                break;
            }
        }
        return commits.Count;
    }

}
