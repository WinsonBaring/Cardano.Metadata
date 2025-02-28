using Github.Client.Interface;
using Github.Client.Models;
using System.Net.Http.Headers;
using System.Reflection;

namespace Metadata.Workers;

public class GithubWorker : BackgroundService
{
    private readonly IGitHubClient _githubClient;
    private readonly ILogger<GithubWorker> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    public GithubWorker(IGitHubClient githubClient, ILogger<GithubWorker> logger, HttpClient httpClient, IConfiguration config)
    {
        _githubClient = githubClient;
        _logger = logger;
        _httpClient = httpClient;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ProductInfoHeaderValue productValue = new("CardanoTokenMetadataService", Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown Version");
        ProductInfoHeaderValue commentValue = new("(+https://github.com/SAIB-Inc/Cardano.Metadata)");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(productValue);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(commentValue);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Github:PAT"]);
        var since = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var formattedSince = since.ToString("yyyy-MM-ddTHH:mm:ssZ");
        DateTime sinceDate = DateTime.Parse(formattedSince);
        var page = 1;
        
        while (!stoppingToken.IsCancellationRequested)
        {

            // List<GitCommit> commits = await _githubClient.GetCommitsSince(new DateTime(2024, 1, 1));
            // var url = $"https://api.github.com/repos/{_owner}/{_repo}/commits?since={since:yyyy-MM-ddTHH:mm:ssZ}&page={page}";
            var url = $"https://api.github.com/repos/{_config["Github:Owner"]}/{_config["Github:Repo"]}/commits?since={since:yyyy-MM-ddTHH:mm:ssZ}&page={page}";
            
            // IEnumerable<GitCommit>? response = await _httpClient.GetFromJsonAsync<IEnumerable<GitCommit>>(url, cancellationToken: stoppingToken);
                _logger.LogInformation("These arethe commits: {response}");
            List<GitCommit>? commits = null;
            try{
                commits = await _githubClient.GetCommitsSince(sinceDate);
                _logger.LogInformation("These are the commits: {response}", commits);
            }
            catch(Exception ex){
                _logger.LogError("Error getting commits since {since}: {ex.Message}", since, ex.Message);
            }

            _logger.LogInformation("These are the commits: {response}", commits);

            await Task.Delay(4000, stoppingToken);
        }
        
    }
}