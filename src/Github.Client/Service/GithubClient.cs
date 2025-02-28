using Github.Client.Interface;
using Github.Client.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Reflection;


namespace Github.Client.Service;
public class GithubClient : IGitHubClient
{
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _githubPAT;

    public GithubClient(string owner, string repo, string githubPAT)
    {
        _owner = owner;
        _repo = repo;
        _githubPAT = githubPAT;
    }


    // https://api.github.com/repos/cardano-foundation/cardano-token-registry/commits?since=2023-01-01T00:00:00Z&page=1
    // Get all commits since a given dat
    public async Task<List<GitCommit>> GetCommitsSince(DateTime sinceDate)
    {
        HttpClient _httpClient = new HttpClient();
        ProductInfoHeaderValue productValue = new("CardanoTokenMetadataService", Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown Version");
        ProductInfoHeaderValue commentValue = new("(+https://github.com/SAIB-Inc/Cardano.Metadata)");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(productValue);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(commentValue);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubPAT);

        int page = 1;
        List<GitCommit> commits = [];
        while (true)
        {
            var url = $"https://api.github.com/repos/{_owner}/{_repo}/commits?since={sinceDate:yyyy-MM-ddTHH:mm:ssZ}&page={page}";
            IEnumerable<GitCommit>? commitPage = await _httpClient.GetFromJsonAsync<IEnumerable<GitCommit>>(url);
            if (commitPage == null || !commitPage.Any())break;
            commits.AddRange(commitPage);
            page++;
        }
        return commits;
    }

}
