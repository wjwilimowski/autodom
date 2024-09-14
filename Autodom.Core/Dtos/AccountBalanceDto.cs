namespace Autodom.Core.Dtos;

public record AccountBalanceDto
{
    public required decimal Balance { get; init; }
    public DateTime LastChangedDateTime { get; set; }
}