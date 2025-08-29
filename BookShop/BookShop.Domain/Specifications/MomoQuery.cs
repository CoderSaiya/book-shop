using System.Text.Json.Serialization;

namespace BookShop.Domain.Specifications;

public class MomoQuery
{
    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("localMessage")] 
    public string? LocalMessage { get; set; }
}