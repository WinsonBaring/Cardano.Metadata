namespace Metadata.Models.Github;

public record GitTreeResponse
{
    public GitTreeItem[]? Tree { get; init; }
}