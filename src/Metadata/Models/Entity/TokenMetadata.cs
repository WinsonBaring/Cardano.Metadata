using System.Text.Json;

namespace Metadata.Models.Entity;

public record TokenMetadata
{
    public string Subject { get; init; } = string.Empty;
    public JsonElement Data { get; set; }
}