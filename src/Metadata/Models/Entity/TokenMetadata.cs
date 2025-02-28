
namespace Metadata.Models.Entity;

public record TokenMetadata
{
    public string Subject { get; init; } = string.Empty;
    
    public string Name { get; init; } = string.Empty;
    
    public string Description { get; init; } = string.Empty;
    
    public string Policy { get; init; } = string.Empty;
    
    public string Ticker { get; init; } = string.Empty;
    
    public string Url { get; init; } = string.Empty;
    
    public string Logo { get; init; } = string.Empty;
    
    public int Decimals { get; init; }
    
    public byte[] Data { get; init; } = Array.Empty<byte>();
}
