using Newtonsoft.Json;

namespace Autodom.Core.Dtos;

public record AccountBalanceDto
{
    [JsonProperty("id")]
    public string Id { get; init; }
    public string AccountId { get; init; }
    public required decimal Balance { get; init; }
    public DateTime LastChangedDateTime { get; set; }
}