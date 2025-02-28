using Metadata.Models.Entity;

class GithubWorker
{
    private readonly HttpClient _httpClient;

    public GithubWorker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    
}