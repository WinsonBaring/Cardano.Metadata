namespace Github.Client.Models;

public record GitHubApiOptions
{
    public string BaseUrl { get; set; } = string.Empty; // Provide a default (though it will likely be overridden)
    public string Token { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}