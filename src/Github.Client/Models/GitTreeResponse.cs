namespace Github.Client.Models;

public record GitTreeResponse
{
    public GitTreeItem[]? Tree { get; init; }
}