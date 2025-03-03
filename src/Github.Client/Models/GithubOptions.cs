namespace Github.Client.Models;
public record GithubOptions
{
    public string GithubClient { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
    public string PAT { get; set; } = string.Empty;
}