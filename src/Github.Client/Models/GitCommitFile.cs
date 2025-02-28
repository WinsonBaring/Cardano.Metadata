using System.Text.Json.Serialization;

namespace Github.Client.Models;

public record GitCommitFile
{
    public string? Filename { get; init; }

    [JsonPropertyName("raw_url")]
    public string? RawUrl { get; init; }
}