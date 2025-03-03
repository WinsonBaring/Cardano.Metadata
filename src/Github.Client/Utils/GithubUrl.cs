using System.Web;

namespace Github.Client.Utils;
public static class GithubUrl
{
    private static readonly string githubApiUrl = "https://api.github.com";
    private static readonly string githubRawUrl = "https://raw.githubusercontent.com";
    public static string GetCommitsSince(string owner, string repo, DateTime sinceDate, int page)
    {
        try
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                throw new ArgumentException("Owner and repo cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(githubApiUrl);
            urlBuilder.Path = $"/repos/{owner}/{repo}/commits";
            urlBuilder.Query = $"since={sinceDate:yyyy-MM-ddTHH:mm:ssZ}&page={page}";
            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting commits since", ex);
        }
    }

    public static string GetCommitsSince(string owner, string repo, DateTime sinceDate)
    {
        try
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                throw new ArgumentException("Owner and repo cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(githubApiUrl);
            urlBuilder.Path = $"/repos/{owner}/{repo}/commits";
            urlBuilder.Query = $"since={sinceDate:yyyy-MM-ddTHH:mm:ssZ}";

            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting commits since", ex);
        }
    }
    public static string GetAllCommits(string owner, string repo)
    {
        try
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                throw new ArgumentException("Owner and repo cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(githubApiUrl);
            urlBuilder.Path = $"/repos/{owner}/{repo}/commits";
            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting all commits", ex);
        }
    }
    public static string GetTree(string owner, string repo, string sha)
    {
        try
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(sha))
            {
                throw new ArgumentException("Owner, repo and sha cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(githubApiUrl);
            urlBuilder.Path = $"/repos/{owner}/{repo}/git/trees/{sha}";
            urlBuilder.Query = "recursive=true";
            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting tree", ex);
        }
    }
    public static string GetJsonMappingPath(string owner, string repo, string sha, string path)
    {
        try
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(sha) || string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Owner, repo, sha and path cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(githubRawUrl);
            urlBuilder.Path = $"/{owner}/{repo}/{sha}/{path}";

            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting json mapping file", ex);
        }
    }
    public static string GetResolvedCommit(string commitUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(commitUrl))
            {
                throw new ArgumentException("Commit url cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(commitUrl);
            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting resolved commit", ex);
        }
    }
    public static string GetJsonMappingFileName(string owner, string repo, string sha, string filename)
    {
        try
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(sha) || string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Owner, repo, sha and filename cannot be null or empty");
            }
            UriBuilder urlBuilder = new UriBuilder(githubRawUrl);
            urlBuilder.Path = $"/{owner}/{repo}/{sha}/{filename}";
            return urlBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting json mapping file", ex);
        }
    }

}
