using System.Text.Json.Serialization;

namespace Metadata.Models.Github;

public record GitCommitFile
{
    public string? Filename { get; init; }

    [JsonPropertyName("raw_url")]
    public string? RawUrl { get; init; }
}