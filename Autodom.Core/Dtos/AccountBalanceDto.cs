namespace Autodom.Core.Dtos;

public record AccountBalanceDto
{
    public string Id { get; init; }
    public int AccountId { get; init; }
    public required decimal Balance { get; init; }
    public DateTime LastChangedDateTime { get; set; }
}